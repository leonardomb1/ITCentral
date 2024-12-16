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
    public int OriginId { get; set; }

    [Column, NotNull, JsonRequired]
    public int DestinationId { get; set; }

    [Column, NotNull, JsonRequired]
    public string IndexName { get; set; } = "";

    [Column, NotNull, JsonRequired]
    public string Type { get; set; } = "";

    [Column, NotNull, JsonRequired]
    public string FileStructure { get; set; } = "";

    [Association(ThisKey = nameof(ScheduleId), OtherKey = nameof(Schedule.Id)), Nullable]
    public Schedule? Schedule { get; set; }

    [Association(ThisKey = nameof(DestinationId), OtherKey = nameof(Destination.Id)), Nullable]
    public Destination? Destination { get; set; }

    [Association(ThisKey = nameof(OriginId), OtherKey = nameof(Origin.Id)), Nullable]
    public Origin? Origin { get; set; }

    public Extraction() { }

    public Extraction(
        string name,
        string fColumn,
        int fTime,
        int schId,
        int sysId,
        Schedule schedule,
        Origin system,
        Destination destination
    )
    {
        Name = name;
        FilterColumn = fColumn;
        FilterTime = fTime;
        ScheduleId = schId;
        OriginId = sysId;
        Schedule = schedule;
        Origin = system;
        Destination = destination;
    }
}