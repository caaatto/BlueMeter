using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using BlueMeter.Services.Checklist;
using BlueMeter.ViewModels.Checklist;
using Microsoft.Extensions.Logging;

namespace BlueMeter.Views.Checklist;

/// <summary>
/// Checklist tool window code-behind.
///
/// Port notes (WPF -> Avalonia):
///   - <c>WindowChrome CaptionHeight=30 + UseAeroCaptionButtons</c> replaced
///     by the chromeless quartet on the XAML Window element + a custom title
///     bar that calls <c>BeginMoveDrag(e)</c> from <c>PointerPressed</c>.
///   - <c>Activated</c>/<c>Deactivated</c> still exist on Avalonia's
///     <see cref="Window"/> with the same shape — the focus-throttle pattern
///     (1s while focused, 5s while in the background) ports verbatim.
///   - <c>OnClosed</c> resets the timer to 1s (default) and unsubscribes the
///     activation events.
/// </summary>
public partial class ChecklistWindow : Window
{
    private readonly ITimerService? _timerService;
    private readonly ILogger<ChecklistWindow>? _logger;

    private ChecklistView? _checklistContent;

    public ChecklistWindow()
    {
        InitializeComponent();
    }

    public ChecklistWindow(
        ChecklistViewModel viewModel,
        ITimerService timerService,
        ILogger<ChecklistWindow> logger) : this()
    {
        _timerService = timerService;
        _logger = logger;

        if (_checklistContent is not null)
        {
            _checklistContent.DataContext = viewModel;
        }
        DataContext = viewModel;

        Activated += OnWindowActivated;
        Deactivated += OnWindowDeactivated;

        logger.LogInformation("ChecklistWindow created");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _checklistContent = this.FindControl<ChecklistView>("ChecklistContent");
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void OnWindowActivated(object? sender, EventArgs e)
    {
        _timerService?.SetUpdateInterval(TimeSpan.FromSeconds(1));
        _logger?.LogDebug("ChecklistWindow activated - timer set to 1s interval");
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        _timerService?.SetUpdateInterval(TimeSpan.FromSeconds(5));
        _logger?.LogDebug("ChecklistWindow deactivated - timer set to 5s interval");
    }

    protected override void OnClosed(EventArgs e)
    {
        Activated -= OnWindowActivated;
        Deactivated -= OnWindowDeactivated;

        _timerService?.SetUpdateInterval(TimeSpan.FromSeconds(1));

        base.OnClosed(e);
    }
}
