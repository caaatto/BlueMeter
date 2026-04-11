using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using BlueMeter.Models.Checklist;
using BlueMeter.ViewModels.Checklist;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Views.Checklist;

/// <summary>
/// Checklist UserControl code-behind.
///
/// Port notes (WPF -> Avalonia):
///   - <c>Loaded</c>/<c>Unloaded</c> become <c>AttachedToVisualTree</c>/
///     <c>DetachedFromVisualTree</c>.
///   - <c>MouseLeftButtonDown</c> on the task-item Border becomes
///     <c>PointerPressed</c>. The "ignore clicks on inner CheckBox/Button"
///     guard walks the visual tree from <c>e.Source</c> upward — Avalonia's
///     <c>RoutedEventArgs.Source</c> is typed as <c>object?</c>, so we cast
///     to <see cref="Visual"/> and use <c>GetVisualAncestors()</c>.
///   - The five WPF Storyboards declared in ChecklistStyles.xaml
///     (FadeIn/FadeOut/SlideInFromTop/TaskCompleted/ProgressIncrement) were
///     dropped. They are not reimplemented here because the WPF code-behind
///     never started any of them either — they were dead resources.
/// </summary>
public partial class ChecklistView : UserControl
{
    private readonly ChecklistViewModel? _viewModel;
    private readonly ILogger<ChecklistView>? _logger;

    public ChecklistView()
    {
        InitializeComponent();
    }

    public ChecklistView(ChecklistViewModel viewModel, ILogger<ChecklistView> logger) : this()
    {
        _viewModel = viewModel;
        _logger = logger;
        DataContext = _viewModel;

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        try
        {
            await _viewModel.InitializeAsync();
            _logger?.LogInformation("ChecklistView loaded and initialized");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error initializing ChecklistView");
        }
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _viewModel?.Dispose();
        _logger?.LogInformation("ChecklistView unloaded");
    }

    private void TaskItem_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { DataContext: ChecklistTask task } || _viewModel is null)
        {
            return;
        }

        // Only toggle when the user clicked the row background, not the inner
        // CheckBox/Button (which have their own handlers).
        if (e.Source is Visual visual)
        {
            foreach (var ancestor in GetSelfAndAncestors(visual))
            {
                if (ancestor == sender)
                {
                    break;
                }

                if (ancestor is CheckBox or Button)
                {
                    return;
                }
            }
        }

        _viewModel.ToggleTaskCommand.Execute(task);
        e.Handled = true;
    }

    private static System.Collections.Generic.IEnumerable<Visual> GetSelfAndAncestors(Visual visual)
    {
        yield return visual;
        foreach (var ancestor in visual.GetVisualAncestors())
        {
            yield return ancestor;
        }
    }
}
