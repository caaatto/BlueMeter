using System.Globalization;
using System.Windows.Data;
using BlueMeter.Core.Models;

namespace BlueMeter.WPF.Converters;

public class ClassSpecToStringConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ClassSpec classSpec)
        {
            return classSpec.ToString();
        }

        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}