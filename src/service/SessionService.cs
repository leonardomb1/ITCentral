using System.Reflection;
using ITCentral.Models;
using ITCentral.Repository;
using ITCentral.Types;

namespace ITCentral.Service;

public class SessionService : ServiceBase
{
    public SessionService() : base(typeof(SessionRepository<>)) {}
    public async Task<Result<Session?, Error>> GetSession(string sessionId)
    {
        MethodInfo method = repositoryType!.GetMethod("Read", [typeof(string)])!;
        var result = await (Task<Result<List<Session>, Error>>) method.Invoke(repositoryInstance, [sessionId])!;

        if(!result.IsSuccessful) {
            return result.Error;
        }

        return result.Value.ElementAtOrDefault(0);
    }
    public async Task<Result<Session, Error>> Create(string sessionId, DateTime expiration)
    {
        Session session = new(sessionId, expiration);

        MethodInfo method = repositoryType!.GetMethod("Save", [typeof(Session)])!;
        var result = await (Task<Result<Session, Error>>) method.Invoke(repositoryInstance, [session])!;

        if(!result.IsSuccessful) {
            return result.Error;
        }

        return result.Value;
    }
    public async Task<Result<bool, Error>> Delete(string sessionId)
    {
        MethodInfo method = repositoryType!.GetMethod("Delete", [typeof(string)])!;
        var result = await (Task<Result<bool, Error>>) method.Invoke(repositoryInstance, [sessionId])!;

        if(!result.IsSuccessful) {
            return result.Error;
        }

        return result.Value;
    }
}