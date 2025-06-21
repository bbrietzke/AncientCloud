using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Transactions;
using Microsoft.VisualBasic;

using Newtonsoft.Json;
using System.Linq;

namespace AncientCloud.EventStore;

public sealed class EventStore : IEventStore
{
    private readonly Dictionary<Type, List<IProjection>> _projections = new();
    private readonly DbConnection _connection;

    public EventStore(DbConnection connection)
    {
        this._connection = connection;
    }

    public void Init()
    {
        DatabaseCreation creator = new DatabaseCreation(this._connection);
        creator.CreateStreamsTable();
        creator.CreateEventTable();

        foreach (var projection in this._projections.Values.SelectMany(p => p))
        {
            projection.Init();
        }
    }

    public void RegisterProjection(IProjection projection)
    {
        foreach (var eventType in projection.Handles)
        {
            if (!this._projections.ContainsKey(eventType))
            {
                this._projections[eventType] = new List<IProjection>();
            }

            _projections[eventType].Add(projection);
        }
    }

    public Task AppendEvents<TStream>(Guid streamId, IEnumerable<object> events, long expectedVersion, CancellationToken ct = default) where TStream : notnull
    {
        return Task.Factory.StartNew(async () =>
        {
            using (DbTransaction trx = this._connection.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                foreach(var @event in events)
                {
                    DbCommand cmd = this._connection.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    string cmdTxt = String.Format(
                                        this.createAppendFunction,
                                        streamId,
                                        typeof(TStream).AssemblyQualifiedName,
                                        Guid.NewGuid(),
                                        @event.GetType().AssemblyQualifiedName,
                                        expectedVersion++,
                                        JsonConvert.SerializeObject(@event)
                                    );
                    Console.WriteLine(cmdTxt);
                    cmd.CommandText = cmdTxt;
                    int records = cmd.ExecuteNonQuery();

                    await this.ApplyProjections(@event, ct);
                }

                if (ct.IsCancellationRequested)
                {
                    trx.Rollback();
                }
                else
                {
                    trx.Commit();
                }
            }
        });
    }

    public Task<IReadOnlyCollection<object>> GetEventsAsync(Guid streamId, long? atVersion = null, DateTime? atTimestamp = null, CancellationToken ct = default)
    {
        return Task.Factory.StartNew<IReadOnlyCollection<object>>(() =>
        {
            List<object> retVal = new List<object>();
            string version = atVersion.HasValue ? String.Format("AND Version <= {0}", atVersion.GetValueOrDefault()).ToString() : String.Empty;
            string timey = atTimestamp.HasValue ? String.Format("AND CreatedOn <= {0}", atTimestamp.GetValueOrDefault()) : String.Empty;
            string cmdTxt = String.Format("SELECT Id, Data, StreamId, Type, Version FROM Events WHERE StreamId = '{0}' {1} {2};", streamId, version, timey);

            DbCommand cmd = this._connection.CreateCommand();
            cmd.CommandText = cmdTxt;
            cmd.CommandType = CommandType.Text;

            DbDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

            while (reader.Read())
            {
                string value = reader.GetString("Data");

                var obj1 = JsonConvert.DeserializeObject(value, Type.GetType(reader.GetString("Type")));
                retVal.Add(obj1);
            }

            return (IReadOnlyCollection<object>)retVal;
        });
    }

    private async Task ApplyProjections(object @event, CancellationToken ct)
    {
        if (!this._projections.ContainsKey(@event.GetType()))
        {
            return;
        }

        foreach (var p in this._projections[@event.GetType()])
        {
            await p.Handle(@event, ct);
        }
    }

    private readonly string createAppendFunction = @"
        WITH CURRENT AS (
            SELECT CASE WHEN 
                EXISTS (
                    SELECT 1 FROM STREAMS WHERE Id = '{0}'
                ) THEN (
                    select version + 1 from streams where id = '{0}'
                ) ELSE (
                    1
                ) END AS Version,
            '{1}' AS Type,
            '{0}' AS Id
        ) INSERT INTO Streams (Version, Type, Id)
            SELECT Version, Type, Id
            FROM CURRENT
            WHERE 1 = 1 
            ON CONFLICT(Id) DO UPDATE
                SET Version = Version + 1;

        INSERT INTO Events (
            Id, StreamId, Type, Version, Data
        ) VALUES (
            '{2}', '{0}', '{3}', {4}, json('{5}')
        ); 
    ";
} 



