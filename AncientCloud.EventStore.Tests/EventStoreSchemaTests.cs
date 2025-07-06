using System.Data;
using Microsoft.Data.Sqlite;

using AncientCloud.EventStore;
using System.Data.Common;

namespace AncientCloud.EventStore.Tests;

[TestClass]
public sealed class EventStoreSchemaTests
{
    [TestMethod]
    public void TablesAreCreated()
    {
        using SqliteConnection con = new SqliteConnection("Data Source=:memory:");
        con.Open();
        IEventStore store = new EventStore(con);
        store.Init();

        Assert.IsTrue(doesTableExist("Events", con));
        Assert.IsTrue(doesTableExist("Streams", con));
    }

    [TestMethod]
    [ExpectedException(typeof(SqliteException))]
    public void VersionStreamConstraints()
    {
        using DbConnection con = new SqliteConnection("Data Source=:memory:");
        con.Open();
        IEventStore store = new EventStore(con);
        store.Init();

        IDbCommand cmd1 = con.CreateCommand();
        cmd1.CommandText = "INSERT INTO Streams(Id, Type, Version) VALUES ('00000000-0000-0000-0000-000000000000', 'Test Stream', 0)";
        cmd1.ExecuteNonQuery();

        IDbCommand cmd2 = con.CreateCommand();
        cmd2.CommandText = "INSERT INTO Events(Id, Version, StreamId, Type, Data) VALUES ('571ad9a2-2a99-48fa-9efd-c9345989d545', 0, '00000000-0000-0000-0000-000000000000', 'TEST', '')";
        cmd2.ExecuteNonQuery();

        IDbCommand cmd3 = con.CreateCommand();
        cmd3.CommandText = "INSERT INTO Events(Id, Version, StreamId, Type, Data) VALUES ('571ad9a2-2a99-48fa-9efd-c9345989d545', 0, '00000000-0000-0000-0000-000000000000', 'TEST', '')";
        cmd3.ExecuteNonQuery();
    }

    private bool doesTableExist(string tableName, IDbConnection connection)
    {
        using IDbCommand cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
        IDbDataParameter p = cmd.CreateParameter();
        p.ParameterName = "@tableName";
        p.Value = tableName;
        cmd.Parameters.Add(p);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        string value = (string)cmd.ExecuteScalar();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        if (value == null)
        {
            return false;
        }

        return true;
    }
}
