using System.Data;
using System.Data.Common;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.App.Exchange;

public abstract class DBExchange : ExchangeBase
{
    protected string? QueryNonLocking;

    protected string? QueryPagination;

    protected abstract DbCommand CreateDbCommand(string query);

    protected abstract Task<Result<int, Error>> BulkInsert(DataTable data, Extraction extraction);

    protected override async Task<Result<DataTable, Error>> FetchDataTable(Extraction extraction, CancellationToken token)
    {
        try
        {
            using DbCommand command = CreateDbCommand(
                $@"
                    SELECT
                        *
                    FROM {extraction.Name} {QueryNonLocking ?? ""}
                    {QueryPagination ?? ""}
                "
            );

            var select = await command.ExecuteReaderAsync(token);

            DataTable data = new();
            data.Load(select);

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