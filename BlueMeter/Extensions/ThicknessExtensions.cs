using Avalonia;

namespace BlueMeter.Extensions;

/// <summary>
/// Helpers for combining <see cref="Thickness"/> values. Mirrors the WPF version
/// (which lived under <c>BlueMeter.Core.Extends.System.Windows</c>) but rebuilt
/// against <see cref="Avalonia.Thickness"/>.
/// </summary>
public static class ThicknessExtensions
{
    public static Thickness Add(this Thickness th1, Thickness th2)
    {
        return new Thickness(
            th1.Left + th2.Left,
            th1.Top + th2.Top,
            th1.Right + th2.Right,
            th1.Bottom + th2.Bottom);
    }
}
