using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StarResonanceDpsAnalysis.WPF.Converters;

public sealed class DoubleNegateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ConverterNumberHelper.TryToDouble(value, out var number)
            ? -number
            : DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

public sealed class DoubleSumConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        double total = 0;
        var hasValue = false;

        foreach (var value in values)
        {
            if (!ConverterNumberHelper.TryToDouble(value, out var number))
            {
                continue;
            }

            total += number;
            hasValue = true;
        }

        return hasValue ? total : DependencyProperty.UnsetValue;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        var results = new object[targetTypes.Length];
        for (var i = 0; i < results.Length; i++)
        {
            results[i] = Binding.DoNothing;
        }

        return results;
    }
}
