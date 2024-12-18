using ITCentral.Common;
using ITCentral.Service;

namespace ITCentral.App;

public static class SessionManager
{
    public static (string sessionId, DateTime ExpirationTime) CreateSession(string ip)
    {
        string randomComponent = Guid.NewGuid().ToString();
        string seed = $"{ip}:{DateTime.Now.Ticks}:{randomComponent}";

        string sessionId = Encryption.Sha256(seed);
        DateTime expiration = DateTime.Now.AddSeconds(AppCommon.SessionTime);

        _ = new SessionService().Create(sessionId, expiration);

        Log.Out($"Creating session id for {ip}, with expiration at: {expiration:yyyy-MM-dd HH:mm:ss}");
        return (sessionId, expiration);
    }

    public static bool IsSessionValid(string sessionId)
    {
        var session = new SessionService().GetSession(sessionId).Result;

        if(!session.IsSuccessful || session.Value is null || DateTime.UtcNow > session.Value.Expiration) {
            return false;
        }
                
        return true;
    }
}