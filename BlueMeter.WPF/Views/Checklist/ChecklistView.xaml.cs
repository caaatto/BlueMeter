using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlueMeter.WPF.Models.Checklist;
using BlueMeter.WPF.ViewModels.Checklist;
using Microsoft.Extensions.Logging;

namespace BlueMeter.WPF.Views.Checklist;

/// <summary>
/// Interaction logic for ChecklistView.xaml
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

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
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
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewModel?.Dispose();
        _logger?.LogInformation("ChecklistView unloaded");
    }

    /// <summary>
    /// Handler für Klick auf Task-Item (Toggle Completion)
    /// </summary>
    private void TaskItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ChecklistTask task } && _viewModel != null)
        {
            // Nur togglen wenn nicht auf Checkbox oder Button geklickt wurde
            if (e.OriginalSource is not CheckBox && e.OriginalSource is not Button)
            {
                _viewModel.ToggleTaskCommand.Execute(task);
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Keyboard Navigation Support - jetzt implementiert via KeyboardNavigationBehavior!
    /// Zusätzliche Shortcuts:
    /// - Ctrl+F: Focus Search (wird vom Behavior gehandelt)
    /// - Enter/Space: Toggle Task (wird vom Behavior gehandelt)
    /// - Arrow Up/Down: Navigate (wird vom Behavior gehandelt)
    /// </summary>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        // Zusätzliche globale Shortcuts können hier hinzugefügt werden
        // z.B. Ctrl+E für Export, Ctrl+I für Import, etc.
    }
}
