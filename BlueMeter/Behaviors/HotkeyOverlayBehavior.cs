using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using BlueMeter.Controls;
using BlueMeter.ViewModels;

namespace BlueMeter.Behaviors;

/// <summary>
/// Controls overlay visibility for hotkey-capture TextBoxes. Shows the overlay
/// while the TextBox is focused and hides it on the first key press, mirroring
/// the WPF behavior. While focused, the global hotkey service is temporarily
/// stopped so the user can rebind a currently-registered combination
/// (e.g. Ctrl+F6) without it firing the existing hotkey first.
///
/// Port notes:
///   - <c>System.Windows.Controls.TextBox</c> → <see cref="HotkeyTextBox"/>
///     (a trivial Avalonia <c>TextBox</c> subclass that exposes a regular
///     <see cref="HotkeyTextBox.ShowOverlay"/> styled property). WPF bound to
///     an attached property here, but Avalonia's runtime binding parser can't
///     resolve the xmlns prefix in a <c>#Name.(prefix:Type.Prop)</c> path when
///     the enclosing window opts out of compiled bindings, so we keep the
///     flag on a regular dependency property instead.
///   - WPF's tunneling <c>PreviewKeyDown</c> becomes
///     <c>AddHandler(InputElement.KeyDownEvent, …, RoutingStrategies.Tunnel)</c>
///     so we still see the key before <see cref="KeyDownCommandBehavior"/> on
///     the same TextBox consumes it.
///   - <c>GotFocus</c> uses Avalonia's <c>GotFocusEventArgs</c>; <c>LostFocus</c>
///     stays on plain <c>RoutedEventArgs</c>.
/// </summary>
public class HotkeyOverlayBehavior : Behavior<HotkeyTextBox>
{
    private bool _stoppedHotkeysForEditing;

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

        AssociatedObject.ShowOverlay = true;

        // Temporarily stop global hotkeys when the user clicks into a hotkey field
        // so they can set a currently-registered hotkey (e.g., Ctrl+F6) without it
        // firing the existing binding.
        if (AssociatedObject.DataContext is SettingsViewModel viewModel
            && viewModel.AppConfig.GlobalHotkeysEnabled)
        {
            viewModel.GlobalHotkeyService.Stop();
            _stoppedHotkeysForEditing = true;
        }
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        // Hide the overlay as soon as the user starts typing.
        if (AssociatedObject is not null)
        {
            AssociatedObject.ShowOverlay = false;
        }
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject is null)
        {
            return;
        }

        AssociatedObject.ShowOverlay = false;

        // Restart global hotkeys when leaving the field — but only if the toggle
        // is still enabled (the user may have flipped it off mid-edit).
        if (_stoppedHotkeysForEditing)
        {
            if (AssociatedObject.DataContext is SettingsViewModel viewModel
                && viewModel.AppConfig.GlobalHotkeysEnabled)
            {
                viewModel.GlobalHotkeyService.Start();
            }

            _stoppedHotkeysForEditing = false;
        }
    }
}
