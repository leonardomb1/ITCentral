using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class OriginService : ServiceBase, IService<Origin, int>, IDisposable
{
    private readonly bool disposed = false;

    public OriginService() : base() { }

    public async Task<Result<List<Origin>, Error>> Get(Dictionary<string, string?>? filters = null)
    {
        try
        {
            var select = from s in Repository.Origins
                         select s;

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    select = filter.Key.ToLower() switch
                    {
                        "name" => select.Where(e => e.Name == filter.Value),
                        _ => select
                    };
                }
            }

            return await select.ToListAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<Origin?, Error>> Get(int id)
    {
        try
        {
            var select = from s in Repository.Origins
                         where s.Id == id
                         select s;

            return await select.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Post(Origin system)
    {
        try
        {
            system.ConnectionString = Encryption.SymmetricEncryptAES256(system.ConnectionString, AppCommon.MasterKey);

            var insert = await Repository.InsertAsync(system);
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Put(Origin system, int id)
    {
        try
        {
            system.ConnectionString = Encryption.SymmetricEncryptAES256(system.ConnectionString, AppCommon.MasterKey);

            system.Id = id;
            await Repository.UpdateAsync(system);

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
            await Repository.Origins
                .Where(s => s.Id == id)
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

    ~OriginService()
    {
        Dispose(false);
    }
}