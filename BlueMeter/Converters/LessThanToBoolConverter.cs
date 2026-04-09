using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

public class LessThanToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d) return DoubleConverter(d, parameter);
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // This converter is designed for one-way bindings (e.g. style classes) and
        // cannot reliably infer the original numeric value. Returning DoNothing
        // prevents the binding system from trying to propagate changes back to
        // the source when a two-way binding is accidentally used.
        return BindingOperations.DoNothing;
    }

    private static bool DoubleConverter(double value, object? parameter)
    {
        return double.TryParse(parameter?.ToString(), out var th) && value < th;
    }
}
