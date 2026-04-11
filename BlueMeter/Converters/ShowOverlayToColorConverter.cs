using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BlueMeter.Converters;

/// <summary>
/// Maps the overlay-visible flag on a hotkey TextBox to the foreground brush.
/// While the overlay ("Enter Hot Key") is showing on top of the text the
/// underlying text should stay invisible so it isn't peeking through. WPF
/// bound <c>TextBox.Foreground.Color</c> via a MultiBinding targeting the
/// inner <see cref="SolidColorBrush.Color"/>; Avalonia's <c>TextBox.Foreground</c>
/// is an <see cref="IBrush"/> so the converter returns a brush directly and
/// the consumer binds the property in one step.
/// </summary>
public sealed class ShowOverlayToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var showOverlay = value as bool? ?? false;
        return showOverlay ? Brushes.White : Brushes.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
