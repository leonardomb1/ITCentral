using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;
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
    protected abstract string AddForeignKeyConstraint(string tableName,string columnName, string fkTable, string fkColumn);
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
        List<string> constraints = new();

        queryBuilder.AppendLine($"CREATE TABLE [{tableName}] (");
        var properties = typeof(T).GetProperties();
        
        foreach(var property in properties)
        {
            var propType = property.PropertyType;
            var primaryKey = property.GetCustomAttribute<KeyAttribute>();
            var foreignKeys = property.GetCustomAttribute<ForeignKeyAttribute>();
            var columnName = foreignKeys != null ? property.GetCustomAttribute<ForeignKeyAttribute>()!.Name : property.Name;

            if (Nullable.GetUnderlyingType(property.PropertyType) != null) 
            {
                propType = Nullable.GetUnderlyingType(propType);
            }
            
            string sqlType = foreignKeys == null ? GetSqlType(propType!) : GetSqlType(typeof(int));
            queryBuilder.Append($"    [{columnName}] {sqlType}");

            if (primaryKey != null)
            {
                AddPrimaryKeyConstraint(queryBuilder, tableName);
            }

            if (foreignKeys != null)
            {
                var foreignTable = GetTableName(propType!);
                var foreignKeyColumn = "Id";

                constraints.Add(AddForeignKeyConstraint(tableName, columnName, foreignTable, foreignKeyColumn));
            }

            queryBuilder.AppendLine(",");
        }

        if (constraints.Count != 0) {
            queryBuilder.AppendLine(string.Join(",\n", constraints));
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
        var query = new StringBuilder($"SELECT * FROM {tableName}");

        var foreignKeys = typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<ForeignKeyAttribute>() != null);

        foreach (var fk in foreignKeys)
        {
            var fkTable = GetTableName(fk.PropertyType);
            var fkColumn = fk.GetCustomAttribute<ForeignKeyAttribute>()!.Name;

            query.Append($" INNER JOIN [{fkTable}] ON [{tableName}].[{fkColumn}] = [{fkTable}].[id]");
        }

        try
        {
            using var command = CreateDbCommand(query.ToString());
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<T?>();

            while (await reader.ReadAsync())
            {
                var obj = Activator.CreateInstance(typeof(T), true) as T;

                foreach (var property in typeof(T).GetProperties())
                {
                    var columnName = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;
                    var foreignKeyAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();
                    var properColumn = foreignKeyAttribute?.Name ?? columnName;
                    if (reader.GetOrdinal(properColumn) != -1)
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal(properColumn)))
                    {
                        var value = reader.GetValue(reader.GetOrdinal(properColumn));
                        var propertyType = property.PropertyType;

                        if (propertyType.IsClass && propertyType != typeof(string))
                        {
                            if (value is long || value is int)
                            {
                                var foreignObject = Activator.CreateInstance(propertyType);
                                var foreignProperty = propertyType.GetProperty("Id");
                                foreignProperty?.SetValue(foreignObject, Convert.ChangeType(value, foreignProperty.PropertyType));
                                property.SetValue(obj, foreignObject);
                            }
                        }
                        else
                        {
                            if (Nullable.GetUnderlyingType(propertyType) != null)
                            {
                                var underlyingType = Nullable.GetUnderlyingType(propertyType)!;
                                property.SetValue(obj, Convert.ChangeType(value, underlyingType));
                            }
                            else
                            {
                                property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                            }
                        }
                    }
                }

               if (foreignKeyAttribute != null)
                {
                    var foreignObject = Activator.CreateInstance(property.PropertyType);

                    foreach (var foreignProperty in property.PropertyType.GetProperties())
                    {
                        var foreignColumn = foreignProperty.GetCustomAttribute<ColumnAttribute>()?.Name ?? foreignProperty.Name;
                        var foreignQualifiedColumnName = $"{foreignKeyAttribute.Name ?? foreignProperty.Name}.{foreignColumn}";

                        if (reader.GetOrdinal(foreignQualifiedColumnName) != -1)
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal(foreignQualifiedColumnName)))
                            {
                                var foreignValue = reader.GetValue(reader.GetOrdinal(foreignQualifiedColumnName));
                                var foreignPropertyType = foreignProperty.PropertyType;

                                if (Nullable.GetUnderlyingType(foreignPropertyType) != null)
                                {
                                    var underlyingType = Nullable.GetUnderlyingType(foreignPropertyType)!;
                                    foreignProperty.SetValue(foreignObject, Convert.ChangeType(foreignValue, underlyingType));
                                }
                                else
                                {
                                    foreignProperty.SetValue(foreignObject, Convert.ChangeType(foreignValue, foreignPropertyType));
                                }
                            }
                        }
                    }

                    property.SetValue(obj, foreignObject);
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

    public async Task<Result<List<T?>, Error>> ReadFromDb<T, V>(string id, V val) where T : class
    {
        var tableName = GetTableName(typeof(T));
        var query = new StringBuilder($"SELECT * FROM {tableName}");

        var foreignKeys = typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<ForeignKeyAttribute>() != null);

        foreach (var fk in foreignKeys) 
        {
            var fkTable = GetTableName(fk.PropertyType);
            var fkColumn = fk.GetCustomAttribute<ForeignKeyAttribute>();

            query.Append($" INNER JOIN [{fkTable}] ON [{tableName}].[id] = [{fkTable}].[{fkColumn}]");
        }

        query.Append($" WHERE [{id}] = @tableId");

        try
        {
            using var command = CreateDbCommand(query.ToString());
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableId";
            parameter.Value = val;
            command.Parameters.Add(parameter);

            using var reader = await command.ExecuteReaderAsync();
            var results = new List<T?>();

            while (await reader.ReadAsync())
            {
                var obj = Activator.CreateInstance(typeof(T), true) as T;

                foreach (var property in typeof(T).GetProperties())
                {
                    var columnName = property.GetCustomAttribute<ColumnAttribute>()?.Name ?? property.Name;
                    if (!reader.HasRows || reader.GetOrdinal(columnName) == -1) continue;
                    if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
                    {
                        var value = reader.GetValue(reader.GetOrdinal(columnName));
                        var propertyType = property.PropertyType;

                        if (Nullable.GetUnderlyingType(propertyType) != null) {
                            var underlyingType = Nullable.GetUnderlyingType(propertyType)!;
                            property.SetValue(obj, Convert.ChangeType(value, underlyingType));
                        } else {
                            property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                        }
                    }

                    var foreignKeyAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();
                    if (foreignKeyAttribute != null)
                    {
                        var foreignObject = Activator.CreateInstance(property.PropertyType);

                        foreach (var foreignProperty in property.PropertyType.GetProperties())
                        {
                            var foreignColumn = foreignProperty.GetCustomAttribute<ColumnAttribute>()?.Name ?? foreignProperty.Name;

                            if (!reader.HasRows || reader.GetOrdinal(foreignColumn) == -1) continue;
                            if (!reader.IsDBNull(reader.GetOrdinal(foreignColumn)))
                            {
                                var foreignValue = reader.GetValue(reader.GetOrdinal(foreignColumn));
                                var foreignPropertyType = foreignProperty.PropertyType;

                                if (Nullable.GetUnderlyingType(foreignPropertyType) != null)
                                {
                                    var underlyingType = Nullable.GetUnderlyingType(foreignPropertyType)!;
                                    foreignProperty.SetValue(foreignObject, Convert.ChangeType(foreignValue, underlyingType));
                                }
                                else
                                {
                                    foreignProperty.SetValue(foreignObject, Convert.ChangeType(foreignValue, foreignPropertyType));
                                }
                            }
                        }

                        property.SetValue(obj, foreignObject);
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
    public async Task<Result<T, Error>> Insert<T>(T entity)
    {
        var tableName = GetTableName(typeof(T));
        var properties = typeof(T)
            .GetProperties()
            .Where(p => p.GetCustomAttribute<KeyAttribute>() == null);

        var columnNames = string.Join(", ", properties.Select(p => {
            var fk = p.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
            return fk != null ? $"[{fk}]" : $"[{p.Name}]";
        }));

        var paramNames = string.Join(", ", properties.Select(p => {
            var fk = p.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
            return fk != null ? $"@{fk}" : $"@{p.Name}";
        }));

        var query = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames});";
        
        try
        {
            using var command = CreateDbCommand(query);

            foreach (var property in properties) {
                var foreignKey = property.GetCustomAttribute<ForeignKeyAttribute>();
                var value = foreignKey != null
                    ? property.PropertyType.GetProperty("Id")?.GetValue(property.GetValue(entity)) ?? DBNull.Value
                    : property.GetValue(entity) ?? DBNull.Value;

                var parameterName = foreignKey != null
                    ? $"@{foreignKey.Name}"
                    : $"@{property.Name}";

                var parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
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
    public async Task<Result<T?, Error>> Update<T, V>(string id, T entity, V val)
    {
        var tableName = GetTableName(typeof(T));
        var properties = typeof(T)
            .GetProperties()
            .Where(p => p.GetCustomAttribute<KeyAttribute>() == null);

        var mapping = string.Join(", ", properties.Select(p => $"[{p.Name}] = @{p.Name}"));

        var query = $"UPDATE {tableName} SET {mapping} WHERE {id} = @tableId";
        
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
    public async Task<Result<bool, Error>> DeleteFromDb<T, V>(string id, V val)
    {
        var tableName = GetTableName(typeof(T));

        var query = $"DELETE FROM {tableName} WHERE {id} = @tableId";

        try
        {
            using var command = CreateDbCommand(query);
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableId";
            parameter.Value = val;
            command.Parameters.Add(parameter);

            await command.ExecuteNonQueryAsync();

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    public async Task<Result<bool, Error>> CheckRecord<T, V>(string id, V val)
    {
        var tableName = GetTableName(typeof(T));

        var query = $"SELECT 1 FROM {tableName} WHERE {id} = @tableId";

        try
        {
            using var command = CreateDbCommand(query);
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableId";
            parameter.Value = val;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();

            return result != null;
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