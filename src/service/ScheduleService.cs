using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class ScheduleService : ServiceBase, IService<Schedule, int>, IDisposable
{
    private readonly bool disposed = false;

    public ScheduleService() : base() { }

    public async Task<Result<List<Schedule>, Error>> Get(Dictionary<string, string?>? filters = null)
    {
        try
        {
            var select = from s in Repository.Schedules
                         select s;

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    select = filter.Key.ToLower() switch
                    {
                        "name" => select.Where(e => e.Name == filter.Value),
                        "status" when bool.TryParse(filter.Value, out var sts) => select.Where(e => e.Status == sts),
                        _ => select
                    };
                }
            }

            return await select.ToListAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<Schedule?, Error>> Get(int id)
    {
        try
        {
            var select = from s in Repository.Schedules
                         where s.Id == id
                         select s;

            return await select.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<bool, Error>> Post(Schedule schedule)
    {
        try
        {
            var insert = await Repository.InsertAsync(schedule);
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<bool, Error>> Put(Schedule schedule, int id)
    {
        try
        {
            schedule.Id = id;

            await Repository.UpdateAsync(schedule);

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    public async Task<Result<bool, Error>> Delete(int id)
    {
        try
        {
            await Repository.Schedules
                .Where(s => s.Id == id)
                .DeleteAsync();

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

    ~ScheduleService()
    {
        Dispose(false);
    }
}