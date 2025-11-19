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

    public ChartsWindow(
        ChartsWindowViewModel viewModel,
        DpsTrendChartView dpsTrendChartView)
    {
        _viewModel = viewModel;
        _dpsTrendChartView = dpsTrendChartView;
        DataContext = _viewModel;

        InitializeComponent();

        // Inject the DPS Trend Chart view into the tab
        DpsTrendChartContainer.Content = _dpsTrendChartView;

        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnWindowLoaded();
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _viewModel.OnWindowClosing();
    }

    /// <summary>
    /// Set the focused player for the charts
    /// </summary>
    public void SetFocusedPlayer(long? playerId)
    {
        _viewModel.SetFocusedPlayer(playerId);

        // Also notify the DPS Trend Chart ViewModel
        if (_dpsTrendChartView.DataContext is DpsTrendChartViewModel dpsTrendViewModel)
        {
            dpsTrendViewModel.SetFocusedPlayer(playerId);
        }
    }
}
