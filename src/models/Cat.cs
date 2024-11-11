using System.ComponentModel.DataAnnotations.Schema;

namespace ITCentral.Models;

[Table("Cats")]
public class Cat : Animal
{
    public int? Id {get; set;}
    public string? Color {get; set;}
    public Cat() : base() {}
    public Cat(
        string name,
        byte age,
        float size,
        string color
    ) : base(name, age, size) {
        Color = color;
    }
}