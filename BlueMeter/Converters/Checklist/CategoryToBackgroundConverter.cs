using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BlueMeter.Models.Checklist;

namespace BlueMeter.Converters.Checklist;

/// <summary>
/// Converts TaskCategory to a semi-transparent background brush
/// </summary>
public class CategoryToBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TaskCategory category)
        {
            var colorCode = category.GetColorCode();
            if (Color.TryParse(colorCode, out var color))
            {
                // Make color transparent (20% opacity)
                var transparent = Color.FromArgb(51, color.R, color.G, color.B);
                return new SolidColorBrush(transparent);
            }
        }

        return new SolidColorBrush(Color.FromArgb(51, 158, 158, 158));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
