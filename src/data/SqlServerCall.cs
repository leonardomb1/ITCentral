using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ITCentral.Data;

public class SqlServerCall : CallBase, IDisposable, IDBCall
{
    private readonly SqlConnection connection;
    private bool disposed = false;

    public SqlServerCall(string connectionString)
    {
        connection = new SqlConnection(connectionString);
        connection.OpenAsync()
            .GetAwaiter()
            .GetResult();
    }

    protected override async Task<string> GenerateSyncLookupQueryAsync(string tableName)
    {
        return await Task.FromResult($"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}';");
    }

    protected override DbCommand CreateDbCommand(string query)
    {
        return new SqlCommand(query, connection);
    }

    protected override void AddPrimaryKeyConstraint(StringBuilder query, string tableName)
    {
        query.Append(" IDENTITY(1,1)");
        query.Append($" CONSTRAINT IX_{tableName.ToUpper()}_PK PRIMARY KEY CLUSTERED");
    }

    protected override string GetSqlType(Type type)
    {   
        return type switch
        {
            _ when type == typeof(long) => "BIGINT",
            _ when type == typeof(int) => "INT",
            _ when type == typeof(short) => "SMALLINT",
            _ when type == typeof(string) => "NVARCHAR(MAX)",
            _ when type == typeof(bool) => "BIT",
            _ when type == typeof(DateTime) => "DATETIME",
            _ when type == typeof(double) => "FLOAT",
            _ when type == typeof(decimal) => "DECIMAL(18,2)",
            _ when type == typeof(byte) => "TINYINT",
            _ when type == typeof(sbyte) => "TINYINT", 
            _ when type == typeof(ushort) => "SMALLINT", 
            _ when type == typeof(uint) => "INT", 
            _ when type == typeof(ulong) => "BIGINT", 
            _ when type == typeof(float) => "REAL",
            _ when type == typeof(char) => "NCHAR(1)",
            _ when type == typeof(Guid) => "UNIQUEIDENTIFIER",
            _ when type == typeof(TimeSpan) => "TIME",
            _ when type == typeof(byte[]) => "VARBINARY(MAX)",
            _ when type == typeof(object) => "SQL_VARIANT",
            _ => throw new NotSupportedException($"Type '{type.Name}' is not supported")
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

    ~SqlServerCall()
    {
        Dispose(false);
    }
}