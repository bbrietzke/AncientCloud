using System;
using System.Data;
using System.Data.Common;

namespace AncientCloud.EventStore;

public sealed class DatabaseCreation
{
    private readonly IDbConnection _connection;
    private readonly string _eventTableSQL =
        @"CREATE TABLE IF NOT EXISTS Events(
            Id          UUID        NOT NULL PRIMARY KEY,
            Data        BLOB        NOT NULL,
            StreamId    UUID        NOT NULL,
            Type        TEXT        NOT NULL,
            Version     INT         NOT NULL,
            CreatedOn   TIMESTAMP   DEFAULT CURRENT_TIMESTAMP,
            FOREIGN KEY(StreamId) REFERENCES Streams(Id)
        );";
    private readonly string _eventStreamsSQL =
        @"CREATE TABLE IF NOT EXISTS Streams(
            Id          UUID        NOT NULL PRIMARY KEY,
            Type        TEXT        NOT NULL,
            Version     BIGINT      NOT NULL
        );";

    public DatabaseCreation(IDbConnection connection)
    {
        this._connection = connection;
    }

    public void CreateEventTable()
    {
        using (DbCommand cmd = (DbCommand)this._connection.CreateCommand())
        {
            if (this._connection.State != ConnectionState.Open)
            {
                this._connection.Open();
            }

            cmd.CommandText = this._eventStreamsSQL;
            cmd.ExecuteNonQuery();
        }
    }

    public void CreateStreamsTable()
    {
        using (DbCommand cmd = (DbCommand)this._connection.CreateCommand())
        {
            if (this._connection.State != ConnectionState.Open)
            {
                this._connection.Open();
            }

            cmd.CommandText = this._eventTableSQL;
            cmd.ExecuteNonQuery();
        }
    }

}
