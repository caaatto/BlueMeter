using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Skill breakdown popup showing per-skill damage statistics with a summary/details/graph
/// tab switcher that's driven by an external segmented selector.
/// </summary>
public partial class SkillBreakdownView : Window
{
    private TabControl? _mainTabControl;
    private StackPanel? _tabSelectorPanel;

    public SkillBreakdownView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
    }

    public SkillBreakdownView(SkillBreakdownViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _mainTabControl = this.FindControl<TabControl>("MainTabControl");
        _tabSelectorPanel = this.FindControl<StackPanel>("TabControlIndexChanger");

        if (_mainTabControl is not null)
        {
            _mainTabControl.SelectionChanged += (_, _) => SyncSelectorWithTab();
        }

        Opened += (_, _) => SyncSelectorWithTab();
    }

    private void TabSelector_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton tb || _mainTabControl is null) return;
        if (!int.TryParse(tb.Tag?.ToString(), out var index)) return;

        _mainTabControl.SelectedIndex = index;
        SyncSelectorWithTab();
    }

    private void SyncSelectorWithTab()
    {
        if (_tabSelectorPanel is null || _mainTabControl is null) return;

        foreach (var child in _tabSelectorPanel.Children)
        {
            if (child is not ToggleButton t) continue;
            if (!int.TryParse(t.Tag?.ToString(), out var tagIndex)) continue;
            t.IsChecked = tagIndex == _mainTabControl.SelectedIndex;
        }
    }

    private void Footer_ConfirmClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Footer_CancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}
