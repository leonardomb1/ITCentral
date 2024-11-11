using System.Text;
using Microsoft.Data.SqlClient;
using ITCentral.Models;
using ITCentral.Types;
using ITCentral.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Data;

public class SqlServerCall : IDBCall, IDisposable
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

    public async Task<Result<bool, Error>> CreateTable<T>()
    {
        var tableName = GetTableName(typeof(T));
        var queryBuilder = new StringBuilder();

        queryBuilder.AppendLine($"CREATE TABLE [{tableName}] (");
        var properties = typeof(T).GetProperties();
        
        for (int i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            var columnName = property.Name;
            var propType = property.PropertyType;

            if (Nullable.GetUnderlyingType(property.PropertyType) != null) {
                propType = Nullable.GetUnderlyingType(propType);
            } 

            var sqlType = GetSqlType(propType!);
            
            var isPrimaryKey = columnName.Equals("Id", StringComparison.OrdinalIgnoreCase);

            queryBuilder.Append($"    [{columnName}] {sqlType}");
            if (isPrimaryKey)
            {
                queryBuilder.Append(" IDENTITY(1,1)");
                queryBuilder.Append($" CONSTRAINT IX_{tableName.ToUpper()}_PK PRIMARY KEY CLUSTERED");
            }

            if (i < properties.Length - 1)
            {
                queryBuilder.AppendLine(",");
            }
            else
            {
                queryBuilder.AppendLine();
            }
        }

        queryBuilder.AppendLine(");");

        try
        {
            using var command = new SqlCommand(queryBuilder.ToString(), connection);
            await command.ExecuteNonQueryAsync();
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }


    public async Task<Result<bool, Error>> SyncLookup<T>()
    {
        var tableName = GetTableName(typeof(T));
        string query = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";

        try
        {
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            var execute = await command.ExecuteScalarAsync() ?? "0";

            _ = byte.TryParse(execute.ToString(), out byte result);
            return result == AppCommon.Exists;
        } 
        catch(Exception ex) 
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<List<T?>, Error>> ReadFromDb<T>() where T : class
    {
        var tableName = GetTableName(typeof(T));
        string query = $"SELECT * FROM {tableName}";

        try
        {
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<T?>();

            while (await reader.ReadAsync())
            {
                T? obj = Activator.CreateInstance(typeof(T), true) as T;

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var property = typeof(T).GetProperty(reader.GetName(i));
                    if (property != null && !reader.IsDBNull(i))
                    {
                        var propertyType = property.PropertyType;
                        var value = reader.GetValue(i);

                        if (Nullable.GetUnderlyingType(propertyType) != null)
                        {
                            var underlyingType = Nullable.GetUnderlyingType(propertyType);
                            property.SetValue(obj, Convert.ChangeType(value, underlyingType!));
                        }
                        else
                        {
                            property.SetValue(obj, Convert.ChangeType(value, propertyType));
                        }
                    }
                }

                results.Add(obj);
            }

            return results;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<T?, Error>> ReadFromDb<T, ID>(ID id) where T : class
    {
        var tableName = GetTableName(typeof(T));
        string query = $"SELECT * FROM {tableName} WHERE Id = @tableId";

        try
        {
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@tableId", id);

            using var reader = await command.ExecuteReaderAsync();
            
            T? obj = Activator.CreateInstance(typeof(T), true) as T;

            if (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var property = typeof(T).GetProperty(reader.GetName(i));
                    if (property != null && !reader.IsDBNull(i))
                    {
                        var propertyType = property.PropertyType;
                        var value = reader.GetValue(i);

                        if (Nullable.GetUnderlyingType(propertyType) != null)
                        {
                            var underlyingType = Nullable.GetUnderlyingType(propertyType);
                            property.SetValue(obj, Convert.ChangeType(value, underlyingType!));
                        }
                        else
                        {
                            property.SetValue(obj, Convert.ChangeType(value, propertyType));
                        }
                    }
                }
            }
            
            return obj;
        } 
        catch (Exception ex) 
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<T, Error>> Insert<T>(T entity)
    {
        var tableName = GetTableName(typeof(T));
        var properties = typeof(T)
            .GetProperties()
            .Where(p => p.Name != "Id");

        var columnNames = string.Join(", ", properties.Select(p => $"[{p.Name}]"));
        var paramNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        var query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames});";
        
        try
        {
            using var command = new SqlCommand(query, connection);

            foreach (var property in properties) {
                var value = property.GetValue(entity) ?? DBNull.Value;
                command.Parameters.AddWithValue($"@{property.Name}", value);
            }

            await command.ExecuteNonQueryAsync();
            return entity;            
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<T?, Error>> Update<T, ID>(T entity, ID id)
    {
        var tableName = GetTableName(typeof(T));
        var properties = typeof(T)
            .GetProperties()
            .Where(p => p.Name != "Id");

        var mapping = string.Join(", ", properties.Select(p => $"[{p.Name}] = @{p.Name}"));

        var query = $"UPDATE {tableName} SET {mapping} WHERE Id = @tableId";
        
        try
        {
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@tableId", id);

            foreach (var property in properties) {
                var value = property.GetValue(entity) ?? DBNull.Value;
                command.Parameters.AddWithValue($"@{property.Name}", value);
            }

            await command.ExecuteNonQueryAsync();
            return entity;            
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> DeleteFromDb<T, ID>(ID id)
    {
        var tableName = GetTableName(typeof(T));

        var query = $"DELETE FROM {tableName} WHERE Id = @tableId";

        try
        {
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@tableId", id);

            await command.ExecuteNonQueryAsync();

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    private static string GetTableName(Type type)
    {
        var tableAttribute = type
            .GetCustomAttributes(typeof(TableAttribute), false)
            .FirstOrDefault() as TableAttribute;
        var tableName = tableAttribute?.Name ?? type.Name;

        return tableName!;
    }

    private static string GetSqlType(Type type)
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