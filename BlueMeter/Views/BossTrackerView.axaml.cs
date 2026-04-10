using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BlueMeter.Views;

/// <summary>
/// Static design preview of the world-boss tracker. The WPF original was
/// purely decorative — no view model, no live data — and this port keeps the
/// same shape with hard-coded sample rows.
///
/// Port notes:
/// - WPF used <c>DrawingBrush</c> wrapping <c>GeometryDrawing</c> +
///   <c>ImageBrush</c> as a verbose way to fill the boss icons. Avalonia's
///   <c>ImageBrush</c> takes a <c>Source</c> directly, so the boilerplate is
///   collapsed into a single keyed <c>ImageBrush</c> resource.
/// - The per-state styles (dead/alive/inFight/na) became Avalonia class
///   selectors instead of WPF keyed BasedOn styles.
/// </summary>
public partial class BossTrackerView : Window
{
    public BossTrackerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
