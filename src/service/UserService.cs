using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using Microsoft.EntityFrameworkCore;


namespace ITCentral.Service;

public class UserService : ServiceBase<User>, IService<User, int>
{
    public UserService() : base() {}
    public async Task<Result<List<User>, Error>> Get()
    {
        try
        {
            var user = await Repository.ToListAsync();
            return user;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    public async Task<Result<User?, Error>> GetById(int id)
    {
        try
        {
            var user = await Repository.FindAsync(id);
            return user;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    public async Task<Result<List<User>, Error>> GetByName(string name)
    {
        try
        {
            var user = await Repository.Where(x => x.Name == name).ToListAsync();
            return user;
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
            var search = await Repository.FirstOrDefaultAsync(x => x.Name == userName);

            if(search is null) {
                return "";
            }

            return Encryption.SymmetricDecryptAES256(search.Password ?? "", AppCommon.MasterKey);
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }    
    }
    public async Task<Result<User, Error>> Post(User user)
    {
        User encryptedUser = user;
        
        try {
            Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);
            await Repository.AddAsync(encryptedUser);
            await SaveChangesAsync();
            return encryptedUser;
        } 
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
    public async Task<Result<User?, Error>> Put(User user, int id)
    {
        User encryptedUser = user;
        
        try {
            Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);
            if(!await Repository.AnyAsync(x => x.Id == id)) return new User();

            User? dbUser = await Repository.FindAsync(id);
            dbUser = encryptedUser;
            await SaveChangesAsync();
            return dbUser;
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
            var user = await Repository.FindAsync(id);
            Repository.Remove(user!);
            await SaveChangesAsync();
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }
}