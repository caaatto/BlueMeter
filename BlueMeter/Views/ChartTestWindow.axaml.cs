using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Standalone test window for the OxyPlot chart pipeline. Used during the
/// migration to validate that <see cref="OxyPlot.Avalonia.PlotView"/> renders
/// the same model that the WPF <c>OxyPlot.Wpf</c> control did.
/// </summary>
public partial class ChartTestWindow : Window
{
    public ChartTestWindow()
    {
        InitializeComponent();
    }

    public ChartTestWindow(ChartTestViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
