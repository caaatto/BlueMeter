using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BlueMeter.WPF.Models.Checklist;

namespace BlueMeter.WPF.Converters.Checklist;

/// <summary>
/// Konvertiert TaskCategory zu SolidColorBrush
/// </summary>
public class CategoryToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TaskCategory category)
        {
            var colorCode = category.GetColorCode();
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode));
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
