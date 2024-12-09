using ITCentral.Common;
using ITCentral.Models;
using ITCentral.Types;
using LinqToDB;

namespace ITCentral.Service;

public class SessionService : ServiceBase, IDisposable
{
    private readonly bool disposed = false;
    
    public SessionService() : base() {}
    
    public Result<Session?, Error> GetSession(string sessionId)
    {
        try
        {
            var session = from s in Repository.Sessions
                          where s.SessionId == sessionId
                          select s;

            return session.FirstOrDefault();

        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public Result<bool, Error> Create(string sessionId, DateTime expiration)
    {
        try
        {
            var insert = Repository.Insert(new Session(sessionId, expiration));
            return AppCommon.Success;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message, ex.StackTrace, false);
        }
    }

    public Result<bool, Error> Delete(string sessionId)
    {
        try
        {
            var check = from s in Repository.Sessions
                        where s.SessionId == sessionId
                        select s.SessionId;
            
            if (check is null) return AppCommon.Fail;

            Repository.Sessions
                .Where(s => s.SessionId == sessionId)
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

    ~SessionService()
    {
        Dispose(false);
    }
}