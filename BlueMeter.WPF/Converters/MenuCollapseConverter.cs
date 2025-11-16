using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlueMeter.WPF.Converters;

public class MenuCollapseConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCollapsed)
        {
            // When collapsed, show minimal width (50), when expanded show full width (350)
            return new GridLength(isCollapsed ? 50 : 350);
        }

        return new GridLength(350);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
