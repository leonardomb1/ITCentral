using System.Text.Json.Serialization;
using LinqToDB.Mapping;

namespace ITCentral.Models;

[Table("ORIGINS")]
public class Origin : IModel
{
    [PrimaryKey, Identity]
    public int? Id { get; set; }

    [Column, NotNull, JsonRequired, JsonPropertyName("OriginName")]
    public string Name { get; set; } = "";

    [Column, NotNull, JsonRequired]
    public string ConnectionString { get; set; } = "";

    public Origin() { }

    public Origin(
        string name,
        string conStr
    )
    {
        Name = name;
        ConnectionString = conStr;
    }
}