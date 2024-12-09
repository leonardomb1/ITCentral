using LinqToDB.Mapping;

namespace ITCentral.Models;

[Table("SESSIONS")]
public class Session : IModel
{
    [PrimaryKey]
    public string SessionId {get; set;} = "";
    
    [Column, NotNull]
    public DateTime Expiration {get; set;}
    
    public Session() {}
    
    public Session(
        string session,
        DateTime time
    ) {
        SessionId = session;
        Expiration = time;
    }
}