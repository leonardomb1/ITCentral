using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;

namespace ITCentral.Service;

public class SessionService : ServiceBase<Session>
{
    public SessionService() : base() {}
    public async Task<Result<Session?, Error>> GetSession(string sessionId)
    {
        var session = await Repository.ReadFromDb<Session, string>("SessionId", sessionId);
        if(!session.IsSuccessful) {
            return session.Error;
        }

        return session.Value.FirstOrDefault();
    }

    public async Task<Result<bool, Error>> Create(string sessionId, DateTime expiration)
    {
        var insert = await Repository.Insert(new Session(sessionId, expiration));
        if(!insert.IsSuccessful) {
            return insert.Error;
        }
        
        return AppCommon.Success;
    }

    public async Task<Result<bool, Error>> Delete(string sessionId)
    {
        var delete = await Repository.DeleteFromDb<Session, string>("SessionId", sessionId);
        if(!delete.IsSuccessful) {
            return delete.Error;
        }

        return delete.Value;
    }
}