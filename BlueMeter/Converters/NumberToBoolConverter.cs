using System.Globalization;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

/// <summary>
/// Converts a number to a bool. Returns true if the number is greater than 0, false otherwise.
/// In WPF this returned <c>Visibility.Visible</c>/<c>Collapsed</c>; in Avalonia we return a
/// bool and bind directly to <c>IsVisible</c>.
/// </summary>
public sealed class NumberToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return false;

        try
        {
            var number = System.Convert.ToDouble(value, culture);
            return number > 0;
        }
        catch
        {
            return false;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
