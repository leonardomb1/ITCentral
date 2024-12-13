using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;
using LinqToDB.Data;

namespace ITCentral.Service;

public class DatabaseService : ServiceBase, IService<SystemMap, int>, IDisposable
{
    private readonly bool disposed = false;

    public DatabaseService() : base() { }

    public Result<List<SystemMap>, Error> Get()
    {
        try
        {
            var select = from db in Repository.Databases
                         select db;

            return select.ToList();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public Result<SystemMap?, Error> Get(int id)
    {
        try
        {
            var select = from s in Repository.Databases
                         where s.Id == id
                         select s;

            return select.FirstOrDefault();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public Result<bool, Error> Post(SystemMap system)
    {
        try
        {
            var insert = Repository.Insert(system);
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public Result<bool, Error> Put(SystemMap system, int id)
    {
        try
        {
            var check = from s in Repository.Databases
                        where s.Id == id
                        select s.Id;

            if (check is null) return AppCommon.Fail;

            system.Id = id;

            Repository.Update(system);

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
            var check = from s in Repository.Databases
                        where s.Id == id
                        select s.Id;

            if (check is null) return AppCommon.Fail;

            Repository.Databases
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

    ~SystemMapService()
    {
        Dispose(false);
    }
}