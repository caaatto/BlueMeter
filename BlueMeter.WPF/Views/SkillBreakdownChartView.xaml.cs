using System.Windows.Controls;
using BlueMeter.WPF.ViewModels;

namespace BlueMeter.WPF.Views;

/// <summary>
/// Skill Breakdown Chart View
/// Displays pie chart of skill damage distribution for a selected player
/// </summary>
public partial class SkillBreakdownChartView : UserControl
{
    private readonly SkillBreakdownChartViewModel? _viewModel;

    public SkillBreakdownChartView(SkillBreakdownChartViewModel viewModel)
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
