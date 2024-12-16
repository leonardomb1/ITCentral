using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class UserService : ServiceBase, IService<User, int>, IDisposable
{
    private readonly bool disposed = false;

    public UserService() : base() { }

    public async Task<Result<List<User>, Error>> Get()
    {
        try
        {
            var select = from u in Repository.Users
                         select u;

            foreach (var u in select) u.Password = null;

            return await select.ToListAsync();
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

    public async Task<Result<List<User>, Error>> Get(string name)
    {
        try
        {
            var select = from u in Repository.Users
                         where u.Name == name
                         select u;

            foreach (var u in select) u.Password = null;

            return await select.ToListAsync();
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<string, Error>> GetUserCredential(string userName)
    {
        var search = await Get(userName);
        if (!search.IsSuccessful)
        {
            return search.Error;
        }

        if (search.Value is null)
        {
            return "";
        }

        var user = search.Value.FirstOrDefault();

        return Encryption.SymmetricDecryptAES256(user?.Password ?? "", AppCommon.MasterKey);
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