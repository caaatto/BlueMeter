namespace BlueMeter.WPF.Models;

/// <summary>
/// Training mode types for DPS meters
/// </summary>
public enum TrainingMode
{
    /// <summary>
    /// No training mode active
    /// </summary>
    None = 0,

    /// <summary>
    /// Personal training dummy mode
    /// </summary>
    Personal = 1,

    /// <summary>
    /// Faction training dummy mode
    /// </summary>
    Faction = 2,

    /// <summary>
    /// Extreme training dummy mode
    /// </summary>
    Extreme = 3
}
