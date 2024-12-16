using System.Data;
using System.Data.Common;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.App.Exchange;

public abstract class DBExchange : ExchangeBase
{
    protected abstract string? QueryNonLocking();

    protected abstract string? QueryPagination(int current);

    protected abstract DbCommand CreateDbCommand(string query, string conStr);

    protected abstract Task<Result<bool, Error>> BulkInsert(DataTable data, Extraction extraction);

    protected override async Task<Result<DataTable, Error>> FetchDataTable(Extraction extraction, int current, CancellationToken token)
    {
        try
        {
            List<DataTable> dataTables = [];
            string[] suffixes = extraction.FileStructure.Split("|");

            await Parallel.ForEachAsync(suffixes, token, async (s, t) =>
            {
                string file = suffixes.Length == 0 ? extraction.Name : extraction.Name + s;
                using DbCommand command = CreateDbCommand(
                    $@"
                        SELECT
                            *
                        FROM {extraction.Name} {QueryNonLocking() ?? ""}
                        ORDER BY {extraction.IndexName} ASC
                        {QueryPagination(current) ?? ""}
                    ",
                    extraction.Origin!.ConnectionString
                );

                await command.Connection!.OpenAsync(token);

                using var fetched = new DataTable();
                var select = await command.ExecuteReaderAsync(t);
                fetched.Load(select);

                lock (dataTables)
                {
                    dataTables.Add(fetched);
                }

                await command.Connection!.CloseAsync();
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
}