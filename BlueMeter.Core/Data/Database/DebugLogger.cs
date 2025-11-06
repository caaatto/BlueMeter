using System;
using System.IO;

namespace BlueMeter.Core.Data.Database;

/// <summary>
/// Simple file logger for database operations debugging
/// </summary>
public static class DebugLogger
{
    private static readonly object _lock = new object();
    private static string? _logFilePath;

    static DebugLogger()
    {
        try
        {
            // Get logs directory
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var logsDir = Path.Combine(baseDir, "logs");

            // Create logs directory if it doesn't exist
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            // Create log file with current date
            var fileName = $"database-debug-{DateTime.Now:yyyyMMdd}.log";
            _logFilePath = Path.Combine(logsDir, fileName);

            // Write initial log entry
            Log("=".PadRight(80, '='));
            Log($"BlueMeter Database Debug Log - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Log("=".PadRight(80, '='));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize DebugLogger: {ex.Message}");
            _logFilePath = null;
        }
    }

    /// <summary>
    /// Log a message to the debug file
    /// </summary>
    public static void Log(string message)
    {
        if (string.IsNullOrEmpty(_logFilePath)) return;

        try
        {
            lock (_lock)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var logLine = $"[{timestamp}] {message}";

                // Write to file
                File.AppendAllText(_logFilePath, logLine + Environment.NewLine);

                // Also write to console for backwards compatibility
                Console.WriteLine(logLine);
            }
        }
        catch
        {
            // Silently fail if logging fails
        }
    }
}
