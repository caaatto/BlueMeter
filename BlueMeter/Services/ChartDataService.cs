using System.Collections.ObjectModel;
using Avalonia.Threading;
using BlueMeter.Models;
using BlueMeter.WPF.Data; // IDataStorage lives under this namespace in BlueMeter.Core (legacy WPF naming)
using Microsoft.Extensions.Logging;

namespace BlueMeter.Services;

/// <summary>
/// Background service for collecting chart data at regular intervals
/// Samples InstantDps/InstantHps from DpsData every 200ms
/// Maintains time-series history with FIFO cleanup (max 500 points per player)
/// </summary>
public sealed class ChartDataService : IChartDataService
{
    private readonly ILogger<ChartDataService> _logger;
    private readonly IDataStorage _dataStorage;
    private readonly DispatcherTimer _samplingTimer;

    // History storage: playerId -> time-series data
    private readonly Dictionary<long, ObservableCollection<ChartDataPoint>> _dpsHistory = new();
    private readonly Dictionary<long, ObservableCollection<ChartDataPoint>> _hpsHistory = new();

    private const int MaxHistoryPoints = 500;
    private const int SamplingIntervalMs = 200; // 200ms = 5 samples per second

    private bool _isDisposed;

    public bool IsRunning => _samplingTimer.IsEnabled;

    /// <summary>
    /// Event fired BEFORE chart history is cleared
    /// Provides snapshots of the data so subscribers can save it
    /// </summary>
    public event EventHandler<ChartHistoryClearingEventArgs>? BeforeHistoryCleared;

    public ChartDataService(
        ILogger<ChartDataService> logger,
        IDataStorage dataStorage)
    {
        _logger = logger;
        _dataStorage = dataStorage;

        _samplingTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(SamplingIntervalMs)
        };
        _samplingTimer.Tick += OnSamplingTick;

        // Subscribe to section events to clear history
        _dataStorage.NewSectionCreated += OnNewSectionCreated;

        _logger.LogDebug("ChartDataService created (sampling interval: {Interval}ms)", SamplingIntervalMs);
    }

    public void Start()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ChartDataService));

        if (_samplingTimer.IsEnabled)
        {
            _logger.LogWarning("ChartDataService already started");
            return;
        }

        _samplingTimer.Start();
        _logger.LogInformation("ChartDataService started (sampling at {Interval}ms intervals)", SamplingIntervalMs);
    }

    public void Stop()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ChartDataService));

        if (!_samplingTimer.IsEnabled)
        {
            _logger.LogWarning("ChartDataService already stopped");
            return;
        }

        _samplingTimer.Stop();
        _logger.LogInformation("ChartDataService stopped");
    }

    private void OnSamplingTick(object? sender, EventArgs e)
    {
        try
        {
            var now = DateTime.UtcNow;

            // Sample DPS/HPS for all active players in current section
            var sectionData = _dataStorage.ReadOnlySectionedDpsDatas;

            foreach (var kvp in sectionData)
            {
                var playerId = kvp.Key;
                var dpsData = kvp.Value;

                // Skip NPCs - only track real players
                if (dpsData.IsNpcData)
                    continue;

                // Ensure history collections exist for this player
                if (!_dpsHistory.ContainsKey(playerId))
                {
                    _dpsHistory[playerId] = new ObservableCollection<ChartDataPoint>();
                    _hpsHistory[playerId] = new ObservableCollection<ChartDataPoint>();
                }

                // Get instant DPS/HPS from sliding window
                var instantDps = dpsData.InstantDps;
                var instantHps = dpsData.InstantHps;

                // Add data points
                _dpsHistory[playerId].Add(new ChartDataPoint(now, instantDps));
                _hpsHistory[playerId].Add(new ChartDataPoint(now, instantHps));

                // Cleanup old data (FIFO - keep max 500 points)
                if (_dpsHistory[playerId].Count > MaxHistoryPoints)
                    _dpsHistory[playerId].RemoveAt(0);

                if (_hpsHistory[playerId].Count > MaxHistoryPoints)
                    _hpsHistory[playerId].RemoveAt(0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during chart data sampling");
        }
    }

    private void OnNewSectionCreated()
    {
        try
        {
            var dpsPlayerCount = _dpsHistory.Count;
            var hpsPlayerCount = _hpsHistory.Count;
            var totalDpsPoints = _dpsHistory.Sum(kvp => kvp.Value.Count);
            var totalHpsPoints = _hpsHistory.Sum(kvp => kvp.Value.Count);

            _logger.LogInformation("New section created - saving and clearing chart history ({DpsPlayers} DPS players, {HpsPlayers} HPS players, {DpsPoints} DPS points, {HpsPoints} HPS points)",
                dpsPlayerCount, hpsPlayerCount, totalDpsPoints, totalHpsPoints);

            // 1. FIRST: Create deep copy snapshots of current data
            var dpsSnapshot = new Dictionary<long, List<ChartDataPoint>>();
            foreach (var kvp in _dpsHistory)
            {
                dpsSnapshot[kvp.Key] = kvp.Value.Select(dp => new ChartDataPoint(dp.Timestamp, dp.Value)).ToList();
            }

            var hpsSnapshot = new Dictionary<long, List<ChartDataPoint>>();
            foreach (var kvp in _hpsHistory)
            {
                hpsSnapshot[kvp.Key] = kvp.Value.Select(dp => new ChartDataPoint(dp.Timestamp, dp.Value)).ToList();
            }

            _logger.LogDebug("Created chart history snapshots: {DpsPlayers} DPS players with {DpsPoints} points, {HpsPlayers} HPS players with {HpsPoints} points",
                dpsSnapshot.Count, dpsSnapshot.Sum(kvp => kvp.Value.Count),
                hpsSnapshot.Count, hpsSnapshot.Sum(kvp => kvp.Value.Count));

            // 2. SECOND: Fire event to allow subscribers to save the data
            // This happens BEFORE clearing, so data is available for saving
            if (BeforeHistoryCleared != null)
            {
                _logger.LogDebug("Firing BeforeHistoryCleared event with {DpsPlayers} DPS and {HpsPlayers} HPS players",
                    dpsSnapshot.Count, hpsSnapshot.Count);

                var eventArgs = new ChartHistoryClearingEventArgs(dpsSnapshot, hpsSnapshot);
                BeforeHistoryCleared.Invoke(this, eventArgs);

                _logger.LogDebug("BeforeHistoryCleared event completed");
            }
            else
            {
                _logger.LogWarning("BeforeHistoryCleared event has no subscribers - chart data will be lost!");
            }

            // 3. THIRD: Now safe to clear the history
            _dpsHistory.Clear();
            _hpsHistory.Clear();

            _logger.LogInformation("Chart history cleared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnNewSectionCreated while saving/clearing chart history");
        }
    }

    public ObservableCollection<ChartDataPoint>? GetDpsHistory(long playerId)
    {
        return _dpsHistory.TryGetValue(playerId, out var history) ? history : null;
    }

    public ObservableCollection<ChartDataPoint>? GetHpsHistory(long playerId)
    {
        return _hpsHistory.TryGetValue(playerId, out var history) ? history : null;
    }

    public IReadOnlyCollection<long> GetTrackedPlayerIds()
    {
        return _dpsHistory.Keys.ToList().AsReadOnly();
    }

    public Dictionary<long, List<ChartDataPoint>> GetDpsHistorySnapshot()
    {
        var snapshot = new Dictionary<long, List<ChartDataPoint>>();
        foreach (var kvp in _dpsHistory)
        {
            // Deep copy: create new ChartDataPoint instances
            snapshot[kvp.Key] = kvp.Value.Select(dp => new ChartDataPoint(dp.Timestamp, dp.Value)).ToList();
        }
        _logger.LogDebug("Created DPS history snapshot: {PlayerCount} players, {TotalPoints} total points",
            snapshot.Count, snapshot.Sum(kvp => kvp.Value.Count));
        return snapshot;
    }

    public Dictionary<long, List<ChartDataPoint>> GetHpsHistorySnapshot()
    {
        var snapshot = new Dictionary<long, List<ChartDataPoint>>();
        foreach (var kvp in _hpsHistory)
        {
            // Deep copy: create new ChartDataPoint instances
            snapshot[kvp.Key] = kvp.Value.Select(dp => new ChartDataPoint(dp.Timestamp, dp.Value)).ToList();
        }
        _logger.LogDebug("Created HPS history snapshot: {PlayerCount} players, {TotalPoints} total points",
            snapshot.Count, snapshot.Sum(kvp => kvp.Value.Count));
        return snapshot;
    }

    public void LoadHistoricalChartData(
        Dictionary<long, List<ChartDataPoint>> dpsHistory,
        Dictionary<long, List<ChartDataPoint>> hpsHistory)
    {
        _dpsHistory.Clear();
        _hpsHistory.Clear();

        foreach (var kvp in dpsHistory)
        {
            _dpsHistory[kvp.Key] = new ObservableCollection<ChartDataPoint>(kvp.Value);
        }

        foreach (var kvp in hpsHistory)
        {
            _hpsHistory[kvp.Key] = new ObservableCollection<ChartDataPoint>(kvp.Value);
        }

        _logger.LogInformation("Loaded historical chart data: {DpsPlayers} DPS players, {HpsPlayers} HPS players",
            dpsHistory.Count, hpsHistory.Count);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _samplingTimer.Stop();
        _samplingTimer.Tick -= OnSamplingTick;
        _dataStorage.NewSectionCreated -= OnNewSectionCreated;

        _dpsHistory.Clear();
        _hpsHistory.Clear();

        _isDisposed = true;

        _logger.LogDebug("ChartDataService disposed");
    }
}
