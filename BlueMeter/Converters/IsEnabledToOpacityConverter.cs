using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

/// <summary>
/// Converts IsEnabled boolean to opacity value.
/// True = 1.0 (fully opaque), False = 0.5 (semi-transparent/grayed out)
/// </summary>
public class IsEnabledToOpacityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isEnabled)
        {
            return isEnabled ? 1.0 : 0.5;
        }
        return 1.0; // Default fully opaque
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
