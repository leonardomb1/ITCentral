using System.Text.Json.Serialization;
using LinqToDB.Mapping;

namespace ITCentral.Models;

[Table("DESTINATIONS")]
public class Destination : IModel
{
    [PrimaryKey, Identity]
    public int? Id { get; set; }

    [Column, NotNull, JsonRequired, JsonPropertyName("DestinationName")]
    public string Name { get; set; } = "";

    [Column, NotNull, JsonRequired]
    public string DbString { get; set; } = "";

    public Destination() { }

    public Destination(
        string name,
        string dbString
    )
    {
        Name = name;
        DbString = dbString;
    }
}