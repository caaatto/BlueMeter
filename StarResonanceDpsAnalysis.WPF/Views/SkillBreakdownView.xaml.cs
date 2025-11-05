using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using StarResonanceDpsAnalysis.WPF.ViewModels;

namespace StarResonanceDpsAnalysis.WPF.Views;

/// <summary>
/// SkillBreakdownView.xaml 的交互逻辑
/// </summary>
public partial class SkillBreakdownView : Window
{
    public SkillBreakdownView(SkillBreakdownViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        // Ensure selector reflects initial SelectedIndex
        Loaded += (_, _) => SyncSelectorWithTab();
        MainTabControl.SelectionChanged += (_, _) => SyncSelectorWithTab();
    }

    private void TabSelector_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton tb || !int.TryParse(tb.Tag?.ToString(), out var index)) return;
        MainTabControl.SelectedIndex = index;
        SyncSelectorWithTab();
    }

    private void SyncSelectorWithTab()
    {
        if (TabControlIndexChanger == null) return;

        foreach (var child in LogicalTreeHelper.GetChildren(TabControlIndexChanger))
        {
            if (child is not ToggleButton t || !int.TryParse(t.Tag?.ToString(), out var tagIndex)) continue;
            t.IsChecked = tagIndex == MainTabControl.SelectedIndex;
        }
    }

    private void Footer_ConfirmClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Footer_CancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}