using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using Microsoft.EntityFrameworkCore;

namespace ITCentral.Service;

public class SessionService : ServiceBase<Session>
{
    public SessionService() : base() {}
    public async Task<Result<Session?, Error>> GetSession(string sessionId)
    {
        try
        {
            var session = await Repository.FirstOrDefaultAsync(x => x.SessionId == sessionId);
            return session;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Create(string sessionId, DateTime expiration)
    {
        var session = new Session(sessionId, expiration);
        try
        {
            await Repository.AddAsync(session);
            await SaveChangesAsync();
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public async Task<Result<bool, Error>> Delete(string sessionId)
    {
        try
        {
            var session = await Repository.FirstOrDefaultAsync(x => x.SessionId == sessionId);
            Repository.Remove(session!);
            await SaveChangesAsync();
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    } 
}