using System.Windows;
using BlueMeter.WPF.ViewModels;

namespace BlueMeter.WPF.Views;

/// <summary>
/// Test window for LiveCharts2 integration
/// </summary>
public partial class ChartTestWindow : Window
{
    public ChartTestWindow(ChartTestViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
