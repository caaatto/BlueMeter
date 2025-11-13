using System.Windows;
using BlueMeter.WPF.ViewModels.Checklist;
using BlueMeter.WPF.Services.Checklist;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.Views.Checklist;

/// <summary>
/// Interaction logic for ChecklistWindow.xaml
/// </summary>
public partial class ChecklistWindow : Window
{
    private readonly ITimerService? _timerService;
    private readonly ILogger<ChecklistWindow>? _logger;

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

        // Set DataContext für das UserControl
        ChecklistContent.DataContext = viewModel;
        DataContext = viewModel;

        // Window Focus Detection für Timer-Anpassung
        Activated += OnWindowActivated;
        Deactivated += OnWindowDeactivated;

        logger.LogInformation("ChecklistWindow created");
    }

    private void OnWindowActivated(object? sender, EventArgs e)
    {
        // Fenster ist fokussiert -> Timer auf 1 Sekunde
        _timerService?.SetUpdateInterval(TimeSpan.FromSeconds(1));
        _logger?.LogDebug("ChecklistWindow activated - timer set to 1s interval");
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        // Fenster ist nicht fokussiert -> Timer auf 5 Sekunden
        _timerService?.SetUpdateInterval(TimeSpan.FromSeconds(5));
        _logger?.LogDebug("ChecklistWindow deactivated - timer set to 5s interval");
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Cleanup
        Activated -= OnWindowActivated;
        Deactivated -= OnWindowDeactivated;

        // Reset Timer auf Standard
        _timerService?.SetUpdateInterval(TimeSpan.FromSeconds(1));
    }
}
