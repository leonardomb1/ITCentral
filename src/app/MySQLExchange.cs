using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Text;

namespace ITCentral.App;

public class MySQLExchange : DBExchange
{
    protected override string? QueryPagination(int current) =>
        $"LIMIT {AppCommon.ProducerLineMax} OFFSET {current}";

    protected override string? QueryNonLocking() => "LOCK IN SHARE MODE";

    protected override StringBuilder AddPrimaryKey(StringBuilder stringBuilder, string index, string tableName, string? file)
    {
        string indexGroup = file == null ? $"{index} ASC" : $"{index} ASC, {tableName}_EMPRESA ASC";
        return stringBuilder.Append($" PRIMARY KEY ({indexGroup}),");
    }

    protected override StringBuilder AddChangeColumn(StringBuilder stringBuilder, string tableName) =>
        stringBuilder.AppendLine($" DT_UPDATE_{tableName} DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,");

    protected override StringBuilder AddColumnarStructure(StringBuilder stringBuilder, string tableName) =>
        stringBuilder.Append($"");

    protected override async Task<bool> LookupTable(string tableName, DbConnection connection)
    {
        using MySqlCommand select = new($"SHOW TABLES LIKE '{tableName}'", (MySqlConnection)connection);
        var res = await select.ExecuteScalarAsync();

        return res != null;
    }

    protected override async Task EnsureSchemaCreation(string system, DbConnection connection)
    {
        using MySqlCommand select = new($"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{system}'", (MySqlConnection)connection);
        var res = await select.ExecuteScalarAsync();

        if (res == null)
        {
            using MySqlCommand createSchema = new($"CREATE SCHEMA {system}", (MySqlConnection)connection);
            await createSchema.ExecuteNonQueryAsync();
        }
    }

    protected override DbConnection CreateConnection(string conStr)
    {
        return new MySqlConnection(conStr);
    }

    protected override DbCommand CreateDbCommand(string query, DbConnection connection)
    {
        return new MySqlCommand(query, (MySqlConnection)connection);
    }

    protected override string GetSqlType(Type type, int? length = -1)
    {
        return type switch
        {
            _ when type == typeof(long) => "BIGINT",
            _ when type == typeof(int) => "INT",
            _ when type == typeof(short) => "SMALLINT",
            _ when type == typeof(string) => length > 0 ? $"VARCHAR({length})" : "TEXT",
            _ when type == typeof(bool) => "TINYINT(1)",
            _ when type == typeof(DateTime) => "DATETIME",
            _ when type == typeof(double) => "DOUBLE",
            _ when type == typeof(decimal) => "DECIMAL(18,2)",
            _ when type == typeof(byte) => "TINYINT UNSIGNED",
            _ when type == typeof(sbyte) => "TINYINT",
            _ when type == typeof(ushort) => "SMALLINT UNSIGNED",
            _ when type == typeof(uint) => "INT UNSIGNED",
            _ when type == typeof(ulong) => "BIGINT UNSIGNED",
            _ when type == typeof(float) => "FLOAT",
            _ when type == typeof(char) => "CHAR(1)",
            _ when type == typeof(Guid) => "CHAR(36)",
            _ when type == typeof(TimeSpan) => "TIME",
            _ when type == typeof(byte[]) => "BLOB",
            _ when type == typeof(object) => "JSON",
            _ => throw new NotSupportedException($"Type '{type.Name}' is not supported")
        };
    }

    protected override async Task<Result<bool, Error>> BulkInsert(DataTable data, Extraction extraction)
    {
        try
        {
            using MySqlConnection connection = new(extraction.Destination!.DbString);
            await connection.OpenAsync();

            using MySqlTransaction transaction = await connection.BeginTransactionAsync();

            MySqlBulkLoader bulk = new(connection)
            {
                TableName = $"{extraction.Origin!.Name}.{extraction.Name}",
                FieldTerminator = ",",
                LineTerminator = "\n",
                NumberOfLinesToSkip = 0,
                Local = true
            };

            foreach (DataColumn column in data.Columns)
            {
                bulk.Columns.Add(column.ColumnName);
            }

            using MemoryStream memoryStream = new();
            using StreamWriter writer = new(memoryStream, Encoding.UTF8, 1024, true);
            foreach (DataRow row in data.Rows)
            {
                var fields = row.ItemArray.Select(field => field?.ToString());
                writer.WriteLine(string.Join(",", fields));
            }

            memoryStream.Position = 0;
            using StreamReader reader = new(memoryStream);
            bulk.Load(reader.BaseStream);
            Log.Out($"Writing row data {data.Rows.Count} - in {bulk.TableName}");
            await bulk.LoadAsync();

            await transaction.CommitAsync();
            await connection.CloseAsync();

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
}
