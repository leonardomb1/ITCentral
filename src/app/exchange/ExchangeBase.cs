using System.Data;
using System.Threading.Channels;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.App.Exchange;

public abstract class ExchangeBase
{
    public async Task<Error[]> ChannelParallelize(int max, List<Extraction> extractions)
    {
        Channel<DataTable> channel = Channel.CreateBounded<DataTable>(AppCommon.MaxDegreeParallel);
        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = AppCommon.MaxDegreeParallel
        };
        Error[] errors = [];

        Task producer = Task.Run(async () =>
        {
            await Parallel.ForAsync(1, max + 1, async (int i, CancellationToken c) =>
            {
                var produced = await FetchDataTable(extractions[i], c);
                if (!produced.IsSuccessful)
                {
                    _ = errors.Append(produced.Error);
                }
                await channel.Writer.WriteAsync(produced.Value, c);
            });

            channel.Writer.Complete();
        });

        Task consumer = Task.Run(async () =>
        {
            int attempt = 0;
            Result<bool, Error> insert = new();

            while (await channel.Reader.WaitToReadAsync())
            {
                using DataTable groupTable = new();

                for (int i = 0; i < AppCommon.ConsumerFetchMax && channel.Reader.TryRead(out DataTable? item); i++)
                {
                    groupTable.Merge(item);
                    item.Dispose();
                }

                do
                {
                    attempt++;
                    insert = await WriteDataTable(groupTable);
                } while (!insert.IsSuccessful && attempt < AppCommon.ConsumerAttemptMax);
            }

            if (attempt < AppCommon.ConsumerAttemptMax)
            {
                _ = errors.Append(new Error("Maximum attempts reached", null, false));
            }
        });

        await Task.WhenAll(producer, consumer);

        return errors;
    }

    protected abstract Task<Result<DataTable, Error>> FetchDataTable(Extraction extraction, CancellationToken token);
    protected abstract Task<Result<bool, Error>> WriteDataTable(DataTable table);
}