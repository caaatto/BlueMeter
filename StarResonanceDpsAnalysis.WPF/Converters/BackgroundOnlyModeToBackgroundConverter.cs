using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// Converts BackgroundOnlyMode boolean to a Background Brush.
/// When true, returns Transparent. When false, returns White.
/// </summary>
public class BackgroundOnlyModeToBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool backgroundOnlyMode && backgroundOnlyMode)
        {
            return Brushes.Transparent;
        }

        return new SolidColorBrush(Color.FromRgb(255, 255, 255)); // White
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
