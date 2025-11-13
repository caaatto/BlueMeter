using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BlueMeter.WPF.Models.Checklist;

namespace BlueMeter.WPF.Converters.Checklist;

/// <summary>
/// Konvertiert TaskCategory zu einem halbtransparenten Background
/// </summary>
public class CategoryToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskCategory category)
        {
            var colorCode = category.GetColorCode();
            var color = (Color)ColorConverter.ConvertFromString(colorCode);

            // Mache Farbe transparent (20% Opacity)
            color.A = 51; // 20% von 255
            return new SolidColorBrush(color);
        }

        return new SolidColorBrush(Color.FromArgb(51, 158, 158, 158));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
