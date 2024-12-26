using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using ClickHouse.Ado;
using System.Data;
using System.Data.Common;
using System.Text;

namespace ITCentral.App.Database;

public class ClickhouseExchange : DBExchange
{
    protected override string? QueryPagination(int current) =>
        $"LIMIT {AppCommon.ProducerLineMax} OFFSET {current}";

    protected override string? QueryNonLocking() => null;

    protected override StringBuilder AddPrimaryKey(StringBuilder stringBuilder, string index, string tableName, string? file)
    {
        string indexGroup = file == null ? $"{index}" : $"{index}, {tableName}_EMPRESA";
        return stringBuilder.Append($" PRIMARY KEY ({indexGroup}),");
    }

    protected override StringBuilder AddChangeColumn(StringBuilder stringBuilder, string tableName) =>
        stringBuilder.AppendLine($" DT_UPDATE_{tableName} DateTime DEFAULT now(),");

    protected override StringBuilder AddColumnarStructure(StringBuilder stringBuilder, string tableName) =>
        stringBuilder.Append($"");

    protected override async Task<bool> LookupTable(string tableName, DbConnection connection)
    {
        using var select = new ClickHouseCommand((ClickHouseConnection)connection, "EXISTS TABLE @table");
        select.Parameters.Add("@table", tableName);

        var res = await select.ExecuteScalarAsync();

        return res != DBNull.Value && res != null && (bool)res;
    }

    protected override async Task EnsureSchemaCreation(string system, DbConnection connection)
    {
        using var select = new ClickHouseCommand((ClickHouseConnection)connection, "EXISTS DATABASE @schema");
        select.Parameters.Add("@schema", system);

        var res = await select.ExecuteScalarAsync();

        if (res == DBNull.Value || res == null || !(bool)res)
        {
            using var createSchema = new ClickHouseCommand((ClickHouseConnection)connection, $"CREATE DATABASE {system}");
            await createSchema.ExecuteNonQueryAsync();
        }
    }

    protected override DbConnection CreateConnection(string conStr)
    {
        return new ClickHouseConnection(conStr);
    }

    protected override DbCommand CreateDbCommand(string query, DbConnection connection)
    {
        return new ClickHouseCommand((ClickHouseConnection)connection, query);
    }

    protected override string GetSqlType(Type type, int? length = -1)
    {
        return type switch
        {
            _ when type == typeof(long) => "Int64",
            _ when type == typeof(int) => "Int32",
            _ when type == typeof(short) => "Int16",
            _ when type == typeof(string) => length > 0 ? $"String({length})" : "String",
            _ when type == typeof(bool) => "UInt8",
            _ when type == typeof(DateTime) => "DateTime",
            _ when type == typeof(double) => "Float64",
            _ when type == typeof(decimal) => "Decimal(18,2)",
            _ when type == typeof(byte) => "UInt8",
            _ when type == typeof(sbyte) => "Int8",
            _ when type == typeof(ushort) => "UInt16",
            _ when type == typeof(uint) => "UInt32",
            _ when type == typeof(ulong) => "UInt64",
            _ when type == typeof(float) => "Float32",
            _ when type == typeof(char) => "FixedString(1)",
            _ when type == typeof(Guid) => "UUID",
            _ when type == typeof(TimeSpan) => "Interval",
            _ when type == typeof(byte[]) => "Array(UInt8)",
            _ when type == typeof(object) => "String",
            _ => throw new NotSupportedException($"Type '{type.Name}' is not supported")
        };
    }

    protected override async Task<Result<bool, Error>> BulkInsert(DataTable data, Extraction extraction)
    {
        try
        {
            using var connection = new ClickHouseConnection(extraction.Destination!.DbString);
            await connection.OpenAsync();

            using var writer = connection.CreateCommand();
            writer.CommandText = $"INSERT INTO {extraction.Origin!.Name}.{extraction.Name} VALUES @bulk";

            var bulk = new List<object[]>();
            foreach (DataRow row in data.Rows)
            {
                bulk.Add(row.ItemArray!);
            }

            writer.Parameters.Add("@bulk", bulk);

            await writer.ExecuteNonQueryAsync();
            await connection.CloseAsync();

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
}
