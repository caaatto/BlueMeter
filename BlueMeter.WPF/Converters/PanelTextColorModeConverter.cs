using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts PanelColorMode string ("Light" or "Dark") to appropriate text color.
/// Light mode = Dark text, Dark mode = Light text
/// </summary>
public class PanelTextColorModeConverter : IValueConverter
{
    private static readonly SolidColorBrush LightModeBrush = new(Color.FromRgb(0, 0, 0)); // #000000 - Black text for light mode
    private static readonly SolidColorBrush DarkModeBrush = new(Color.FromRgb(255, 255, 255)); // #FFFFFF - White text for dark mode
    private static readonly SolidColorBrush DefaultBrush = DarkModeBrush;

    static PanelTextColorModeConverter()
    {
        LightModeBrush.Freeze();
        DarkModeBrush.Freeze();
        DefaultBrush.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string mode)
        {
            return mode.Equals("Light", StringComparison.OrdinalIgnoreCase) ? LightModeBrush : DarkModeBrush;
        }

        return DefaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
