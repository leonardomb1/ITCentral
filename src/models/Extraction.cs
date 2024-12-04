using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Models;

[Table("EXTRACTIONS")]
public class Extraction
{
    [Key]
    public int? Id {get; set;}
    public string? Name {get; set;}
    public string? FilterColumn {get; set;}
    public int? FilterTime {get; set;}
    public string? IndexName {get; set;}
    [ForeignKey("ScheduleId")]
    public Schedule? Schedule {get; set;}
    [ForeignKey("SystemId")]
    public SystemMap? System {get; set;}
    public Extraction() {}
    public Extraction(
        string name,
        Schedule schedule,
        SystemMap system
    ) {
        Name = name;
        Schedule = schedule;
        System = system;
    }
}