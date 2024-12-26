using System.Text.Json.Serialization;
using LinqToDB.Mapping;

namespace ITCentral.Models;

[Table("USERS")]
public class User : IModel
{
    [PrimaryKey, Identity]
    public int? Id { get; set; }

    [Column, NotNull, JsonRequired, JsonPropertyName("Username")]
    public string Name { get; set; } = "";

    [Column, Nullable]
    public string? Password { get; set; }

    public User() { }

    public User(
        string name,
        string password
    )
    {
        Name = name;
        Password = password;
    }
}