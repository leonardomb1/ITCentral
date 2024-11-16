using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Models;

[Table("SESSIONS")]
public class Session
{
    public int? Id {get; set;}
    public string? SessionId {get; set;}
    public DateTime? Expiration {get; set;}
    public Session() : base() {}
    public Session(
        string session,
        DateTime time
    ) {
        SessionId = session;
        Expiration = time;
    }
}