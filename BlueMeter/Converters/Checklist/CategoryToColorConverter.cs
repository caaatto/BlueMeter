using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BlueMeter.Models.Checklist;

namespace BlueMeter.Converters.Checklist;

/// <summary>
/// Converts TaskCategory to SolidColorBrush
/// </summary>
public class CategoryToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskCategory category)
        {
            var colorCode = category.GetColorCode();
            if (Color.TryParse(colorCode, out var color))
            {
                return new SolidColorBrush(color);
            }
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
