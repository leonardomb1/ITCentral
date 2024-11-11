using ITCentral.Data;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Service;

public class CatService<T> : ServiceBase<T, Cat>, IService<Cat, int> where T : IDBCall
{
    public CatService(T db) : base(db) {}
    public async Task<Result<List<Cat?>, Error>> Read()
    {
        var dbResult = await dbCaller.ReadFromDb<Cat>();

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value;
    }

    public async Task<Result<Cat?, Error>> Read(int id)
    {
        var dbResult = await dbCaller.ReadFromDb<Cat, int>(id);

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value;
    }

    public async Task<Result<Cat?, Error>> Save(Cat data)
    {
        var dbResult = await dbCaller.Insert(data);

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value;
    }

    public async Task<Result<Cat?, Error>> Save(Cat data, int id)
    {
        var dbResult = await dbCaller.Update(data, id);
        if(!dbResult.IsSuccessful) return dbResult.Error;

        var fetchUpdated = await Read(id);

        return fetchUpdated.Value!;
    }

    public async Task<Result<bool, Error>> Delete(int id)
    {
        var dbResult = await dbCaller.DeleteFromDb<Cat, int>(id);
        if(!dbResult.IsSuccessful) return dbResult.Error;

        return dbResult.Value;
    }
}