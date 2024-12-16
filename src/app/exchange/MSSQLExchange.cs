using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace ITCentral.App.Exchange;

public class MSSQLExchange : DBExchange
{
    protected override string? QueryPagination(int current) =>
        @$"OFFSET {current} ROWS FETCH NEXT {AppCommon.ProducerLineMax} ROWS ONLY";

    protected override string? QueryNonLocking() => "WITH(NOLOCK)";

    protected override DbCommand CreateDbCommand(string query, string conStr)
    {
        using SqlConnection connection = new(conStr);
        return new SqlCommand(query, connection);
    }

    protected override async Task<Result<bool, Error>> BulkInsert(DataTable data, Extraction extraction)
    {
        try
        {
            using SqlConnection connection = new(extraction.Destination!.DbString);

            await connection.OpenAsync();

            using var bulk = new SqlBulkCopy(connection)
            {
                BulkCopyTimeout = AppCommon.BulkCopyTimeout,
                DestinationTableName = $"{extraction.Origin!.Name}.{extraction.Name}"
            };

            await bulk.WriteToServerAsync(data);

            await connection.CloseAsync();

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
}