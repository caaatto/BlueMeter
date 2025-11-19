using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BlueMeter.WPF.Services;
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
    public void OnWindowLoaded()
    {
        _logger.LogInformation("ChartsWindow loaded");

        // Ensure ChartDataService is running
        if (!_chartDataService.IsRunning)
        {
            _logger.LogWarning("ChartDataService not running, starting it now");
            _chartDataService.Start();
        }
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
}
