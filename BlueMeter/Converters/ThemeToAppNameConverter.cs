using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using BlueMeter.Config;

namespace BlueMeter.Converters;

/// <summary>
/// Converts theme color to appropriate app display name.
/// E.g., blue theme → "BlueMeter", pink theme → "PinkMeter", etc.
/// </summary>
public class ThemeToAppNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string colorOrId)
        {
            return ThemeDefinitions.GetAppName(colorOrId);
        }

        return "BlueMeter"; // Default
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
