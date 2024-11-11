namespace ITCentral.Models;

public abstract class Animal
{
    public string? Name {get; set;}
    public byte? Age {get; set;}
    /// <summary>
    /// Size is in centimeters
    /// </summary>
    public float? Size {get; set;}
    protected Animal() {}
    protected Animal(
        string name,
        byte age,
        float size
    ) {
        Name = name;
        Age = age;
        Size = size;
    }
}