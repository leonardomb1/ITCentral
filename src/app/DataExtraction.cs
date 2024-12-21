using System.Data;
using System.Threading.Channels;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.App;

public static class DataExtraction
{
    public static async Task<Result<bool, List<Error>>> ChannelParallelize(List<Extraction> extractions)
    {
        Channel<(DataTable, Extraction)> channel = Channel.CreateBounded<(DataTable, Extraction)>(AppCommon.MaxDegreeParallel);
        List<Error> errors = [];

        Task producer = Task.Run(async () =>
        {
            await Parallel.ForEachAsync(extractions, AppCommon.ParallelRule, async (e, t) =>
            {
                bool hasData = true;
                int curr = 0;

                var fetcher = DBExchangeFactory.Create(e.Origin!.DbType);

                do
                {
                    var attempt = await fetcher.FetchDataTable(e, curr, t);
                    if (!attempt.IsSuccessful)
                    {
                        errors.Add(attempt.Error);
                        break;
                    }

                    if (attempt.Value.Rows.Count == 0) hasData = false;

                    curr += AppCommon.ProducerLineMax;

                    await channel.Writer.WriteAsync((attempt.Value, e), t);
                } while (hasData);
            });

            channel.Writer.Complete();
        });

        Task consumer = Task.Run(async () =>
        {
            Result<bool, Error> insert = new();
            Result<bool, Error> create = new();

            while (await channel.Reader.WaitToReadAsync())
            {
                int attempt = 0;
                List<(DataTable, Extraction)> fetchedData = [];

                for (int i = 0; i < AppCommon.ConsumerFetchMax && channel.Reader.TryRead(out (DataTable, Extraction) item); i++)
                {
                    fetchedData.Add(item);
                }

                var groupedData = fetchedData
                    .GroupBy(e => e.Item2)
                    .Select(group =>
                    {
                        using var mergedTable = MergeDataTables(group.Select(x => x.Item1).ToList());
                        return (MergedTable: mergedTable, Extraction: group.Key);
                    }).ToList();

                do
                {
                    attempt++;
                    await Parallel.ForEachAsync(groupedData, AppCommon.ParallelRule, async (e, t) =>
                    {
                        try
                        {
                            var inserter = DBExchangeFactory.Create(e.Extraction.Destination!.DbType);
                            create = await inserter.CreateTable(e.MergedTable, e.Extraction);
                            insert = await inserter.WriteDataTable(e.MergedTable, e.Extraction);
                        }
                        finally
                        {
                            e.MergedTable.Dispose();
                        }
                    });
                } while ((!insert.IsSuccessful || !create.IsSuccessful) && attempt < AppCommon.ConsumerAttemptMax);

                if (attempt > AppCommon.ConsumerAttemptMax)
                {
                    errors.Add(insert.Error!);
                }
            }
        });

        await Task.WhenAll(producer, consumer);

        return AppCommon.Success;
    }

    public static DataTable MergeDataTables(List<DataTable> tables)
    {
        if (tables == null || tables.Count == 0)
            throw new ArgumentException("No tables to merge.");

        DataTable mergedTable = tables[0].Clone();

        try
        {
            foreach (var table in tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    mergedTable.ImportRow(row);
                }
            }
        }
        finally
        {
            foreach (var table in tables)
            {
                table.Dispose();
            }
        }

        return mergedTable;
    }
}