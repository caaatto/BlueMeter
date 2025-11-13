namespace BlueMeter.WPF.Models.Checklist;

/// <summary>
/// Kategorien für Checklist-Tasks mit zugehörigen Farbcodes
/// </summary>
public enum TaskCategory
{
    /// <summary>
    /// Dungeon-bezogene Tasks (Blau: #2196F3)
    /// </summary>
    Dungeon,

    /// <summary>
    /// Raid-bezogene Tasks (Lila: #9C27B0)
    /// </summary>
    Raid,

    /// <summary>
    /// PvP-bezogene Tasks (Rot: #F44336)
    /// </summary>
    PvP,

    /// <summary>
    /// Crafting/Gathering Tasks (Grün: #4CAF50)
    /// </summary>
    Crafting,

    /// <summary>
    /// Social/Guild Tasks (Orange: #FF9800)
    /// </summary>
    Social,

    /// <summary>
    /// Allgemeine Daily Tasks (Gelb: #FFEB3B)
    /// </summary>
    Dailies,

    /// <summary>
    /// Allgemeine Weekly Tasks (Cyan: #00BCD4)
    /// </summary>
    Weeklies
}

/// <summary>
/// Extension-Methoden für TaskCategory
/// </summary>
public static class TaskCategoryExtensions
{
    /// <summary>
    /// Gibt den Farbcode für eine Task-Kategorie zurück
    /// </summary>
    public static string GetColorCode(this TaskCategory category)
    {
        return category switch
        {
            TaskCategory.Dungeon => "#2196F3",
            TaskCategory.Raid => "#9C27B0",
            TaskCategory.PvP => "#F44336",
            TaskCategory.Crafting => "#4CAF50",
            TaskCategory.Social => "#FF9800",
            TaskCategory.Dailies => "#FFEB3B",
            TaskCategory.Weeklies => "#00BCD4",
            _ => "#9E9E9E" // Grau als Fallback
        };
    }
}
