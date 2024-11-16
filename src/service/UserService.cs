using ITCentral.Data;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Service;

public class UserService<T> : ServiceBase<T, User>, IService<User, int> where T : IDBCall
{
    public UserService(T db) : base(db) {}
    public async Task<Result<List<User?>, Error>> Read()
    {
        var dbResult = await dbCaller.ReadFromDb<User>();

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value;
    }
    public async Task<Result<User?, Error>> Read(int id)
    {
        var dbResult = await dbCaller.ReadFromDb<User, string, int>("Id", id);

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value[0];
    }
    public async Task<Result<List<User?>, Error>> Read(string name)
    {
        var dbResult = await dbCaller.ReadFromDb<User, string, string>("Name", name);

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value;
    }
    public async Task<Result<User?, Error>> Save(User data)
    {
        var dbResult = await dbCaller.Insert(data);

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value;
    }
    public async Task<Result<User?, Error>> Save(User data, int id)
    {
        var dbResult = await dbCaller.Update(data, id);
        if(!dbResult.IsSuccessful) return dbResult.Error;

        var fetchUpdated = await Read(id);

        return fetchUpdated.Value!;
    }
    public async Task<Result<bool, Error>> Delete(int id)
    {
        var dbResult = await dbCaller.DeleteFromDb<User, int>(id);
        if(!dbResult.IsSuccessful) return dbResult.Error;

        return dbResult.Value;
    }
}