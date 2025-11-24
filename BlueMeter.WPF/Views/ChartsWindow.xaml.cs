using System.Windows;
using BlueMeter.WPF.ViewModels;

namespace BlueMeter.WPF.Views;

/// <summary>
/// Advanced Combat Log - Charts Window
/// Displays real-time DPS/HPS charts and analytics
/// </summary>
public partial class ChartsWindow : Window
{
    private readonly ChartsWindowViewModel _viewModel;
    private readonly DpsTrendChartView _dpsTrendChartView;
    private readonly SkillBreakdownChartView _skillBreakdownChartView;

    public ChartsWindow(
        ChartsWindowViewModel viewModel,
        DpsTrendChartView dpsTrendChartView,
        SkillBreakdownChartView skillBreakdownChartView)
    {
        _viewModel = viewModel;
        _dpsTrendChartView = dpsTrendChartView;
        _skillBreakdownChartView = skillBreakdownChartView;
        DataContext = _viewModel;

        InitializeComponent();

        // Inject the chart views into their respective tabs
        DpsTrendChartContainer.Content = _dpsTrendChartView;
        SkillBreakdownChartContainer.Content = _skillBreakdownChartView;

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnWindowLoaded();

        // Subscribe to ViewModel events
        _viewModel.HistoricalEncounterLoaded += OnHistoricalEncounterLoaded;
        _viewModel.LiveDataRestored += OnLiveDataRestored;
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Unsubscribe from ViewModel events
        _viewModel.HistoricalEncounterLoaded -= OnHistoricalEncounterLoaded;
        _viewModel.LiveDataRestored -= OnLiveDataRestored;

        _viewModel.OnWindowClosing();
    }

    /// <summary>
    /// Handle historical encounter loaded
    /// </summary>
    private void OnHistoricalEncounterLoaded(Core.Data.Database.EncounterData encounterData)
    {
        // Notify SkillBreakdownChart ViewModel
        if (_skillBreakdownChartView.DataContext is SkillBreakdownChartViewModel skillViewModel)
        {
            skillViewModel.LoadHistoricalEncounter(encounterData);
        }

        // Note: DpsTrendChart doesn't support historical data yet
        // if (_dpsTrendChartView.DataContext is DpsTrendChartViewModel dpsViewModel)
        // {
        //     dpsViewModel.LoadHistoricalEncounter(encounterData);
        // }
    }

    /// <summary>
    /// Handle live data restored
    /// </summary>
    private void OnLiveDataRestored()
    {
        // Notify SkillBreakdownChart ViewModel
        if (_skillBreakdownChartView.DataContext is SkillBreakdownChartViewModel skillViewModel)
        {
            skillViewModel.RestoreLiveData();
        }

        // if (_dpsTrendChartView.DataContext is DpsTrendChartViewModel dpsViewModel)
        // {
        //     dpsViewModel.RestoreLiveData();
        // }
    }

    /// <summary>
    /// Set the focused player for the charts
    /// </summary>
    public void SetFocusedPlayer(long? playerId)
    {
        _viewModel.SetFocusedPlayer(playerId);

        // Notify chart ViewModels
        if (_dpsTrendChartView.DataContext is DpsTrendChartViewModel dpsTrendViewModel)
        {
            dpsTrendViewModel.SetFocusedPlayer(playerId);
        }

        if (_skillBreakdownChartView.DataContext is SkillBreakdownChartViewModel skillBreakdownViewModel)
        {
            skillBreakdownViewModel.SetFocusedPlayer(playerId);
        }
    }
}
