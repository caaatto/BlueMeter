using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Skill Breakdown Chart View — displays a pie chart of skill damage distribution
/// for a selected player. The chart model is owned by the view model.
/// </summary>
public partial class SkillBreakdownChartView : UserControl
{
    private readonly SkillBreakdownChartViewModel? _viewModel;

    public SkillBreakdownChartView()
    {
        InitializeComponent();
    }

    public SkillBreakdownChartView(SkillBreakdownChartViewModel viewModel)
        : this()
    {
        _viewModel = viewModel;
        DataContext = _viewModel;

        AttachedToVisualTree += OnAttached;
        DetachedFromVisualTree += OnDetached;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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
