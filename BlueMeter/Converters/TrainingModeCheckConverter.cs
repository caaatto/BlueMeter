using System.Globalization;
using Avalonia.Data.Converters;
using BlueMeter.Models;

namespace BlueMeter.Converters;

public sealed class TrainingModeCheckConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TrainingMode currentMode || parameter is not string modeString)
            return false;

        if (Enum.TryParse<TrainingMode>(modeString, out var targetMode))
        {
            return currentMode == targetMode;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
