using ITCentral.Service;

namespace ITCentral.Common;

public static class SessionManager
{
    public static async Task<(string sessionId, DateTime ExpirationTime)> CreateSession(string ip)
    {
        string randomComponent = Guid.NewGuid().ToString();
        string seed = $"{ip}:{DateTime.Now.Ticks}:{randomComponent}";

        string sessionId = Encryption.Sha256(seed);
        DateTime expiration = DateTime.Now.AddSeconds(AppCommon.SessionTime);

        using var sessionService = new SessionService();

        await sessionService.Create(sessionId, expiration);

        Log.Out($"Creating session id for {ip}, with expiration at: {expiration:yyyy-MM-dd HH:mm:ss}");
        return (sessionId, expiration);
    }

    public static async Task<bool> IsSessionValid(string sessionId)
    {
        var service = new SessionService();
        var session = await service.GetSession(sessionId);

        if (!session.IsSuccessful || session.Value is null || DateTime.UtcNow > session.Value.Expiration)
        {
            return false;
        }

        return true;
    }
}