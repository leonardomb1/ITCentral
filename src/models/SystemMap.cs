using System.Text.Json.Serialization;
using LinqToDB.Mapping;

namespace ITCentral.Models;

[Table("SYSTEM_MAPS")]
public class SystemMap : IModel
{
    [PrimaryKey, Identity]
    public int? Id {get; set;}
    
    [Column, NotNull, JsonRequired, JsonPropertyName("SystemName")]
    public string Name {get; set;} = "";
    
    [Column, NotNull, JsonRequired]
    public string ConnectionString {get; set;} = "";
    
    public SystemMap() {}
    
    public SystemMap(
        string name,
        string conStr
    ) {
        Name = name;
        ConnectionString = conStr;
    }
}