using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueMeter.WPF.Data;
using BlueMeter.WPF.Services;
using Microsoft.Extensions.Logging;
using OxyPlot;
using OxyPlot.Series;
using BlueMeter.Assets;
using BlueMeter.Core;
using BlueMeter.Core.Data.Database;
using BlueMeter.Core.Data.Models;
using Newtonsoft.Json;

namespace BlueMeter.WPF.ViewModels;

/// <summary>
/// ViewModel for Skill Breakdown Chart
/// Shows skill damage distribution as a pie chart for a selected player
/// </summary>
public partial class SkillBreakdownChartViewModel : ObservableObject
{
    private readonly ILogger<SkillBreakdownChartViewModel> _logger;
    private readonly IDataStorage _dataStorage;
    private readonly DispatcherTimer _updateTimer;

    [ObservableProperty]
    private PlotModel _plotModel;

    [ObservableProperty]
    private List<PlayerSelectionItem> _availablePlayers = new();

    [ObservableProperty]
    private PlayerSelectionItem? _selectedPlayer;

    [ObservableProperty]
    private int _topSkillsLimit = 10;

    [ObservableProperty]
    private string _statusText = "No player selected";

    [ObservableProperty]
    private bool _isHistoricalDataMode = false;

    private EncounterData? _loadedEncounter;

    // Pie chart colors
    private readonly List<OxyColor> _skillColors = new()
    {
        OxyColor.FromRgb(0, 122, 204),   // Blue
        OxyColor.FromRgb(255, 99, 71),   // Red
        OxyColor.FromRgb(50, 205, 50),   // Green
        OxyColor.FromRgb(255, 165, 0),   // Orange
        OxyColor.FromRgb(147, 112, 219), // Purple
        OxyColor.FromRgb(255, 20, 147),  // Pink
        OxyColor.FromRgb(64, 224, 208),  // Turquoise
        OxyColor.FromRgb(255, 215, 0),   // Gold
        OxyColor.FromRgb(255, 105, 180), // Hot Pink
        OxyColor.FromRgb(0, 191, 255),   // Deep Sky Blue
        OxyColor.FromRgb(50, 255, 127),  // Spring Green
        OxyColor.FromRgb(255, 140, 0),   // Dark Orange
        OxyColor.FromRgb(138, 43, 226),  // Blue Violet
        OxyColor.FromRgb(255, 69, 0),    // Red Orange
        OxyColor.FromRgb(0, 255, 255),   // Cyan
    };

    public SkillBreakdownChartViewModel(
        ILogger<SkillBreakdownChartViewModel> logger,
        IDataStorage dataStorage,
        Dispatcher dispatcher)
    {
        _logger = logger;
        _dataStorage = dataStorage;

        // Initialize PlotModel
        _plotModel = CreatePlotModel();

        // Update timer (1000ms - slower updates for pie chart)
        _updateTimer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };
        _updateTimer.Tick += OnUpdateTick;
        _updateTimer.Start();

        _logger.LogDebug("SkillBreakdownChartViewModel created, update interval: 1000ms");
    }

    private PlotModel CreatePlotModel()
    {
        var model = new PlotModel
        {
            Title = "Skill Damage Breakdown",
            Background = OxyColor.FromRgb(30, 30, 30), // Dark background
            TextColor = OxyColors.White,
            TitleColor = OxyColors.White,
            PlotAreaBorderColor = OxyColor.FromRgb(63, 63, 70)
        };

        return model;
    }

    private void OnUpdateTick(object? sender, EventArgs e)
    {
        try
        {
            if (!IsHistoricalDataMode)
            {
                UpdateAvailablePlayers();
                if (SelectedPlayer != null)
                {
                    UpdateChart();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skill breakdown chart");
        }
    }

    /// <summary>
    /// Update the list of available players from current combat data
    /// </summary>
    private void UpdateAvailablePlayers()
    {
        // Get all players from current combat data (use Full instead of Sectioned to include "Last" data)
        var currentPlayerIds = _dataStorage.ReadOnlyPlayerInfoDatas.Keys.ToList();
        var dpsPlayerIds = _dataStorage.ReadOnlyFullDpsDatas.Keys.ToList();

        // Get players that have both info and DPS data
        var activePlayerIds = currentPlayerIds.Intersect(dpsPlayerIds).ToList();

        // Check if player list has changed
        var currentAvailableIds = AvailablePlayers.Select(p => p.PlayerId).ToList();
        if (activePlayerIds.SequenceEqual(currentAvailableIds))
        {
            return; // No changes
        }

        // Rebuild player list
        var newPlayers = new List<PlayerSelectionItem>();
        foreach (var playerId in activePlayerIds)
        {
            if (_dataStorage.ReadOnlyPlayerInfoDatas.TryGetValue(playerId, out var playerInfo))
            {
                newPlayers.Add(new PlayerSelectionItem
                {
                    PlayerId = playerId,
                    PlayerName = playerInfo.Name ?? $"Player {playerId}"
                });
            }
        }

        // Sort by name
        newPlayers = newPlayers.OrderBy(p => p.PlayerName).ToList();

        // Update collection
        AvailablePlayers = newPlayers;

        // If current selection is no longer valid, clear it
        if (SelectedPlayer != null && !newPlayers.Any(p => p.PlayerId == SelectedPlayer.PlayerId))
        {
            SelectedPlayer = null;
            StatusText = "Selected player no longer available";
        }

        _logger.LogDebug("Updated available players: {Count} players", newPlayers.Count);
    }

    private void UpdateChart()
    {
        if (SelectedPlayer == null)
        {
            PlotModel.Series.Clear();
            PlotModel.Title = "Skill Damage Breakdown - No Player Selected";
            PlotModel.InvalidatePlot(true);
            StatusText = "No player selected";
            return;
        }

        // Get skill list based on mode
        List<(string SkillName, double TotalDamage)> skills;

        if (IsHistoricalDataMode && _loadedEncounter != null)
        {
            skills = GetSkillsFromHistoricalData(SelectedPlayer.PlayerId);
        }
        else
        {
            skills = GetSkillsFromLiveData(SelectedPlayer.PlayerId);
        }

        if (skills == null || skills.Count == 0)
        {
            PlotModel.Series.Clear();
            PlotModel.Title = $"Skill Damage Breakdown - {SelectedPlayer.PlayerName}";
            PlotModel.InvalidatePlot(true);
            StatusText = "No skill data available";
            return;
        }

        // Take top N skills
        var topSkills = skills.Take(TopSkillsLimit).ToList();
        var totalDamage = topSkills.Sum(s => s.TotalDamage);

        // If there are more skills, group the rest as "Other"
        if (skills.Count > TopSkillsLimit)
        {
            var otherDamage = skills.Skip(TopSkillsLimit).Sum(s => s.TotalDamage);
            if (otherDamage > 0)
            {
                topSkills.Add(("Other (" + (skills.Count - TopSkillsLimit) + " skills)", otherDamage));
                totalDamage += otherDamage;
            }
        }

        // Create pie series
        var pieSeries = new PieSeries
        {
            StrokeThickness = 2,
            Stroke = OxyColor.FromRgb(30, 30, 30),
            InsideLabelColor = OxyColors.White,
            InsideLabelPosition = 0.5,
            AngleSpan = 360,
            StartAngle = 0,
            TextColor = OxyColors.White,
            FontSize = 12,
            OutsideLabelFormat = "{0}: {2:P1}"
        };

        // Add slices
        for (int i = 0; i < topSkills.Count; i++)
        {
            var skill = topSkills[i];
            pieSeries.Slices.Add(new PieSlice(skill.SkillName, skill.TotalDamage)
            {
                Fill = _skillColors[i % _skillColors.Count],
                IsExploded = false
            });
        }

        // Update plot model
        PlotModel.Series.Clear();
        PlotModel.Series.Add(pieSeries);

        var dataMode = IsHistoricalDataMode ? " (Historical)" : "";
        PlotModel.Title = $"Skill Damage Breakdown - {SelectedPlayer.PlayerName}{dataMode}";
        PlotModel.InvalidatePlot(true);

        // Update status
        StatusText = $"Showing top {topSkills.Count} skills • Total Damage: {totalDamage:N0}{(IsHistoricalDataMode ? " (Historical Data)" : "")}";

        _logger.LogDebug("Updated skill breakdown chart for player {PlayerId}: {SkillCount} skills",
            SelectedPlayer.PlayerId, topSkills.Count);
    }

    /// <summary>
    /// Get skills from live data
    /// </summary>
    private List<(string SkillName, double TotalDamage)> GetSkillsFromLiveData(long playerId)
    {
        // Get DPS data for selected player (use Full to include "Last" data)
        if (!_dataStorage.ReadOnlyFullDpsDatas.TryGetValue(playerId, out var dpsData))
        {
            return new List<(string, double)>();
        }

        // Get skill list from DPS data
        return dpsData.ReadOnlySkillDatas.Values
            .Select(skill =>
            {
                // Get skill name from EmbeddedSkillConfig
                var skillIdText = skill.SkillId.ToString();
                var skillName = EmbeddedSkillConfig.TryGet(skillIdText, out var definition)
                    ? definition.Name
                    : skillIdText;

                // Translate skill name using DeepL if available
                var translatedSkillName = DpsStatisticsSubViewModel.GetTranslator()?.Translate(skillName) ?? skillName;

                return (SkillName: translatedSkillName, TotalDamage: (double)skill.TotalValue);
            })
            .OrderByDescending(s => s.TotalDamage)
            .ToList();
    }

    /// <summary>
    /// Get skills from historical encounter data
    /// </summary>
    private List<(string SkillName, double TotalDamage)> GetSkillsFromHistoricalData(long playerId)
    {
        if (_loadedEncounter == null || !_loadedEncounter.PlayerStats.TryGetValue(playerId, out var playerData))
        {
            return new List<(string, double)>();
        }

        // Parse skill data from JSON
        if (string.IsNullOrEmpty(playerData.SkillDataJson))
        {
            return new List<(string, double)>();
        }

        try
        {
            var skillDataDict = JsonConvert.DeserializeObject<Dictionary<long, SkillData>>(playerData.SkillDataJson);
            if (skillDataDict == null)
            {
                return new List<(string, double)>();
            }

            return skillDataDict.Values
                .Select(skill =>
                {
                    // Get skill name from EmbeddedSkillConfig
                    var skillIdText = skill.SkillId.ToString();
                    var skillName = EmbeddedSkillConfig.TryGet(skillIdText, out var definition)
                        ? definition.Name
                        : skillIdText;

                    // Translate skill name using DeepL if available
                    var translatedSkillName = DpsStatisticsSubViewModel.GetTranslator()?.Translate(skillName) ?? skillName;

                    return (SkillName: translatedSkillName, TotalDamage: (double)skill.TotalValue);
                })
                .OrderByDescending(s => s.TotalDamage)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing skill data JSON for player {PlayerId}", playerId);
            return new List<(string, double)>();
        }
    }

    /// <summary>
    /// Load historical encounter data
    /// </summary>
    public void LoadHistoricalEncounter(EncounterData encounterData)
    {
        _logger.LogInformation("Loading historical encounter data for Skill Breakdown Chart");

        _loadedEncounter = encounterData;
        IsHistoricalDataMode = true;

        // Stop auto-updates
        _updateTimer.Stop();

        // Update player list from historical data
        UpdateAvailablePlayersFromHistoricalData();

        // Update chart
        UpdateChart();

        _logger.LogInformation("Historical encounter loaded with {PlayerCount} players", encounterData.PlayerStats.Count);
    }

    /// <summary>
    /// Restore live data mode
    /// </summary>
    public void RestoreLiveData()
    {
        _logger.LogInformation("Restoring live data mode for Skill Breakdown Chart");

        _loadedEncounter = null;
        IsHistoricalDataMode = false;

        // Restart auto-updates
        _updateTimer.Start();

        // Update player list from live data
        UpdateAvailablePlayers();

        // Update chart
        UpdateChart();

        _logger.LogInformation("Live data mode restored");
    }

    /// <summary>
    /// Update available players from historical encounter data
    /// </summary>
    private void UpdateAvailablePlayersFromHistoricalData()
    {
        if (_loadedEncounter == null)
        {
            AvailablePlayers = new List<PlayerSelectionItem>();
            return;
        }

        var newPlayers = new List<PlayerSelectionItem>();

        foreach (var kvp in _loadedEncounter.PlayerStats)
        {
            // Skip NPCs
            if (kvp.Value.IsNpcData) continue;

            newPlayers.Add(new PlayerSelectionItem
            {
                PlayerId = kvp.Key,
                PlayerName = kvp.Value.Name ?? $"Player {kvp.Key}"
            });
        }

        // Sort by name
        newPlayers = newPlayers.OrderBy(p => p.PlayerName).ToList();

        // Update collection
        AvailablePlayers = newPlayers;

        _logger.LogDebug("Updated available players from historical data: {Count} players", newPlayers.Count);
    }

    /// <summary>
    /// Set the focused player ID and auto-select in dropdown
    /// </summary>
    public void SetFocusedPlayer(long? playerId)
    {
        if (!playerId.HasValue)
        {
            _logger.LogInformation("Skill Breakdown Chart: No focused player");
            return;
        }

        _logger.LogInformation("Skill Breakdown Chart focused player set to: {PlayerId}", playerId.Value);

        // Update available players first
        if (IsHistoricalDataMode)
        {
            UpdateAvailablePlayersFromHistoricalData();
        }
        else
        {
            UpdateAvailablePlayers();
        }

        // Find and select the player
        var player = AvailablePlayers.FirstOrDefault(p => p.PlayerId == playerId.Value);
        if (player != null)
        {
            SelectedPlayer = player;
            _logger.LogInformation("Auto-selected player: {PlayerName}", player.PlayerName);

            // Force immediate chart update
            UpdateChart();
        }
        else
        {
            _logger.LogWarning("Focused player {PlayerId} not found in available players", playerId.Value);
        }
    }

    /// <summary>
    /// Handle selected player change
    /// </summary>
    partial void OnSelectedPlayerChanged(PlayerSelectionItem? value)
    {
        _logger.LogInformation("Selected player changed: {PlayerName}", value?.PlayerName ?? "None");
        UpdateChart();
    }

    /// <summary>
    /// Stop the update timer when view is unloaded
    /// </summary>
    public void OnViewUnloaded()
    {
        _updateTimer.Stop();
        _logger.LogDebug("SkillBreakdownChartViewModel update timer stopped");
    }

    /// <summary>
    /// Restart the update timer when view is loaded
    /// </summary>
    public void OnViewLoaded()
    {
        if (!_updateTimer.IsEnabled && !IsHistoricalDataMode)
        {
            _updateTimer.Start();
            _logger.LogDebug("SkillBreakdownChartViewModel update timer started");
        }
    }
}

/// <summary>
/// Player selection item for dropdown
/// </summary>
public class PlayerSelectionItem
{
    public long PlayerId { get; init; }
    public string PlayerName { get; init; } = string.Empty;

    public override string ToString() => PlayerName;
}
