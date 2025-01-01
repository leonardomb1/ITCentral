using System.Data;
using System.Threading.Channels;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using ITCentral.App.Database;

namespace ITCentral.App;

public static class ParallelExtractionManager
{
    public static async Task<Result<bool, List<Error>>> ChannelParallelize(
        List<Extraction> extractions,
        Func<List<Extraction>, Channel<(DataTable, Extraction)>, List<Error>, Task> produceData,
        Func<Channel<(DataTable, Extraction)>, List<Error>, Task> consumeData
    )
    {
        Channel<(DataTable, Extraction)> channel = Channel.CreateBounded<(DataTable, Extraction)>(AppCommon.MaxDegreeParallel);
        List<Error> errors = [];

        Task producer = Task.Run(async () => await produceData(extractions, channel, errors));
        Task consumer = Task.Run(async () => await consumeData(channel, errors));

        await Task.WhenAll(producer, consumer);

        if (errors.Count > 0)
        {
            return errors;
        }

        return AppCommon.Success;
    }

    public static async Task ProduceDBData(
        List<Extraction> extractions,
        Channel<(DataTable, Extraction)> channel,
        List<Error> errors
    )
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
    }

    public static async Task ConsumeDBData(Channel<(DataTable, Extraction)> channel, List<Error> errors)
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
                    using var mergedTable = Converter.MergeDataTables(group.Select(x => x.Item1).ToList());
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
    }
}