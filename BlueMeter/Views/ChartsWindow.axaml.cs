using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BlueMeter.Core.Data.Database;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Advanced Combat Log - Charts Window.
/// Displays real-time DPS/HPS charts and analytics.
/// </summary>
public partial class ChartsWindow : Window
{
    private readonly ChartsWindowViewModel? _viewModel;
    private readonly DpsTrendChartView? _dpsTrendChartView;
    private readonly EnhancedSkillBreakdownView? _enhancedSkillBreakdownView;
    private ContentControl? _dpsTrendChartContainer;
    private ContentControl? _enhancedSkillBreakdownContainer;

    public ChartsWindow()
    {
        InitializeComponent();
    }

    public ChartsWindow(
        ChartsWindowViewModel viewModel,
        DpsTrendChartView dpsTrendChartView,
        EnhancedSkillBreakdownView enhancedSkillBreakdownView)
        : this()
    {
        _viewModel = viewModel;
        _dpsTrendChartView = dpsTrendChartView;
        _enhancedSkillBreakdownView = enhancedSkillBreakdownView;
        DataContext = _viewModel;

        if (_dpsTrendChartContainer is not null)
        {
            _dpsTrendChartContainer.Content = _dpsTrendChartView;
        }
        if (_enhancedSkillBreakdownContainer is not null)
        {
            _enhancedSkillBreakdownContainer.Content = _enhancedSkillBreakdownView;
        }

        Opened += OnOpened;
        Closing += OnClosing;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _dpsTrendChartContainer = this.FindControl<ContentControl>("DpsTrendChartContainer");
        _enhancedSkillBreakdownContainer = this.FindControl<ContentControl>("EnhancedSkillBreakdownContainer");
    }

    private void OnOpened(object? sender, System.EventArgs e)
    {
        if (_viewModel is null) return;

        _viewModel.OnWindowLoaded();

        _viewModel.HistoricalEncounterLoaded += OnHistoricalEncounterLoaded;
        _viewModel.LiveDataRestored += OnLiveDataRestored;
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_viewModel is null) return;

        _viewModel.HistoricalEncounterLoaded -= OnHistoricalEncounterLoaded;
        _viewModel.LiveDataRestored -= OnLiveDataRestored;

        _viewModel.OnWindowClosing();
    }

    private void OnHistoricalEncounterLoaded(EncounterData encounterData)
    {
        if (_dpsTrendChartView?.DataContext is DpsTrendChartViewModel dpsViewModel)
        {
            dpsViewModel.LoadHistoricalEncounter(encounterData);
        }

        if (_enhancedSkillBreakdownView?.DataContext is EnhancedSkillBreakdownViewModel enhancedViewModel)
        {
            enhancedViewModel.LoadHistoricalEncounter(encounterData);
        }
    }

    private void OnLiveDataRestored()
    {
        if (_dpsTrendChartView?.DataContext is DpsTrendChartViewModel dpsViewModel)
        {
            dpsViewModel.RestoreLiveData();
        }

        if (_enhancedSkillBreakdownView?.DataContext is EnhancedSkillBreakdownViewModel enhancedViewModel)
        {
            enhancedViewModel.RestoreLiveData();
        }
    }

    /// <summary>
    /// Set the focused player for the charts.
    /// </summary>
    public void SetFocusedPlayer(long? playerId)
    {
        _viewModel?.SetFocusedPlayer(playerId);

        if (_dpsTrendChartView?.DataContext is DpsTrendChartViewModel dpsTrendViewModel)
        {
            dpsTrendViewModel.SetFocusedPlayer(playerId);
        }

        if (_enhancedSkillBreakdownView?.DataContext is EnhancedSkillBreakdownViewModel enhancedBreakdownViewModel)
        {
            enhancedBreakdownViewModel.SetFocusedPlayer(playerId);
        }
    }
}
