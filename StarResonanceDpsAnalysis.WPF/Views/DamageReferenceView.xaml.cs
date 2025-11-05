using System.Windows;
using System.Windows.Controls.Primitives;

namespace StarResonanceDpsAnalysis.WPF.Views;

/// <summary>
/// DamageReferenceView.xaml 的交互逻辑
/// </summary>
public partial class DamageReferenceView : Window
{
    public DamageReferenceView()
    {
        InitializeComponent();
        // Ensure selector reflects initial SelectedIndex
        Loaded += (_, _) => SyncSelectorWithTab();
        //MainTabControl.SelectionChanged += (_, _) => SyncSelectorWithTab();
    }

    private void TabSelector_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton tb || !int.TryParse(tb.Tag?.ToString(), out var index)) return;
        //MainTabControl.SelectedIndex = index;
        SyncSelectorWithTab();
    }

    private void SyncSelectorWithTab()
    {
        if (TabControlIndexChanger == null) return;

        foreach (var child in LogicalTreeHelper.GetChildren(TabControlIndexChanger))
        {
            if (child is not ToggleButton t || !int.TryParse(t.Tag?.ToString(), out var tagIndex)) continue;
            //t.IsChecked = tagIndex == MainTabControl.SelectedIndex;
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
}