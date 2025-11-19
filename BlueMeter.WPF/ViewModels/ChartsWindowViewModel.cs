using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueMeter.WPF.Services;
using BlueMeter.Core.Data;
using BlueMeter.Core.Data.Database;
using BlueMeter.Core.Data.Models;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.ViewModels;

/// <summary>
/// ViewModel for the Charts Window
/// Manages chart tabs and data refresh
/// </summary>
public partial class ChartsWindowViewModel : ObservableObject
{
    private readonly ILogger<ChartsWindowViewModel> _logger;
    private readonly IChartDataService _chartDataService;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private bool _autoRefresh = true;

    [ObservableProperty]
    private long? _focusedPlayerId = null;

    [ObservableProperty]
    private ObservableCollection<CombatSelectionItem> _availableCombats = new();

    [ObservableProperty]
    private CombatSelectionItem? _selectedCombat;

    [ObservableProperty]
    private bool _isHistoricalDataMode = false;

    [ObservableProperty]
    private bool _isLoadingCombats = false;

    private EncounterData? _loadedEncounter;

    // Events to notify child ViewModels
    public event Action<EncounterData>? HistoricalEncounterLoaded;
    public event Action? LiveDataRestored;

    public ChartsWindowViewModel(
        ILogger<ChartsWindowViewModel> logger,
        IChartDataService chartDataService)
    {
        _logger = logger;
        _chartDataService = chartDataService;

        _logger.LogDebug("ChartsWindowViewModel created");
    }

    [RelayCommand]
    private void Refresh()
    {
        _logger.LogInformation("Manual chart refresh triggered");
        // Chart refresh will be handled by individual chart ViewModels
        // For now, just log
    }

    [RelayCommand]
    private void ToggleAutoRefresh()
    {
        AutoRefresh = !AutoRefresh;
        _logger.LogInformation("Auto-refresh toggled: {AutoRefresh}", AutoRefresh);
    }

    /// <summary>
    /// Called when window is loaded
    /// </summary>
    public async void OnWindowLoaded()
    {
        _logger.LogInformation("ChartsWindow loaded");

        // Ensure ChartDataService is running
        if (!_chartDataService.IsRunning)
        {
            _logger.LogWarning("ChartDataService not running, starting it now");
            _chartDataService.Start();
        }

        // Load recent combats
        await LoadRecentCombatsAsync();
    }

    /// <summary>
    /// Called when window is closing
    /// </summary>
    public void OnWindowClosing()
    {
        _logger.LogInformation("ChartsWindow closing");
    }

    /// <summary>
    /// Set the focused player ID for the charts
    /// </summary>
    public void SetFocusedPlayer(long? playerId)
    {
        FocusedPlayerId = playerId;
        _logger.LogInformation("Focused player set to: {PlayerId}", playerId);
    }

    /// <summary>
    /// Load recent combats (Current + Last 10 encounters)
    /// </summary>
    private async Task LoadRecentCombatsAsync()
    {
        IsLoadingCombats = true;

        try
        {
            var combats = new List<CombatSelectionItem>();

            // Add "Current Combat" option
            combats.Add(new CombatSelectionItem
            {
                EncounterId = null,
                DisplayName = "🟢 Current Combat (Live)",
                IsCurrentCombat = true,
                StartTime = null
            });

            // Load last 10 encounters from database
            try
            {
                var encounters = await DataStorageExtensions.GetRecentEncountersAsync(10);

                int index = 1;
                foreach (var encounter in encounters)
                {
                    var timeAgo = GetTimeAgo(encounter.StartTime);
                    var displayName = $"📜 Last {index} ({timeAgo}) - {encounter.BossName}";

                    combats.Add(new CombatSelectionItem
                    {
                        EncounterId = encounter.EncounterId,
                        DisplayName = displayName,
                        IsCurrentCombat = false,
                        StartTime = encounter.StartTime,
                        BossName = encounter.BossName,
                        Duration = TimeSpan.FromMilliseconds(encounter.DurationMs)
                    });

                    index++;
                }

                _logger.LogInformation("Loaded {Count} historical encounters", encounters.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load encounter history");
            }

            // Update collection
            AvailableCombats.Clear();
            foreach (var combat in combats)
            {
                AvailableCombats.Add(combat);
            }

            // Select "Current Combat" by default
            SelectedCombat = AvailableCombats.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recent combats");
        }
        finally
        {
            IsLoadingCombats = false;
        }
    }

    /// <summary>
    /// Called when selected combat changes
    /// </summary>
    partial void OnSelectedCombatChanged(CombatSelectionItem? value)
    {
        if (value == null) return;

        _logger.LogInformation("Selected combat changed: {DisplayName}", value.DisplayName);

        if (value.IsCurrentCombat)
        {
            // Switch back to live data
            RestoreLiveData();
        }
        else
        {
            // Load historical encounter
            _ = LoadHistoricalEncounterAsync(value.EncounterId!);
        }
    }

    /// <summary>
    /// Load historical encounter data
    /// </summary>
    private async Task LoadHistoricalEncounterAsync(string encounterId)
    {
        _logger.LogInformation("Loading historical encounter: {EncounterId}", encounterId);

        try
        {
            var encounterData = await DataStorageExtensions.LoadEncounterAsync(encounterId);

            if (encounterData == null)
            {
                _logger.LogWarning("Failed to load encounter {EncounterId}", encounterId);
                return;
            }

            _loadedEncounter = encounterData;
            IsHistoricalDataMode = true;
            AutoRefresh = false; // Disable auto-refresh for historical data

            // Notify child ViewModels
            HistoricalEncounterLoaded?.Invoke(encounterData);

            _logger.LogInformation("Historical encounter loaded: {EncounterId}, {PlayerCount} players",
                encounterId, encounterData.PlayerStats.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading historical encounter {EncounterId}", encounterId);
        }
    }

    /// <summary>
    /// Restore live data mode
    /// </summary>
    private void RestoreLiveData()
    {
        _logger.LogInformation("Restoring live data mode");

        _loadedEncounter = null;
        IsHistoricalDataMode = false;

        // Notify child ViewModels
        LiveDataRestored?.Invoke();
    }

    /// <summary>
    /// Get time ago string (e.g., "2m ago", "1h ago")
    /// </summary>
    private static string GetTimeAgo(DateTime startTime)
    {
        var timeSpan = DateTime.Now - startTime;

        if (timeSpan.TotalMinutes < 1)
            return "just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays}d ago";

        return startTime.ToString("MMM dd");
    }
}

/// <summary>
/// Combat selection item for dropdown
/// </summary>
public class CombatSelectionItem
{
    public string? EncounterId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public bool IsCurrentCombat { get; init; }
    public DateTime? StartTime { get; init; }
    public string BossName { get; init; } = "Unknown";
    public TimeSpan Duration { get; init; }
}
