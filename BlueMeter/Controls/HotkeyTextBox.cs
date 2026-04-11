using Avalonia;
using Avalonia.Controls;

namespace BlueMeter.Controls;

/// <summary>
/// <see cref="TextBox"/> variant that exposes a regular <see cref="ShowOverlay"/>
/// styled property instead of the attached property the
/// <see cref="Behaviors.HotkeyOverlayBehavior"/> originally registered on plain
/// <see cref="TextBox"/>. Runtime bindings that point at attached properties
/// via <c>#Name.(prefix:Type.Prop)</c> lose the xmlns context when the window
/// uses <c>x:CompileBindings="False"</c> — the path parser then fails with
/// "Unable to resolve type prefix:Type from any of the following locations".
/// A plain styled property sidesteps the issue and keeps the overlay wiring
/// declarative in XAML.
/// <para>
/// <see cref="StyleKeyOverride"/> points at <see cref="TextBox"/> so the Ant
/// theme ControlTheme (and every other keyed <c>TextBox</c> style) still
/// applies without a dedicated selector.
/// </para>
/// </summary>
public class HotkeyTextBox : TextBox
{
    public static readonly StyledProperty<bool> ShowOverlayProperty =
        AvaloniaProperty.Register<HotkeyTextBox, bool>(nameof(ShowOverlay));

    public bool ShowOverlay
    {
        get => GetValue(ShowOverlayProperty);
        set => SetValue(ShowOverlayProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(TextBox);
}
