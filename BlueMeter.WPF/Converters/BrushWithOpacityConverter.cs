using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

public sealed class BrushWithOpacityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0 || values[0] is not Brush baseBrush)
        {
            return Brushes.Transparent;
        }

        var opacity = 1d;
        if (values.Length > 1 && values[1] is double d)
        {
            opacity = Math.Clamp(d, 0d, 1d);
        }

        var brush = baseBrush.CloneCurrentValue();
        brush.Opacity = opacity;
        return brush;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
