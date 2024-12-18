using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class UserService : ServiceBase, IService<User, int>, IDisposable
{
    private readonly bool disposed = false;

    public UserService() : base() { }

    public async Task<Result<List<User>, Error>> Get(Dictionary<string, string?>? filters = null)
    {
        try
        {
            var select = from u in Repository.Users
                         select u;

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    select = filter.Key.ToLower() switch
                    {
                        "username" => select.Where(e => e.Name == filter.Value),
                        _ => select
                    };
                }
            }

            var result = await select.ToListAsync();
            result.ForEach(u => u.Password = null);

            return result;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<User?, Error>> Get(int id)
    {
        try
        {
            var select = from u in Repository.Users
                         where u.Id == id
                         select u;

            foreach (var u in select) u.Password = null;

            return await select.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<string, Error>> GetUserCredential(string userName)
    {
        try
        {
            var search = from u in Repository.Users
                         where u.Name == userName
                         select u;

            var result = await search.FirstOrDefaultAsync();

            return result?.Password == null ? "" : Encryption.SymmetricDecryptAES256(result.Password, AppCommon.MasterKey) ?? "";
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Post(User user)
    {
        User encryptedUser = user;
        Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);

        try
        {
            var insert = await Repository.InsertAsync(encryptedUser);
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Put(User user, int id)
    {
        User encryptedUser = user;
        Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);

        try
        {
            user.Id = id;

            await Repository.UpdateAsync(user);

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
            await Repository.Users
                .Where(u => u.Id == id)
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

    ~UserService()
    {
        Dispose(false);
    }
}