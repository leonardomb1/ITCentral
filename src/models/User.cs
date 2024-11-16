using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Models;

[Table("Users")]
public class User
{
    public int? Id {get; set;}
    public string? Name {get; set;}
    public string? Password {get; set;}
    public User() {}
    public User(
        string name,
        string password
    ) {
        Name = name;
        Password = password;
    }
}