using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BlueMeter.Converters;

public sealed class BrushWithOpacityConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0 || values[0] is not IBrush baseBrush)
        {
            return Brushes.Transparent;
        }

        var opacity = 1d;
        if (values.Count > 1 && values[1] is double d)
        {
            opacity = Math.Clamp(d, 0d, 1d);
        }

        // Avalonia brushes are mutable but generally not shared the same way WPF
        // freezable brushes are. We make a clone via a new SolidColorBrush when
        // possible so we don't mutate the source brush; for non-solid brushes we
        // wrap with the requested opacity by mutating a copy where supported.
        if (baseBrush is ISolidColorBrush solid)
        {
            return new SolidColorBrush(solid.Color, opacity);
        }

        // Fallback: create a SolidColorBrush from the brush's transparent black
        // (we can't deep-clone arbitrary brushes here without reflection).
        if (baseBrush is Brush mutable)
        {
            return new SolidColorBrush
            {
                Color = Colors.Transparent,
                Opacity = opacity
            };
        }

        return baseBrush;
    }
}
