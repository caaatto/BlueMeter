namespace BlueMeter.WPF.Models.Checklist;

/// <summary>
/// Typ des Tasks (Daily/Weekly)
/// </summary>
public enum TaskType
{
    /// <summary>
    /// Daily Task - Resettet täglich um 08:00 GMT+1
    /// </summary>
    Daily,

    /// <summary>
    /// Weekly Task - Resettet montags um 08:00 GMT+1
    /// </summary>
    Weekly
}
