using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace BlueMeter.Behaviors;

/// <summary>
/// Shows a "Press key combination..." placeholder while the TextBox is focused.
///
/// Port notes: WPF's version called
/// <c>GetBindingExpression(TextBox.TextProperty).UpdateTarget()</c> on
/// LostFocus to pull the source value back into the target. Avalonia has no
/// equivalent API, so we cache the pre-focus text in <see cref="_savedText"/>
/// and restore it if the user leaves the field without pressing a key. The
/// tunneling key-down handler still clears the placeholder ahead of
/// <see cref="KeyDownCommandBehavior"/> so the real hotkey capture path runs
/// on an empty TextBox.
/// </summary>
public class HotkeyPlaceholderBehavior : Behavior<TextBox>
{
    private const string PlaceholderText = "Press key combination...";
    private bool _isShowingPlaceholder;
    private string? _savedText;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.GotFocus += OnGotFocus;
        AssociatedObject.LostFocus += OnLostFocus;
        AssociatedObject.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
        {
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
            AssociatedObject.RemoveHandler(InputElement.KeyDownEvent, (EventHandler<KeyEventArgs>)OnPreviewKeyDown);
        }

        base.OnDetaching();
    }

    private void OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        _savedText = AssociatedObject.Text;
        _isShowingPlaceholder = true;
        AssociatedObject.Text = PlaceholderText;
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_isShowingPlaceholder || AssociatedObject is null)
        {
            return;
        }

        // Clear placeholder before KeyDownCommandBehavior handles the event.
        _isShowingPlaceholder = false;
        AssociatedObject.Text = string.Empty;
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        if (_isShowingPlaceholder && _savedText is not null)
        {
            AssociatedObject.Text = _savedText;
        }

        _isShowingPlaceholder = false;
        _savedText = null;
    }
}
