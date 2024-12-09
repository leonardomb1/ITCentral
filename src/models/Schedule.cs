using System.Text.Json.Serialization;
using LinqToDB.Mapping;

namespace ITCentral.Models;

[Table("SCHEDULES")]
public class Schedule : IModel
{
    [PrimaryKey, Identity]
    public int? Id {get; set;}

    [Column, NotNull, JsonRequired, JsonPropertyName("ScheduleName")]
    public string Name {get; set;} = "";
    
    [Column, NotNull, JsonRequired]
    public bool Status {get; set;}
    
    [Column, NotNull, JsonRequired]
    public int Value {get; set;}
    
    public Schedule() {}
    
    public Schedule(
        string name,
        bool status,
        int value
    ) {
        Name = name;
        Status = status;
        Value = value;
    }
}