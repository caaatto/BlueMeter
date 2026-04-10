namespace BlueMeter.Services.ModuleSolver.Models;

/// <summary>
/// Represents a single attribute/part of a module
/// </summary>
public class ModulePart
{
    /// <summary>
    /// Attribute ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Attribute name (e.g., "Strength Boost", "Crit Focus")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Attribute value
    /// </summary>
    public int Value { get; set; }

    public ModulePart(int id, string name, int value)
    {
        Id = id;
        Name = name;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Name}+{Value}";
    }
}
