namespace BlueMeter.Models;

/// <summary>
/// Represents a single data point in a time-series chart
/// Stores timestamp and value for DPS/HPS tracking
/// </summary>
public record ChartDataPoint
{
    /// <summary>
    /// UTC timestamp when this data point was recorded
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// The value at this timestamp (DPS, HPS, etc.)
    /// </summary>
    public double Value { get; init; }

    public ChartDataPoint(DateTime timestamp, double value)
    {
        Timestamp = timestamp;
        Value = value;
    }
}
