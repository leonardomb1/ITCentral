using System.Data;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB.Data;
using LinqToDB.SchemaProvider;

namespace ITCentral.App.Exchange;

public class DBExchange
{
    public Result<int, Error> FetchDataTable(Extraction extraction, CancellationToken token)
    {
        try
        {
            if (extraction.System == null)
            {
                return new Error("Invalid configuration for extraction.", null, false);
            }

            using DataConnection DBCall = new(extraction.System.DatabaseType, extraction.System.ConnectionString);

            var tables = DBCall
                .DataProvider
                .GetSchemaProvider()
                .GetSchema(DBCall, new GetSchemaOptions() { })
                .Tables
                .Where(table =>
                {
                    return table.TableName!.Contains(extraction.Name);
                });

            foreach (var table in tables)
            {
                Console.WriteLine(table.TableName);
            }

            return 1;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
            throw;
        }
    }
}