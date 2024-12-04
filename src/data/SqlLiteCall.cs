using System.Data.Common;
using System.Text;
using Microsoft.Data.Sqlite;

namespace ITCentral.Data;

public class SqlLiteCall : CallBase, IDisposable
{
    private readonly SqliteConnection connection;
    private bool disposed = false;

    public SqlLiteCall(string connectionString)
    {
        connection = new SqliteConnection(connectionString);
        connection.Open();
    }

    protected override async Task<string> GenerateSyncLookupQueryAsync(string tableName)
    {
        return await Task.FromResult($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';");
    }

    protected override DbCommand CreateDbCommand(string query)
    {
        return new SqliteCommand(query, connection);
    }

    protected override void AddPrimaryKeyConstraint(StringBuilder query, string tableName)
    {
        query.Append(" PRIMARY KEY AUTOINCREMENT");
    }
    protected override string AddForeignKeyConstraint(string tableName, string columnName, string fkTable, string fkColumn)
    {
        return $" FOREIGN KEY ([{columnName}]) REFERENCES [{fkTable}]([{fkColumn}])";
    }

    protected override string GetSqlType(Type type)
    {
        return type switch
        {
            _ when type == typeof(long) => "INTEGER",
            _ when type == typeof(int) => "INTEGER",
            _ when type == typeof(short) => "INTEGER",
            _ when type == typeof(byte) => "INTEGER",
            _ when type == typeof(sbyte) => "INTEGER",
            _ when type == typeof(ulong) => "INTEGER",
            _ when type == typeof(uint) => "INTEGER",
            _ when type == typeof(ushort) => "INTEGER",
            _ when type == typeof(bool) => "INTEGER",
            _ when type == typeof(float) => "REAL",
            _ when type == typeof(double) => "REAL",
            _ when type == typeof(decimal) => "REAL", 
            _ when type == typeof(string) => "TEXT",
            _ when type == typeof(char) => "TEXT", 
            _ when type == typeof(DateTime) => "TEXT",
            _ when type == typeof(Guid) => "TEXT",
            _ when type == typeof(byte[]) => "BLOB",
            _ when type == typeof(ReadOnlyMemory<byte>) => "BLOB",
            _ when type == typeof(object) => "BLOB",
            _ when type == typeof(TimeSpan) => "TEXT",
            _ => throw new NotSupportedException($"Type '{type.Name}' is not supported by SQLite.")
        };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                connection.Dispose();
            }

            disposed = true;
        }
    } 

    ~SqlLiteCall()
    {
        Dispose(false);
    }
}