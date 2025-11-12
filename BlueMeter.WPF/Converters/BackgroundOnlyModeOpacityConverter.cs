using System;
using System.Globalization;
using System.Windows.Data;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts BackgroundOnlyMode boolean to opacity value.
/// True = 0.3 (semi-transparent), False = 1.0 (fully opaque)
/// </summary>
public class BackgroundOnlyModeOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool backgroundOnlyMode)
        {
            return backgroundOnlyMode ? 0.3 : 1.0;
        }
        return 1.0; // Default fully opaque
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
