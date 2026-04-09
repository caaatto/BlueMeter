using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BlueMeter.Converters;

/// <summary>
/// Converts ShowOverlay boolean to Color
/// True → White (hide text), False → Black (show text)
/// </summary>
public sealed class ShowOverlayToColorConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count < 1)
        {
            return Colors.Black;
        }

        var showOverlay = values[0] as bool? ?? false;

        // When overlay is shown, make text white (invisible on white background)
        // When overlay is hidden, make text black (visible)
        return showOverlay ? Colors.White : Colors.Black;
    }
}
