using System.Text.Json.Serialization;
using LinqToDB.Mapping;

namespace ITCentral.Models;

[Table("RECORD")]
public class Record : IModel
{
    [Column, JsonRequired, NotNull]
    public string HostName { get; set; } = "";

    [Column, JsonRequired, NotNull]
    public DateTime TimeStamp { get; set; }

    [Column, JsonRequired, NotNull]
    public string EventType { get; set; } = "";

    [Column, JsonRequired, NotNull]
    public string CallerMethod { get; set; } = "";

    [Column, JsonRequired, NotNull]
    public string Event { get; set; } = "";

    public Record() { }

    public Record(
        string host,
        DateTime time,
        string type,
        string caller,
        string eventStr
    )
    {
        HostName = host;
        TimeStamp = time;
        EventType = type;
        CallerMethod = caller;
        Event = eventStr;
    }
}