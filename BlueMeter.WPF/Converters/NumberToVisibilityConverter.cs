using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts a number to Visibility. Returns Visible if number > 0, Collapsed otherwise.
/// </summary>
public sealed class NumberToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return Visibility.Collapsed;

        try
        {
            var number = System.Convert.ToDouble(value);
            return number > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch
        {
            return Visibility.Collapsed;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
