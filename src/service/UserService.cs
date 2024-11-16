using System.Reflection;
using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Repository;
using ITCentral.Types;

namespace ITCentral.Service;

public class UserService : ServiceBase, IService<User, int>
{
    public UserService() : base(typeof(UserRepository<>)) {}
    public async Task<Result<List<User>, Error>> Get()
    {
        MethodInfo method = repositoryType!.GetMethod("Read", [])!;
        return await (Task<Result<List<User>, Error>>) method.Invoke(repositoryInstance, null)!;
    }
    public async Task<Result<User, Error>> GetById(int id)
    {
        MethodInfo method = repositoryType!.GetMethod("Read", [typeof(int)])!;
        return await (Task<Result<User, Error>>) method.Invoke(repositoryInstance, [id])!;
    }
    public async Task<Result<List<User>, Error>> GetByName(string name)
    {
        MethodInfo method = repositoryType!.GetMethod("Read", [typeof(string)])!;
        return await (Task<Result<List<User>, Error>>) method.Invoke(repositoryInstance, [name])!;
    }
    public async Task<Result<string, Error>> GetUserCredential(string userName)
    {
        MethodInfo method = repositoryType!.GetMethod("Read", [typeof(string)])!;
        var search = await (Task<Result<List<User>, Error>>) method.Invoke(repositoryInstance, [userName])!;
        if(!search.IsSuccessful) {
            return search.Error;
        }
        return Encryption.SymmetricDecryptAES256(search.Value[0].Password ?? "", AppCommon.MasterKey);
    }
    public async Task<Result<User, Error>> Post(User user)
    {
        MethodInfo method = repositoryType!.GetMethod("Save", [typeof(User)])!;
        User encryptedUser = user;
        Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);
        return await (Task<Result<User, Error>>) method.Invoke(repositoryInstance, [encryptedUser])!;
    }
    public async Task<Result<User, Error>> Put(User user, int id)
    {
        MethodInfo method = repositoryType!.GetMethod("Save", [typeof(User), typeof(int)])!;
        User encryptedUser = user;
        Encryption.SymmetricEncryptAES256(encryptedUser.Password!, AppCommon.MasterKey);
        return await (Task<Result<User, Error>>) method.Invoke(repositoryInstance, [encryptedUser, id])!;
    }
    public async Task<Result<User, Error>> Delete(int id)
    {
        MethodInfo method = repositoryType!.GetMethod("Delete", [typeof(int)])!;
        return await (Task<Result<User, Error>>) method.Invoke(repositoryInstance, [id])!;
    }
}