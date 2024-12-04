using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Models;

[Table("SCHEDULES")]
public class Schedule
{
    [Key]
    public int? Id {get; set;}
    public string? Name {get; set;}
    public bool? Status {get; set;}
    public int? Value {get; set;}
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