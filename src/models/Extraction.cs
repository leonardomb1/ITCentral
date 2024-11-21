using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Models;

[Table("EXTRACTIONS")]
public class Extraction
{
    public int? Id {get; set;}
    public string? Name {get; set;}
    public int? ScheduleId {get; set;}
    public int? SystemId {get; set;}
    public int? OptionsId {get; set;}
    public Schedule? Schedule {get; set;}
    public SystemMap? System {get; set;}
    public Option? Options {get; set;}
    public Extraction() {}
    public Extraction(
        string name,
        Schedule schedule,
        SystemMap system,
        Option option
    ) {
        Name = name;
        Schedule = schedule;
        System = system;
        Options = option;
    }
}