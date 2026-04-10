using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Real-time DPS line chart driven by <see cref="DpsTrendChartViewModel"/>.
///
/// Port notes:
/// - Avalonia's <c>UserControl</c> doesn't expose <c>Loaded</c>/<c>Unloaded</c>
///   as routed events the same way WPF does. The handlers attach via
///   <c>AttachedToVisualTree</c>/<c>DetachedFromVisualTree</c> instead.
/// - <c>OxyPlot.Avalonia.OxyPlotModule</c> must be loaded before any chart is
///   instantiated; that initialization happens in <c>App.axaml.cs</c> on
///   first paint of any chart view.
/// </summary>
public partial class DpsTrendChartView : UserControl
{
    private readonly DpsTrendChartViewModel? _viewModel;

    public DpsTrendChartView()
    {
        InitializeComponent();
    }

    public DpsTrendChartView(DpsTrendChartViewModel viewModel)
        : this()
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        AttachedToVisualTree += OnAttached;
        DetachedFromVisualTree += OnDetached;
    }

    private void OnAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _viewModel?.OnViewLoaded();
    }

    private void OnDetached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _viewModel?.OnViewUnloaded();
    }
}
