using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class ScheduleService : ServiceBase, IService<Schedule, int>, IDisposable
{
    private readonly bool disposed = false;
    
    public ScheduleService() : base() { }
    
    public Result<List<Schedule>, Error> Get()
    {
        try 
        {
            var select = from s in Repository.Schedules
                         select s;
            
            return select.ToList();
        } 
        catch (Exception ex) 
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    
    public Result<Schedule?, Error> Get(int id)
    {
        try 
        {
            var select = from s in Repository.Schedules
                         where s.Id == id
                         select s;
            
            return select.FirstOrDefault();
        } 
        catch (Exception ex) 
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    
    public Result<bool, Error> Post(Schedule schedule)
    {
        try 
        {
            var insert = Repository.Insert(schedule);
            return AppCommon.Success;
        } 
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    
    public Result<bool, Error> Put(Schedule schedule, int id)
    {
        try 
        {
            var check = from s in Repository.Schedules
                        where s.Id == id
                        select s.Id;
            
            if (check is null) return AppCommon.Fail;

            schedule.Id = id;

            Repository.Update(schedule); 

            return AppCommon.Success;
        } 
        catch (Exception ex) 
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    
    public Result<bool, Error> Delete(int id)
    {
        try
        {
            var check = from s in Repository.Schedules
                        where s.Id == id
                        select s.Id;
            
            if (check is null) return AppCommon.Fail;

            Repository.Schedules
                .Where(s => s.Id == id)
                .Delete();

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

    ~ScheduleService()
    {
        Dispose(false);
    }
}