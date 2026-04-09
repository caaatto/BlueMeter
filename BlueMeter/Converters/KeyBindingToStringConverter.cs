using System.Globalization;
using Avalonia.Data.Converters;
using BlueMeter.Extensions;
using BlueMeter.Models;

namespace BlueMeter.Converters;

public class KeyBindingToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is KeyBinding dd)
        {
            return dd.Key.KeyToString(dd.Modifiers);
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
