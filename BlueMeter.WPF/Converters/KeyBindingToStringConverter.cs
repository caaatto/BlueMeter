using System.Globalization;
using System.Windows.Data;
using BlueMeter.Core.Extends.System;
using BlueMeter.WPF.Models;

namespace BlueMeter.WPF.Converters;

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