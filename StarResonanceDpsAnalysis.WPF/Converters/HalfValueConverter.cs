using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StarResonanceDpsAnalysis.WPF.Converters;

public class HalfValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConverterNumberHelper.TryToDouble(value, out var original))
        {
            original = 0d;
        }

        var half = original / 2d;

        if (targetType == typeof(double) || targetType == typeof(double?))
        {
            return half;
        }

        if (targetType == typeof(float) || targetType == typeof(float?))
        {
            return (float)half;
        }

        if (targetType == typeof(decimal) || targetType == typeof(decimal?))
        {
            return (decimal)half;
        }

        if (targetType == typeof(string))
        {
            return half.ToString(culture);
        }

        // Default to CornerRadius for compatibility with existing usages.
        return new CornerRadius(half);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
