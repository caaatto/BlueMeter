using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Views;

/// <summary>
/// Enhanced Skill Breakdown View with full StarResonanceDps-style stats.
/// Includes Lucky Hits tracking and detailed skill breakdown.
/// </summary>
public partial class EnhancedSkillBreakdownView : UserControl
{
    private readonly EnhancedSkillBreakdownViewModel? _viewModel;
    private readonly ILogger<EnhancedSkillBreakdownView>? _logger;

    public EnhancedSkillBreakdownView()
    {
        InitializeComponent();
    }

    public EnhancedSkillBreakdownView(
        EnhancedSkillBreakdownViewModel viewModel,
        ILogger<EnhancedSkillBreakdownView> logger)
        : this()
    {
        _viewModel = viewModel;
        _logger = logger;

        DataContext = _viewModel;

        AttachedToVisualTree += OnAttached;
        DetachedFromVisualTree += OnDetached;

        _logger.LogInformation("EnhancedSkillBreakdownView created and DataContext set");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _logger?.LogInformation("EnhancedSkillBreakdownView attached, calling ViewModel.OnViewLoaded()");
        _viewModel?.OnViewLoaded();
    }

    private void OnDetached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _logger?.LogInformation("EnhancedSkillBreakdownView detached");
        _viewModel?.OnViewUnloaded();
    }
}
