using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class DestinationService : ServiceBase, IService<Destination, int>, IDisposable
{
    private readonly bool disposed = false;

    public DestinationService() : base() { }

    public async Task<Result<List<Destination>, Error>> Get()
    {
        try
        {
            var select = from db in Repository.Destinations
                         select db;

            return await select.ToListAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<Destination?, Error>> Get(int id)
    {
        try
        {
            var select = from db in Repository.Destinations
                         where db.Id == id
                         select db;

            return await select.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Post(Destination Destination)
    {
        try
        {
            var insert = await Repository.InsertAsync(Destination);
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Put(Destination Destination, int id)
    {
        try
        {
            Destination.Id = id;

            await Repository.UpdateAsync(Destination);

            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Delete(int id)
    {
        try
        {
            await Repository.Destinations
                .Where(db => db.Id == id)
                .DeleteAsync();

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

    ~DestinationService()
    {
        Dispose(false);
    }
}