using System.Collections.ObjectModel;
using BlueMeter.Models;

namespace BlueMeter.Services;

/// <summary>
/// Event arguments for chart history being cleared
/// Contains snapshots of the data before clearing
/// </summary>
public class ChartHistoryClearingEventArgs : EventArgs
{
    public Dictionary<long, List<ChartDataPoint>> DpsHistorySnapshot { get; }
    public Dictionary<long, List<ChartDataPoint>> HpsHistorySnapshot { get; }

    public ChartHistoryClearingEventArgs(
        Dictionary<long, List<ChartDataPoint>> dpsHistory,
        Dictionary<long, List<ChartDataPoint>> hpsHistory)
    {
        DpsHistorySnapshot = dpsHistory;
        HpsHistorySnapshot = hpsHistory;
    }
}

/// <summary>
/// Service for collecting and managing chart data in real-time
/// Samples DPS/HPS data at regular intervals for chart visualization
/// </summary>
public interface IChartDataService : IDisposable
{
    /// <summary>
    /// Fired BEFORE chart history is cleared, providing snapshots of the data
    /// This allows subscribers to save the data before it's lost
    /// </summary>
    event EventHandler<ChartHistoryClearingEventArgs>? BeforeHistoryCleared;

    /// <summary>
    /// Start background sampling timer
    /// </summary>
    void Start();

    /// <summary>
    /// Stop background sampling timer
    /// </summary>
    void Stop();

    /// <summary>
    /// Get DPS history for a specific player
    /// </summary>
    /// <param name="playerId">Player UID</param>
    /// <returns>Observable collection of DPS data points, or null if player not found</returns>
    ObservableCollection<ChartDataPoint>? GetDpsHistory(long playerId);

    /// <summary>
    /// Get HPS history for a specific player
    /// </summary>
    /// <param name="playerId">Player UID</param>
    /// <returns>Observable collection of HPS data points, or null if player not found</returns>
    ObservableCollection<ChartDataPoint>? GetHpsHistory(long playerId);

    /// <summary>
    /// Get all player IDs that have chart data
    /// </summary>
    /// <returns>Collection of player UIDs</returns>
    IReadOnlyCollection<long> GetTrackedPlayerIds();

    /// <summary>
    /// Get a snapshot of all DPS history (for persistence)
    /// Returns a deep copy that won't be affected by subsequent updates
    /// </summary>
    /// <returns>Dictionary of player ID to DPS history list</returns>
    Dictionary<long, List<ChartDataPoint>> GetDpsHistorySnapshot();

    /// <summary>
    /// Get a snapshot of all HPS history (for persistence)
    /// Returns a deep copy that won't be affected by subsequent updates
    /// </summary>
    /// <returns>Dictionary of player ID to HPS history list</returns>
    Dictionary<long, List<ChartDataPoint>> GetHpsHistorySnapshot();

    /// <summary>
    /// Load historical chart data (e.g., from database)
    /// Replaces current chart data with provided historical data
    /// </summary>
    /// <param name="dpsHistory">DPS history per player</param>
    /// <param name="hpsHistory">HPS history per player</param>
    void LoadHistoricalChartData(
        Dictionary<long, List<ChartDataPoint>> dpsHistory,
        Dictionary<long, List<ChartDataPoint>> hpsHistory);

    /// <summary>
    /// Indicates if the service is currently sampling data
    /// </summary>
    bool IsRunning { get; }
}
