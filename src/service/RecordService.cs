using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;
using LinqToDB.Data;

namespace ITCentral.Service;

public class RecordService : ServiceBase, IDisposable
{
    private readonly bool disposed = false;

    public RecordService() : base() { }

    public async Task<Result<List<Record>, Error>> Get(Dictionary<string, string?>? filters = null)
    {
        try
        {
            var select = from s in Repository.Records
                         select s;

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    select = filter.Key.ToLower() switch
                    {
                        "relative" when int.TryParse(filter.Value, out var time) =>
                            select
                                .Where(e => e.TimeStamp > DateTime.Now.AddSeconds(-time)),
                        "hostname" =>
                            select
                                .Where(e => e.HostName == filter.Value),
                        "type" =>
                            select
                                .Where(e => e.EventType == filter.Value),
                        "event" =>
                            select
                                .Where(e => e.Event.Contains(filter.Value ?? "")),
                        _ => select
                    };
                }

                if (filters.TryGetValue("take", out var takeValue) && int.TryParse(takeValue, out var count))
                {
                    select = select.OrderByDescending(x => x.TimeStamp).Take(count);
                }
                else
                {
                    select = select.OrderByDescending(x => x.TimeStamp);
                }
            }
            else
            {
                select = select.OrderByDescending(x => x.TimeStamp);
            }


            return await select.ToListAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<int, Error>> GetCount()
    {
        try
        {
            var select = from s in Repository.Records
                         select s;

            return await select.CountAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<bool, Error>> Post(List<Record> record)
    {
        try
        {
            var insert = await Repository.BulkCopyAsync(record);
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<bool, Error>> Clear()
    {
        try
        {
            await Repository.Origins
                .TruncateAsync();

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed && !disposing)
        {
            Repository.Dispose();
        }
    }

    ~RecordService()
    {
        Dispose(false);
    }
}