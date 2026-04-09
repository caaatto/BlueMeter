using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

/// <summary>
/// Divides the first bound value by the second and adapts the result to the target type.
/// </summary>
public sealed class DivisionMultiValueConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count < 2)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (!ConverterNumberHelper.TryToDouble(values[0], out var numerator) ||
            !ConverterNumberHelper.TryToDouble(values[1], out var denominator))
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (double.IsNaN(numerator) || double.IsNaN(denominator) || Math.Abs(denominator) < double.Epsilon)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var result = numerator / denominator;

        if (parameter is string fmt && !string.IsNullOrWhiteSpace(fmt))
        {
            return result.ToString(fmt, culture);
        }

        if (targetType == typeof(string))
        {
            return result.ToString(culture);
        }

        if (targetType == typeof(int))
        {
            return (int)Math.Round(result);
        }

        if (targetType == typeof(float))
        {
            return (float)result;
        }

        if (targetType == typeof(decimal))
        {
            return (decimal)result;
        }

        if (targetType == typeof(double) || targetType == typeof(object))
        {
            return result;
        }

        return System.Convert.ChangeType(result, targetType, culture);
    }
}
