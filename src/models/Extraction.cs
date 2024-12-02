using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Models;

[Table("EXTRACTIONS")]
public class Extraction
{
    public int? Id {get; set;}
    public string? Name {get; set;}
    public int? ScheduleId {get; set;}
    public int? SystemId {get; set;}
    public string? FilterColumn {get; set;}
    public int? FilterTime {get; set;}
    public string? IndexName {get; set;}
    public Schedule? Schedule {get; set;}
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