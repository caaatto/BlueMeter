using Microsoft.Extensions.Logging;

namespace StarResonanceDpsAnalysis.WPF.ViewModels;

/// <summary>
/// Optimized log entry for better performance - immutable with pre-computed display values
/// </summary>
public sealed class LogEntry
{
    public LogEntry(DateTime timestamp, LogLevel level, string message, string category, Exception? exception = null)
    {
        Timestamp = timestamp;
        Level = level;
        Message = message ?? string.Empty;
        Category = category ?? string.Empty;
        Exception = exception;

        // Pre-compute display strings to avoid repeated formatting during binding
        TimeDisplay = timestamp.ToString("HH:mm:ss.fff");
        LevelDisplay = level.ToString();
    }

    public DateTime Timestamp { get; }
    public LogLevel Level { get; }
    public string Message { get; }
    public string Category { get; }
    public Exception? Exception { get; }

    // Pre-computed display properties for better binding performance
    public string TimeDisplay { get; }
    public string LevelDisplay { get; }
}