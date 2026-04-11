using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
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
///   - <c>System.Windows.Controls.TextBox</c> → <c>Avalonia.Controls.TextBox</c>.
///   - <c>DependencyProperty.Register</c> → <c>AvaloniaProperty.Register&lt;TOwner, T&gt;</c>.
///   - WPF's tunneling <c>PreviewKeyDown</c> becomes
///     <c>AddHandler(InputElement.KeyDownEvent, …, RoutingStrategies.Tunnel)</c>
///     so we still see the key before <see cref="KeyDownCommandBehavior"/> on
///     the same TextBox consumes it.
///   - <c>GotFocus</c> uses Avalonia's <c>GotFocusEventArgs</c>; <c>LostFocus</c>
///     stays on plain <c>RoutedEventArgs</c>.
///   - <see cref="ShowOverlayProperty"/> is an **attached** property hosted on
///     the AssociatedObject (the TextBox) rather than on the behavior itself.
///     Avalonia XAML cannot assign <c>x:Name</c> to a <see cref="Behavior{T}"/>,
///     so bindings from sibling controls (e.g. the overlay TextBlock) reference
///     the TextBox by name and use the attached-property path
///     <c>(behaviors:HotkeyOverlayBehavior.ShowOverlay)</c>.
/// </summary>
public class HotkeyOverlayBehavior : Behavior<TextBox>
{
    private bool _stoppedHotkeysForEditing;

    public static readonly AttachedProperty<bool> ShowOverlayProperty =
        AvaloniaProperty.RegisterAttached<HotkeyOverlayBehavior, TextBox, bool>("ShowOverlay");

    public static bool GetShowOverlay(TextBox element) => element.GetValue(ShowOverlayProperty);

    public static void SetShowOverlay(TextBox element, bool value) => element.SetValue(ShowOverlayProperty, value);

    private void SetOverlay(bool value)
    {
        if (AssociatedObject is not null)
        {
            SetShowOverlay(AssociatedObject, value);
        }
    }

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
        SetOverlay(true);

        // Temporarily stop global hotkeys when the user clicks into a hotkey field
        // so they can set a currently-registered hotkey (e.g., Ctrl+F6) without it
        // firing the existing binding.
        if (AssociatedObject?.DataContext is SettingsViewModel viewModel
            && viewModel.AppConfig.GlobalHotkeysEnabled)
        {
            viewModel.GlobalHotkeyService.Stop();
            _stoppedHotkeysForEditing = true;
        }
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        // Hide the overlay as soon as the user starts typing.
        SetOverlay(false);
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        SetOverlay(false);

        // Restart global hotkeys when leaving the field — but only if the toggle
        // is still enabled (the user may have flipped it off mid-edit).
        if (_stoppedHotkeysForEditing)
        {
            if (AssociatedObject?.DataContext is SettingsViewModel viewModel
                && viewModel.AppConfig.GlobalHotkeysEnabled)
            {
                viewModel.GlobalHotkeyService.Start();
            }

            _stoppedHotkeysForEditing = false;
        }
    }
}
