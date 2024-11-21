using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Models;

[Table("SYSTEM_MAPS")]
public class SystemMap
{
    [Key]
    public int Id {get; set;}
    public string? Name {get; set;}
    public string? ConnectionString {get; set;}
    public SystemMap() {}
    public SystemMap(
        string name,
        string conStr
    ) {
        Name = name;
        ConnectionString = conStr;
    }
}