using System.Text.Json.Serialization;
using LinqToDB.Mapping;

namespace ITCentral.Models;

[Table("EXTRACTIONS")]
public class Extraction : IModel
{
    [PrimaryKey, Identity]
    public int? Id { get; set; }

    [Column, NotNull, JsonRequired, JsonPropertyName("ExtractionName")]
    public string Name { get; set; } = "";
    
    [Column, Nullable]
    public string? FilterColumn { get; set; }
    
    [Column, Nullable]
    public int FilterTime { get; set; }
    
    [Column, NotNull, JsonRequired]
    public int ScheduleId { get; set; }
    
    [Column, NotNull, JsonRequired]
    public int SystemId { get; set; }
    
    [Column, NotNull, JsonRequired]
    public string IndexName { get; set; } = "";
    
    [Column, NotNull, JsonRequired]
    public string Type { get; set; } = "";
    
    [Association(ThisKey = nameof(ScheduleId), OtherKey = nameof(Schedule.Id)), Nullable]
    public Schedule? Schedule { get; set; }
    
    [Association(ThisKey = nameof(SystemId), OtherKey = nameof(System.Id)), Nullable]
    public SystemMap? System { get; set; }
    
    public static Extraction? Build(
        Extraction e,
        SystemMap sys,
        Schedule sch
    )
    {
        if (e == null) return e;
        if (sys != null)
        {
            e.System = sys;
        }
        if (sch != null)
        {
            e.Schedule = sch;
        }

        return e;
    }

    public Extraction() { }
    
    public Extraction(
        string name,
        string fColumn,
        int fTime,
        int schId,
        int sysId,
        Schedule schedule,
        SystemMap system
    )
    {
        Name = name;
        FilterColumn = fColumn;
        FilterTime = fTime;
        ScheduleId = schId;
        SystemId = sysId;
        Schedule = schedule;
        System = system;
    }
}