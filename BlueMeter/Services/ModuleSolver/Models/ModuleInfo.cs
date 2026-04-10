namespace BlueMeter.Services.ModuleSolver.Models;

/// <summary>
/// Represents a complete module with all its attributes
/// </summary>
public class ModuleInfo
{
    /// <summary>
    /// Module name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Module configuration ID
    /// </summary>
    public int ConfigId { get; set; }

    /// <summary>
    /// Unique identifier for this module instance
    /// </summary>
    public long Uuid { get; set; }

    /// <summary>
    /// Module quality/rarity level
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    /// List of attribute parts on this module
    /// </summary>
    public List<ModulePart> Parts { get; set; } = new();

    /// <summary>
    /// Module category (Attack/Defense/Support)
    /// </summary>
    public ModuleCategory Category { get; set; }

    public ModuleInfo()
    {
    }

    public ModuleInfo(string name, int configId, long uuid, int quality, List<ModulePart> parts)
    {
        Name = name;
        ConfigId = configId;
        Uuid = uuid;
        Quality = quality;
        Parts = parts;
    }

    /// <summary>
    /// Get total attribute value sum
    /// </summary>
    public int GetTotalAttributeValue()
    {
        int total = 0;
        foreach (var part in Parts)
        {
            total += part.Value;
        }
        return total;
    }

    public override string ToString()
    {
        return $"{Name} (Q{Quality}) - {string.Join(", ", Parts)}";
    }
}
