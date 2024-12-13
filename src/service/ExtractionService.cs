using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class ExtractionService : ServiceBase, IService<Extraction, int>, IDisposable
{
    private readonly bool disposed = false;

    public ExtractionService() : base() { }

    public Result<List<Extraction>, Error> Get()
    {
        try
        {
            var select = from e in Repository.Extractions
                         join sys in Repository.SystemMaps
                            on e.SystemId equals sys.Id
                         join sch in Repository.Schedules
                            on e.ScheduleId equals sch.Id
                         join db in Repository.Databases
                            on e.DatabaseId equals db.Id
                         select Extraction.Build(e, sys, sch, db);

            return select.ToList();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public Result<Extraction?, Error> Get(int id)
    {
        try
        {
            var select = from e in Repository.Extractions
                         join sys in Repository.SystemMaps
                           on e.SystemId equals sys.Id
                         join sch in Repository.Schedules
                           on e.ScheduleId equals sch.Id
                         join db in Repository.Databases
                            on e.DatabaseId equals db.Id
                         where e.Id == id
                         select Extraction.Build(e, sys, sch, db);

            return select.FirstOrDefault();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public Result<bool, Error> Post(Extraction extraction)
    {
        try
        {
            var insert = Repository.Insert(extraction);
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public Result<bool, Error> Put(Extraction extraction, int id)
    {
        try
        {
            var check = from e in Repository.Extractions
                        where e.Id == id
                        select e.Id;

            if (check is null) return AppCommon.Fail;

            extraction.Id = id;

            Repository.Update(extraction);

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
            var check = from e in Repository.Extractions
                        where e.Id == id
                        select e.Id;

            if (check is null) return AppCommon.Fail;

            Repository.Extractions
                .Where(e => e.Id == id)
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

    ~ExtractionService()
    {
        Dispose(false);
    }
}