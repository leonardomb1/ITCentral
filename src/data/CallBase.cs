using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Text;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Data;

public abstract class CallBase : IDBCall
{
    protected abstract DbCommand CreateDbCommand(string query);
    protected abstract Task<string> GenerateSyncLookupQueryAsync(string tableName);
    protected abstract void AddPrimaryKeyConstraint(StringBuilder query, string tableName);
    protected async Task ExecuteNonQueryAsync(string query)
    {
        using var command = CreateDbCommand(query);
        await command.ExecuteNonQueryAsync();
    }
    protected async Task<object?> ExecuteScalarAsync(string query)
    {
        using var command = CreateDbCommand(query);
        return await command.ExecuteScalarAsync();
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
            var isPrimaryKey = columnName.Equals("Id", StringComparison.OrdinalIgnoreCase);

            if (Nullable.GetUnderlyingType(property.PropertyType) != null) {
                propType = Nullable.GetUnderlyingType(propType);
            }


            var sqlType = GetSqlType(propType!);
            
            queryBuilder.Append($"    [{columnName}] {sqlType}");

            if (isPrimaryKey)
            {
                AddPrimaryKeyConstraint(queryBuilder, tableName);
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
            using var command = CreateDbCommand(queryBuilder.ToString());
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
        var query = await GenerateSyncLookupQueryAsync(tableName);
        
        try
        {
            var result = await ExecuteScalarAsync(query);
            return result != null;
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
            using var command = CreateDbCommand(query);
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
                        var propertyType = property!.PropertyType;
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
            using var command = CreateDbCommand(query);
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableId";
            parameter.Value = id;
            command.Parameters.Add(parameter);

            using var reader = await command.ExecuteReaderAsync();
            
            T? obj = Activator.CreateInstance(typeof(T), true) as T;

            if (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var property = typeof(T).GetProperty(reader.GetName(i));
                    if (property != null && !reader.IsDBNull(i))
                    {
                        var propertyType = property!.PropertyType;
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
            using var command = CreateDbCommand(query);

            foreach (var property in properties) {
                var value = property.GetValue(entity) ?? DBNull.Value;
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{property.Name}";
                parameter.Value = value;
                command.Parameters.Add(parameter);
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
            using var command = CreateDbCommand(query);
            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "@tableId";
            idParameter.Value = id;
            command.Parameters.Add(idParameter);

            foreach (var property in properties) {
                var value = property.GetValue(entity) ?? DBNull.Value;
                var valuesParameter = command.CreateParameter();
                valuesParameter.ParameterName = $"@{property.Name}";
                valuesParameter.Value = value;
                command.Parameters.Add(valuesParameter);
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
            using var command = CreateDbCommand(query);
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableId";
            parameter.Value = id;
            command.Parameters.Add(parameter);

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
    protected abstract string GetSqlType(Type type);   
}