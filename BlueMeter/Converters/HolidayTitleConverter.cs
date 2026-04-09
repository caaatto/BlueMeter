using System.Globalization;
using Avalonia.Data.Converters;
using BlueMeter.Config;

namespace BlueMeter.Converters;

/// <summary>
/// Combines app name with holiday name when a holiday is active
/// Takes AppConfig as input and returns the formatted window title
/// E.g., "BlueMeter" + "Christmas" → "BlueMeter - Christmas"
/// </summary>
public class HolidayTitleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AppConfig config)
            return "BlueMeter";

        // Get base app name from theme color
        string baseName = ThemeDefinitions.GetAppName(config.EffectiveThemeColor);

        // If there's a holiday name, append it
        string? holidayName = config.CurrentHolidayName;
        if (!string.IsNullOrEmpty(holidayName))
        {
            return $"{baseName} - {holidayName}";
        }

        return baseName;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
