using CommunityToolkit.Mvvm.ComponentModel;
using BlueMeter.Services;
using BlueMeter.WPF.Data; // IDataStorage lives under this legacy namespace inside BlueMeter.Core
using Microsoft.Extensions.Logging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Avalonia.Threading;

namespace BlueMeter.ViewModels;

/// <summary>
/// ViewModel for DPS Trend Chart
/// Real-time line chart showing DPS over time for all tracked players
/// </summary>
public partial class DpsTrendChartViewModel : ObservableObject
{
    private readonly ILogger<DpsTrendChartViewModel> _logger;
    private readonly IChartDataService _chartDataService;
    private readonly IDataStorage _dataStorage;
    private readonly DispatcherTimer _updateTimer;

    [ObservableProperty]
    private PlotModel _plotModel;

    [ObservableProperty]
    private long? _focusedPlayerId = null;

    [ObservableProperty]
    private bool _isHistoricalDataMode = false;

    private Core.Data.Database.EncounterData? _loadedEncounter;

    // Player colors for line series
    private readonly Dictionary<long, OxyColor> _playerColors = new();
    private readonly List<OxyColor> _availableColors = new()
    {
        OxyColor.FromRgb(0, 122, 204),  // Blue
        OxyColor.FromRgb(255, 99, 71),  // Red
        OxyColor.FromRgb(50, 205, 50),  // Green
        OxyColor.FromRgb(255, 165, 0),  // Orange
        OxyColor.FromRgb(147, 112, 219),// Purple
        OxyColor.FromRgb(255, 20, 147), // Pink
        OxyColor.FromRgb(64, 224, 208), // Turquoise
        OxyColor.FromRgb(255, 215, 0),  // Gold
    };
    private int _colorIndex = 0;

    public DpsTrendChartViewModel(
        ILogger<DpsTrendChartViewModel> logger,
        IChartDataService chartDataService,
        IDataStorage dataStorage)
    {
        _logger = logger;
        _chartDataService = chartDataService;
        _dataStorage = dataStorage;

        // Initialize PlotModel
        _plotModel = CreatePlotModel();

        // Update timer (500ms for smooth updates without too much overhead)
        _updateTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _updateTimer.Tick += OnUpdateTick;
        // Don't start timer immediately - wait for view to load
        // _updateTimer.Start();

        _logger.LogDebug("DpsTrendChartViewModel created, update interval: 500ms (timer not started yet)");
    }

    private PlotModel CreatePlotModel()
    {
        var model = new PlotModel
        {
            Title = "DPS Trend (Real-time)",
            Background = OxyColor.FromRgb(30, 30, 30), // Dark background
            TextColor = OxyColors.White,
            TitleColor = OxyColors.White,
            PlotAreaBorderColor = OxyColor.FromRgb(63, 63, 70)
        };

        // X-Axis (Time in seconds)
        var xAxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Time (seconds)",
            TitleColor = OxyColors.White,
            TextColor = OxyColors.White,
            TicklineColor = OxyColor.FromRgb(63, 63, 70),
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColor.FromRgb(45, 45, 48),
            MinorGridlineStyle = LineStyle.Dot,
            MinorGridlineColor = OxyColor.FromRgb(40, 40, 43),
            AxislineColor = OxyColor.FromRgb(63, 63, 70),
            Minimum = 0,
            Maximum = 60 // Show last 60 seconds by default
        };
        model.Axes.Add(xAxis);

        // Y-Axis (DPS)
        var yAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "DPS",
            TitleColor = OxyColors.White,
            TextColor = OxyColors.White,
            TicklineColor = OxyColor.FromRgb(63, 63, 70),
            MajorGridlineStyle = LineStyle.Solid,
            MajorGridlineColor = OxyColor.FromRgb(45, 45, 48),
            MinorGridlineStyle = LineStyle.Dot,
            MinorGridlineColor = OxyColor.FromRgb(40, 40, 43),
            AxislineColor = OxyColor.FromRgb(63, 63, 70),
            Minimum = 0,
            StringFormat = "N0" // No decimals for DPS
        };
        model.Axes.Add(yAxis);

        // Legend
        var legend = new Legend
        {
            LegendPosition = LegendPosition.TopRight,
            LegendPlacement = LegendPlacement.Inside,
            LegendBackground = OxyColor.FromArgb(200, 30, 30, 30),
            LegendBorder = OxyColor.FromRgb(63, 63, 70),
            LegendTextColor = OxyColors.White,
            LegendBorderThickness = 1
        };
        model.Legends.Add(legend);

        return model;
    }

    private void OnUpdateTick(object? sender, EventArgs e)
    {
        try
        {
            UpdateChart();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DPS trend chart");
        }
    }

    private void UpdateChart()
    {
        // In historical mode, don't update from live data
        if (IsHistoricalDataMode)
        {
            return;
        }

        var trackedPlayerIds = _chartDataService.GetTrackedPlayerIds();

        // Show helpful message if no players are tracked
        if (trackedPlayerIds.Count == 0)
        {
            PlotModel.Title = "DPS Trend - Waiting for combat data...";
            PlotModel.InvalidatePlot(true);
            return;
        }

        PlotModel.Title = "DPS Trend (Real-time)";

        // Remove series for players that are no longer tracked
        var seriesToRemove = PlotModel.Series
            .OfType<LineSeries>()
            .Where(s => s.Tag is long playerId && !trackedPlayerIds.Contains(playerId))
            .ToList();

        foreach (var series in seriesToRemove)
        {
            PlotModel.Series.Remove(series);
            if (series.Tag is long playerId)
            {
                _playerColors.Remove(playerId);
            }
        }

        // Track earliest timestamp for X-axis calculation
        DateTime? earliestTimestamp = null;

        // Update or create series for each tracked player
        foreach (var playerId in trackedPlayerIds)
        {
            var dpsHistory = _chartDataService.GetDpsHistory(playerId);
            if (dpsHistory == null || dpsHistory.Count == 0)
                continue;

            // Find or create series for this player
            var series = PlotModel.Series
                .OfType<LineSeries>()
                .FirstOrDefault(s => s.Tag is long id && id == playerId);

            if (series == null)
            {
                // Create new series
                var color = AssignColorToPlayer(playerId);
                var playerName = GetPlayerName(playerId);
                var isFocused = FocusedPlayerId.HasValue && FocusedPlayerId.Value == playerId;

                series = new LineSeries
                {
                    Tag = playerId,
                    Title = playerName,
                    Color = color,
                    StrokeThickness = isFocused ? 4 : 2, // Thicker line for focused player
                    LineStyle = LineStyle.Solid,
                    MarkerType = MarkerType.None
                };
                PlotModel.Series.Add(series);
                _logger.LogDebug("Created new DPS series for player {PlayerId} ({PlayerName})", playerId, playerName);
            }
            else
            {
                // Update existing series styling if focus changed
                var isFocused = FocusedPlayerId.HasValue && FocusedPlayerId.Value == playerId;
                series.StrokeThickness = isFocused ? 4 : 2;
                series.Title = GetPlayerName(playerId); // Update name in case it changed
            }

            // Get base timestamp (earliest point in this player's history)
            if (dpsHistory.Count > 0)
            {
                var baseTime = dpsHistory[0].Timestamp;
                if (earliestTimestamp == null || baseTime < earliestTimestamp)
                {
                    earliestTimestamp = baseTime;
                }

                // Convert to OxyPlot data points
                var dataPoints = dpsHistory
                    .Select(dp => dp.ToOxyDataPoint(baseTime))
                    .ToList();

                // Update series points
                series.Points.Clear();
                series.Points.AddRange(dataPoints);
            }
        }

        // Auto-adjust X-axis to show all data
        if (earliestTimestamp.HasValue && trackedPlayerIds.Any())
        {
            var xAxis = PlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            if (xAxis != null)
            {
                var allPoints = PlotModel.Series
                    .OfType<LineSeries>()
                    .SelectMany(s => s.Points)
                    .ToList();

                var maxTime = allPoints.Any() ? allPoints.Max(p => p.X) : 60;

                xAxis.Minimum = 0;
                xAxis.Maximum = Math.Max(60, maxTime + 5); // At least 60s, or max + 5s buffer
            }
        }

        // Refresh the plot
        PlotModel.InvalidatePlot(true);
    }

    private OxyColor AssignColorToPlayer(long playerId)
    {
        if (_playerColors.TryGetValue(playerId, out var existingColor))
        {
            return existingColor;
        }

        // Assign next available color
        var color = _availableColors[_colorIndex % _availableColors.Count];
        _colorIndex++;
        _playerColors[playerId] = color;
        return color;
    }

    private string GetPlayerName(long playerId)
    {
        var playerInfo = _dataStorage.ReadOnlyPlayerInfoDatas.TryGetValue(playerId, out var info) ? info : null;
        if (playerInfo != null && !string.IsNullOrEmpty(playerInfo.Name))
        {
            return playerInfo.Name;
        }
        return $"Player {playerId}";
    }

    /// <summary>
    /// Set the focused player ID and refresh the chart
    /// </summary>
    public void SetFocusedPlayer(long? playerId)
    {
        if (FocusedPlayerId != playerId)
        {
            FocusedPlayerId = playerId;
            _logger.LogInformation("DPS Trend Chart focused player set to: {PlayerId}", playerId);

            // Force immediate chart update to reflect new focus
            UpdateChart();
        }
    }

    /// <summary>
    /// Stop the update timer when view is unloaded
    /// </summary>
    public void OnViewUnloaded()
    {
        _updateTimer.Stop();
        _logger.LogDebug("DpsTrendChartViewModel update timer stopped");
    }

    /// <summary>
    /// Restart the update timer when view is loaded
    /// </summary>
    public void OnViewLoaded()
    {
        _logger.LogDebug("DpsTrendChartViewModel view loaded");

        // Do initial chart update to load any existing data
        UpdateChart();

        // Start timer if not already running and not in historical mode
        if (!_updateTimer.IsEnabled && !IsHistoricalDataMode)
        {
            _updateTimer.Start();
            _logger.LogDebug("DpsTrendChartViewModel update timer started");
        }
    }

    /// <summary>
    /// Load historical encounter data
    /// </summary>
    public void LoadHistoricalEncounter(Core.Data.Database.EncounterData encounterData)
    {
        _logger.LogInformation("Loading historical encounter data for DPS Trend Chart");

        _loadedEncounter = encounterData;
        IsHistoricalDataMode = true;

        // Stop auto-updates
        _updateTimer.Stop();

        // Update chart with historical data
        UpdateChartFromHistoricalData();

        _logger.LogInformation("Historical encounter loaded with {PlayerCount} players", encounterData.PlayerStats.Count);
    }

    /// <summary>
    /// Restore live data mode
    /// </summary>
    public void RestoreLiveData()
    {
        _logger.LogInformation("Restoring live data mode for DPS Trend Chart");

        _loadedEncounter = null;
        IsHistoricalDataMode = false;

        // Clear existing series
        PlotModel.Series.Clear();
        _playerColors.Clear();
        _colorIndex = 0;

        // Restart auto-updates
        _updateTimer.Start();

        // Update chart with live data
        UpdateChart();

        _logger.LogInformation("Live data mode restored");
    }

    /// <summary>
    /// Update chart from historical encounter data
    /// </summary>
    private void UpdateChartFromHistoricalData()
    {
        if (_loadedEncounter == null)
        {
            _logger.LogWarning("No historical encounter data available");
            return;
        }

        PlotModel.Series.Clear();
        _playerColors.Clear();
        _colorIndex = 0;

        PlotModel.Title = "DPS Trend (Historical)";

        // Build chart history from player stats
        var playersWithHistory = _loadedEncounter.PlayerStats
            .Where(kvp => kvp.Value.DpsHistory != null && kvp.Value.DpsHistory.Count > 0)
            .ToList();

        if (playersWithHistory.Count == 0)
        {
            PlotModel.Title = "DPS Trend - No chart data in this encounter";
            PlotModel.InvalidatePlot(true);
            _logger.LogWarning("No DPS history found in historical encounter");
            return;
        }

        // Find earliest timestamp for X-axis
        DateTime? earliestTimestamp = null;
        foreach (var kvp in playersWithHistory)
        {
            var history = kvp.Value.DpsHistory;
            if (history != null && history.Count > 0)
            {
                var baseTime = history[0].Timestamp;
                if (earliestTimestamp == null || baseTime < earliestTimestamp)
                {
                    earliestTimestamp = baseTime;
                }
            }
        }

        if (!earliestTimestamp.HasValue)
        {
            PlotModel.Title = "DPS Trend - No valid data points";
            PlotModel.InvalidatePlot(true);
            return;
        }

        // Create series for each player with history
        foreach (var kvp in playersWithHistory)
        {
            var playerId = kvp.Key;
            var playerData = kvp.Value;
            var history = playerData.DpsHistory;

            if (history == null || history.Count == 0)
                continue;

            // Skip NPCs
            if (playerData.IsNpcData)
                continue;

            // Create series
            var color = AssignColorToPlayer(playerId);
            var playerName = playerData.Name ?? $"Player {playerId}";
            var isFocused = FocusedPlayerId.HasValue && FocusedPlayerId.Value == playerId;

            var series = new LineSeries
            {
                Tag = playerId,
                Title = playerName,
                Color = color,
                StrokeThickness = isFocused ? 4 : 2,
                LineStyle = LineStyle.Solid,
                MarkerType = MarkerType.None
            };

            // Convert to OxyPlot data points
            var dataPoints = history
                .Select(dp => new DataPoint((dp.Timestamp - earliestTimestamp.Value).TotalSeconds, dp.Value))
                .ToList();

            series.Points.AddRange(dataPoints);
            PlotModel.Series.Add(series);

            _logger.LogDebug("Created historical DPS series for player {PlayerId} ({PlayerName}) with {PointCount} points",
                playerId, playerName, dataPoints.Count);
        }

        // Auto-adjust X-axis
        var allPoints = PlotModel.Series
            .OfType<LineSeries>()
            .SelectMany(s => s.Points)
            .ToList();

        if (allPoints.Any())
        {
            var xAxis = PlotModel.Axes.FirstOrDefault(a => a.Position == AxisPosition.Bottom);
            if (xAxis != null)
            {
                var maxTime = allPoints.Max(p => p.X);
                xAxis.Minimum = 0;
                xAxis.Maximum = Math.Max(60, maxTime + 5);
            }
        }

        PlotModel.InvalidatePlot(true);
        _logger.LogInformation("DPS Trend chart updated with historical data: {SeriesCount} players", PlotModel.Series.Count);
    }

    /// <summary>
    /// Get player name from loaded encounter data
    /// </summary>
    private string GetPlayerNameFromEncounter(long playerId)
    {
        if (_loadedEncounter != null && _loadedEncounter.PlayerStats.TryGetValue(playerId, out var playerStats))
        {
            return playerStats.Name ?? $"Player {playerId}";
        }
        return $"Player {playerId}";
    }
}
