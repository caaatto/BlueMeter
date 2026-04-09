using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

/// <summary>
/// Subtracts sequential numeric values. The first valid value seeds the result; subsequent values are subtracted.
/// Non-numeric entries are ignored. Negative results are clamped to zero by default.
/// </summary>
public sealed class MinusConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Count == 0)
        {
            return AvaloniaProperty.UnsetValue;
        }

        double? accumulator = null;

        foreach (var value in values)
        {
            if (!ConverterNumberHelper.TryToDouble(value, out var number))
            {
                continue;
            }

            if (accumulator is null)
            {
                accumulator = number;
            }
            else
            {
                accumulator -= number;
            }
        }

        if (accumulator is null)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var result = accumulator.Value;

        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (result < 0)
        {
            result = 0;
        }

        if (targetType == typeof(int) || targetType == typeof(int?))
        {
            return (int)Math.Round(result);
        }

        if (targetType == typeof(float) || targetType == typeof(float?))
        {
            return (float)result;
        }

        if (targetType == typeof(decimal) || targetType == typeof(decimal?))
        {
            return (decimal)result;
        }

        if (targetType == typeof(string))
        {
            return result.ToString(culture);
        }

        return result;
    }
}
