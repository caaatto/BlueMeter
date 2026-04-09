using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

public sealed class DoubleNegateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ConverterNumberHelper.TryToDouble(value, out var number)
            ? -number
            : AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}

public sealed class DoubleSumConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
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

        return hasValue ? total : AvaloniaProperty.UnsetValue;
    }
}
