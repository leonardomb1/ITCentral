using System.Data;
using System.Data.Common;
using System.Text;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.App;

public abstract class DBExchange : ExchangeBase
{
    protected abstract string? QueryNonLocking();

    protected abstract string? QueryPagination(int current);

    protected abstract DbCommand CreateDbCommand(string query, DbConnection connection);

    protected abstract DbConnection CreateConnection(string conStr);

    protected abstract string GetSqlType(Type dataType, int? lenght);

    protected abstract StringBuilder AddPrimaryKey(StringBuilder stringBuilder, string index, string tableName, string? file = null);

    protected abstract StringBuilder AddChangeColumn(StringBuilder stringBuilder, string tableName);

    protected abstract StringBuilder AddColumnarStructure(StringBuilder stringBuilder, string tableName);

    protected abstract Task EnsureSchemaCreation(string system, DbConnection connection);

    protected abstract Task<bool> LookupTable(string tableName, DbConnection connection);

    protected abstract Task<Result<bool, Error>> BulkInsert(DataTable data, Extraction extraction);

    protected override async Task<Result<DataTable, Error>> FetchDataTable(Extraction extraction, int current, CancellationToken token)
    {
        try
        {
            List<DataTable> dataTables = [];
            string[] suffixes = extraction.FileStructure.Split("|");

            await Parallel.ForEachAsync(suffixes, token, async (s, t) =>
            {
                using DbConnection connection = CreateConnection(extraction.Origin!.ConnectionString);

                string file = suffixes.Length == 0 ? extraction.Name : extraction.Name + s;
                string columns = suffixes.Length == 0 ? "*" : $"'{s[..2]}' AS {extraction.Name}_EMPRESA, *";

                using DbCommand command = CreateDbCommand(
                    $@"
                        SELECT
                            {columns}
                        FROM {file} {QueryNonLocking() ?? ""}
                        ORDER BY {extraction.IndexName} ASC
                        {QueryPagination(current) ?? ""}
                    ",
                    connection
                );

                await connection.OpenAsync(t);

                using var fetched = new DataTable();
                var select = await command.ExecuteReaderAsync(t);
                fetched.Load(select);

                lock (dataTables)
                {
                    dataTables.Add(fetched);
                }

                await connection.CloseAsync();
            });

            DataTable data = dataTables[0].Clone();

            foreach (var table in dataTables)
            {
                data.Merge(table, false, MissingSchemaAction.Ignore);
            }

            return data;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    protected override async Task<Result<bool, Error>> WriteDataTable(DataTable table, Extraction extraction)
    {
        var insert = await BulkInsert(table, extraction);
        if (!insert.IsSuccessful) return insert.Error;

        return AppCommon.Success;
    }

    protected override async Task<Result<bool, Error>> CreateTable(DataTable table, Extraction extraction)
    {
        using var connection = CreateConnection(extraction.Destination!.DbString);
        await connection.OpenAsync();

        if (await LookupTable(extraction.Name, connection))
        {
            await connection.CloseAsync();
            return AppCommon.Success;
        }

        var queryBuilder = new StringBuilder();

        queryBuilder.AppendLine($"CREATE TABLE [{extraction.Origin!.Name}].[{extraction.Name}] (");

        foreach (DataColumn column in table.Columns)
        {
            int? maxStringLength = column.MaxLength;
            string SqlType = GetSqlType(column.DataType, maxStringLength);
            queryBuilder.AppendLine($"    [{column.ColumnName}] {SqlType},");
        }

        queryBuilder = AddChangeColumn(queryBuilder, extraction.Name);
        queryBuilder = AddPrimaryKey(queryBuilder, extraction.IndexName, extraction.Name, extraction.FileStructure);
        queryBuilder = AddColumnarStructure(queryBuilder, extraction.Name);
        queryBuilder.AppendLine(");");

        try
        {

            await EnsureSchemaCreation(extraction.Origin.Name, connection);

            using var command = CreateDbCommand(queryBuilder.ToString(), connection);
            await command.ExecuteNonQueryAsync();

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}