using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace BlueMeter.Views;

/// <summary>
/// Reference window listing example damage values per class — used as a sanity
/// check for new players.
///
/// Port notes:
/// - The matching <c>TabControl</c> was already commented out in the WPF source;
///   the toggle-button row is decorative and does not yet drive a content area.
///   The class-selection sync logic from WPF is preserved as a no-op.
/// </summary>
public partial class DamageReferenceView : Window
{
    public DamageReferenceView()
    {
        InitializeComponent();
        Loaded += (_, _) => SyncSelectorWithTab();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TabSelector_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton tb || !int.TryParse(tb.Tag?.ToString(), out _))
        {
            return;
        }

        SyncSelectorWithTab();
    }

    private void SyncSelectorWithTab()
    {
        var changer = this.FindControl<StackPanel>("TabControlIndexChanger");
        if (changer is null)
        {
            return;
        }

        foreach (var child in changer.Children)
        {
            if (child is not ToggleButton t || !int.TryParse(t.Tag?.ToString(), out _))
            {
                continue;
            }

            // No backing TabControl yet; matches the WPF stub.
        }
    }

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
