using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using BlueMeter.Models;

namespace BlueMeter.Converters;

/// <summary>
/// Converts numeric values to compact human-readable strings.
/// </summary>
public class HumanReadableNumberConverter : IValueConverter, IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0)
        {
            return string.Empty;
        }

        var mode = NumberDisplayMode.KMB;
        if (values.Count > 1 && values[1] != null)
        {
            mode = ConverterNumberHelper.ParseDisplayMode(values[1], mode);
        }
        else if (parameter != null)
        {
            mode = ConverterNumberHelper.ParseDisplayMode(parameter, mode);
        }

        if (!ConverterNumberHelper.TryToDouble(values[0], out var number))
        {
            return string.Empty;
        }

        return ConverterNumberHelper.FormatHumanReadable(number, mode, culture);
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var mode = ConverterNumberHelper.ParseDisplayMode(parameter);
        if (!ConverterNumberHelper.TryToDouble(value, out var number))
        {
            return string.Empty;
        }

        return ConverterNumberHelper.FormatHumanReadable(number, mode, culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
