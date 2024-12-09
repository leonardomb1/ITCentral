using System.Data;
using System.Threading.Channels;
using ITCentral.Common;
using ITCentral.Types;

namespace ITCentral.App.Exchange;

public abstract class ExchangeBase
{
    public async Task<Result<int, Error>> ChannelParallelize(int max,
                                                             Func<DataTable> produce,
                                                             Func<Result<DataTable, Error>> consume)
    {
        var channel = Channel.CreateBounded<DataTable>(AppCommon.MaxDegreeParallel);
        var options = new ParallelOptions()
        {
            MaxDegreeOfParallelism = AppCommon.MaxDegreeParallel
        };

        var producer = Task.Run(async () =>
        {
            await Parallel.ForAsync(1, max + 1, async (i, c) =>
            {
                DataTable produced = produce();
                await channel.Writer.WriteAsync(produced, c);
            });

            channel.Writer.Complete();
        });

        var consumer = Task.Run(async () =>
        {
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

                } while ();
            }
        });

        await Task.WhenAll(producer, consumer);
    }
}