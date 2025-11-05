using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Subtracts sequential numeric values. The first valid value seeds the result; subsequent values are subtracted.
/// Non-numeric entries are ignored. Negative results are clamped to zero by default.
/// </summary>
public sealed class MinusConverter : IMultiValueConverter
{
    public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length == 0)
        {
            return DependencyProperty.UnsetValue;
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
            return DependencyProperty.UnsetValue;
        }

        var result = accumulator.Value;

        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            return DependencyProperty.UnsetValue;
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

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        return [Binding.DoNothing, Binding.DoNothing];
    }
}
