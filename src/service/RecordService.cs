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

    public async Task<Result<List<Record>, Error>> GetLast(int relativeTimeSec)
    {
        try
        {
            var select = from s in Repository.Records
                         where s.TimeStamp > DateTime.Now.AddSeconds(-relativeTimeSec)
                         orderby s.TimeStamp descending
                         select s;

            return await select.ToListAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
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
            return new Error(ex.Message, ex.StackTrace, false);
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
            return new Error(ex.Message, ex.StackTrace, false);
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
            return new Error(ex.Message, ex.StackTrace, false);
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