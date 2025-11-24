using System.Collections.ObjectModel;
using System.Windows.Threading;
using BlueMeter.WPF.Data;
using BlueMeter.WPF.Models;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.Services;

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

    public ChartDataService(
        ILogger<ChartDataService> logger,
        IDataStorage dataStorage,
        Dispatcher dispatcher)
    {
        _logger = logger;
        _dataStorage = dataStorage;

        _samplingTimer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
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
            // Clear all history when new section starts
            _logger.LogInformation("New section created - clearing chart history ({DpsPlayers} DPS, {HpsPlayers} HPS tracked players)",
                _dpsHistory.Count, _hpsHistory.Count);

            _dpsHistory.Clear();
            _hpsHistory.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing chart history on new section");
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
