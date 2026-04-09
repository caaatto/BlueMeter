namespace BlueMeter.Models;

/// <summary>
/// DPS meter refresh rate settings
/// Controls how frequently DPS numbers update (FPS)
/// </summary>
public enum DpsRefreshRate
{
    /// <summary>
    /// Minimal - 10 FPS (100ms refresh)
    /// Best for low-end PCs or minimal CPU usage
    /// </summary>
    Minimal = 0,

    /// <summary>
    /// Low - 20 FPS (50ms refresh)
    /// Good balance between smoothness and performance (Default)
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium - 30 FPS (33ms refresh)
    /// Smooth updates, moderate CPU usage
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High - 60 FPS (16ms refresh)
    /// Maximum smoothness, highest CPU usage
    /// Recommended for high-end PCs only
    /// </summary>
    High = 3
}

/// <summary>
/// Extension methods for DpsRefreshRate
/// </summary>
public static class DpsRefreshRateExtensions
{
    /// <summary>
    /// Get the update interval in milliseconds for a given refresh rate
    /// </summary>
    public static int GetIntervalMs(this DpsRefreshRate rate)
    {
        return rate switch
        {
            DpsRefreshRate.Minimal => 100, // 10 FPS
            DpsRefreshRate.Low => 50,      // 20 FPS
            DpsRefreshRate.Medium => 33,   // 30 FPS
            DpsRefreshRate.High => 16,     // 60 FPS
            _ => 50 // Default to Low
        };
    }

    /// <summary>
    /// Get the FPS value for a given refresh rate
    /// </summary>
    public static int GetFps(this DpsRefreshRate rate)
    {
        return rate switch
        {
            DpsRefreshRate.Minimal => 10,
            DpsRefreshRate.Low => 20,
            DpsRefreshRate.Medium => 30,
            DpsRefreshRate.High => 60,
            _ => 20 // Default to Low
        };
    }
}
