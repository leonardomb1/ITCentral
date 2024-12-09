using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class UserService : ServiceBase, IService<User, int>, IDisposable
{
    private readonly bool disposed = false;
    
    public UserService() : base() { }
    
    public Result<List<User>, Error> Get()
    {
        try 
        {
            var select = from u in Repository.Users
                         select u;
            
            select.ForEachAsync(u => u.Password = null);

            return select.ToList();
        } 
        catch (Exception ex) 
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    
    public Result<User?, Error> Get(int id)
    {
        try 
        {
            var select = from u in Repository.Users
                         where u.Id == id
                         select u;

            select.ForEachAsync(u => u.Password = null);
            
            return select.FirstOrDefault();
        } 
        catch (Exception ex) 
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    
    public Result<List<User>, Error> Get(string name)
    {
        try 
        {
            var select = from u in Repository.Users
                         where u.Name == name
                         select u;
            
            select.ForEachAsync(u => u.Password = null);
            
            return select.ToList();
        } 
        catch (Exception ex) 
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    
    public Result<string, Error> GetUserCredential(string userName)
    {
        var search = Get(userName);
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
    
    public Result<bool, Error> Post(User user)
    {
        User encryptedUser = user;
        Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);

        try 
        {
            var insert = Repository.Insert(encryptedUser);
            return AppCommon.Success;
        } 
        catch (Exception ex) 
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    
    public Result<bool, Error> Put(User user, int id)
    {
        User encryptedUser = user;
        Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);

        try 
        {
            var check = from u in Repository.Users
                        where u.Id == id
                        select u.Id;
            
            if (check is null) return AppCommon.Fail;

            user.Id = id;

            Repository.Update(user); 

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
            var check = from u in Repository.Users
                        where u.Id == id
                        select u.Id;
            
            if (check is null) return AppCommon.Fail;

            Repository.Users
                .Where(u => u.Id == id)
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

    ~UserService()
    {
        Dispose(false);
    }
}