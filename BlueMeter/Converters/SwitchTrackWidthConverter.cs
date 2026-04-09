using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

/// <summary>
/// Computes track width as: max(OnLabelWidth, OffLabelWidth) + 1.5 * TrackHeight.
/// Expects values: [0] = OnLabel.ActualWidth, [1] = OffLabel.ActualWidth, [2] = Track.ActualHeight
/// </summary>
public sealed class SwitchTrackWidthConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count < 3)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (!ConverterNumberHelper.TryToDouble(values[0], out var onWidth)) onWidth = 0d;
        if (!ConverterNumberHelper.TryToDouble(values[1], out var offWidth)) offWidth = 0d;
        if (!ConverterNumberHelper.TryToDouble(values[2], out var height)) height = 0d;

        var label = Math.Max(onWidth, offWidth);
        var width = label + (height * 1.5d);

        if (double.IsNaN(width) || double.IsInfinity(width) || width < 0)
        {
            return AvaloniaProperty.UnsetValue;
        }

        return width;
    }
}
