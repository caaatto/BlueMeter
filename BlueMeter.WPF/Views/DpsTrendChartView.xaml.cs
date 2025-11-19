using System.Windows.Controls;
using BlueMeter.WPF.ViewModels;

namespace BlueMeter.WPF.Views;

/// <summary>
/// DPS Trend Chart View
/// Real-time line chart showing DPS over time
/// </summary>
public partial class DpsTrendChartView : UserControl
{
    private readonly DpsTrendChartViewModel? _viewModel;

    public DpsTrendChartView(DpsTrendChartViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel?.OnViewLoaded();
    }

    private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel?.OnViewUnloaded();
    }
}
