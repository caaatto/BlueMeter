using System.Globalization;
using System.Windows.Data;
using StarResonanceDpsAnalysis.WPF.Models;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// Converts numeric values to compact human-readable strings.
/// </summary>
public class HumanReadableNumberConverter : IValueConverter, IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length == 0)
        {
            return string.Empty;
        }

        var mode = NumberDisplayMode.KMB;
        if (values.Length > 1 && values[1] != null)
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

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var mode = ConverterNumberHelper.ParseDisplayMode(parameter);
        if (!ConverterNumberHelper.TryToDouble(value, out var number))
        {
            return string.Empty;
        }

        return ConverterNumberHelper.FormatHumanReadable(number, mode, culture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}