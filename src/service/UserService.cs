using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Service;

public class UserService : ServiceBase<User>, IService<User, int>
{
    public UserService() : base() { }
    public async Task<Result<List<User>, Error>> Get()
    {
        var select = await Repository.ReadFromDb<User>();
        if(!select.IsSuccessful) {
            return select.Error;
        }

        return select.Value!;
    }
    public async Task<Result<User?, Error>> GetById(int id)
    {
        var selectById = await Repository.ReadFromDb<User, int>("id", id);
        if(!selectById.IsSuccessful) {
            return selectById.Error;
        }

        return selectById.Value.FirstOrDefault();
    }
    public async Task<Result<List<User>, Error>> GetByName(string name)
    {
        var selectByName = await Repository.ReadFromDb<User, string>("name", name);
        if(!selectByName.IsSuccessful) {
            return selectByName.Error;
        }

        return selectByName.Value!;
    }
    public async Task<Result<string, Error>> GetUserCredential(string userName)
    {
        var search = await GetByName(userName);
        if(!search.IsSuccessful) {
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
        var insert = await Repository.Insert(encryptedUser);
        if(!insert.IsSuccessful) {
            return insert.Error;
        }

        return AppCommon.Success;
    }
    public async Task<Result<User?, Error>> Put(User user, int id)
    {
        User encryptedUser = user;

        Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);

        var select = await Repository.CheckRecord<User, int>("id", id);
        if(!select.IsSuccessful) {
            return select.Error;
        }

        encryptedUser.Id = id;
        var update = await Repository.Update("id", encryptedUser, id);
        if(!update.IsSuccessful) {
            return update.Error;
        }

        return update.Value;
    }
    public async Task<Result<bool, Error>> Delete(int id)
    {
        var delete = await Repository.DeleteFromDb<User, int>("id", id);
        if(!delete.IsSuccessful) {
            return delete.Error;
        }

        return delete.Value;
    }
}