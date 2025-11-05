using System.Globalization;
using System.Windows.Data;
using BlueMeter.WPF.Models;

namespace BlueMeter.WPF.Converters;

public class EnumToLocalizedStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return enumValue.GetLocalizedDescription();
            // return EnumLocalizer.GetLocalizedDescription(enumValue);
        }

        return value?.ToString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}