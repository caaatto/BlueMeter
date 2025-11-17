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
    /// Personal training dummy mode - shows only your damage
    /// </summary>
    Personal = 1,

    /// <summary>
    /// Group training dummy mode - shows only your party/group
    /// </summary>
    Group = 2
}
