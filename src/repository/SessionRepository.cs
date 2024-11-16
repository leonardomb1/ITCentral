using ITCentral.Data;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Repository;

public class SessionRepository<T> : RepositoryBase<T, Session>, IRepository<Session, int> where T : IDBCall
{
    public SessionRepository(T db) : base(db) {}
    public async Task<Result<List<Session?>, Error>> Read()
    {
        var dbResult = await dbCaller.ReadFromDb<Session>();

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value;
    }
    public async Task<Result<Session?, Error>> Read(int id)
    {
        var dbResult = await dbCaller.ReadFromDb<Session, string, int>("Id", id);

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value[0];
    }
    public async Task<Result<List<Session?>, Error>> Read(string session)
    {
        var dbResult = await dbCaller.ReadFromDb<Session, string, string>("SessionId", session);

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value;
    }
    public async Task<Result<Session?, Error>> Save(Session data)
    {
        var dbResult = await dbCaller.Insert(data);

        if(!dbResult.IsSuccessful) return dbResult.Error;
        return dbResult.Value;
    }
    public async Task<Result<Session?, Error>> Save(Session data, int id)
    {
        var dbResult = await dbCaller.Update(data, id);
        if(!dbResult.IsSuccessful) return dbResult.Error;

        var fetchUpdated = await Read(id);

        return fetchUpdated.Value!;
    }
    public async Task<Result<bool, Error>> Delete(int id)
    {
        var dbResult = await dbCaller.DeleteFromDb<Session, string, int>("Id", id);
        if(!dbResult.IsSuccessful) return dbResult.Error;

        return dbResult.Value;
    }
    public async Task<Result<bool, Error>> Delete(string sessionId)
    {
        var dbResult = await dbCaller.DeleteFromDb<Session, string, string>("SessionId", sessionId);
        if(!dbResult.IsSuccessful) return dbResult.Error;

        return dbResult.Value;
    }
}