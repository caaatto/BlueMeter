using System.Globalization;
using System.Windows.Data;
using BlueMeter.WPF.Models;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Multi value converter that computes DPS (damage per second) and formats it with the same
/// rules as <see cref="HumanReadableNumberConverter"/>.
/// Expected bindings:
/// values[0] = total damage (numeric)
/// values[1] = duration (TimeSpan or numeric seconds)
/// values[2] (optional) = mode (NumberDisplayMode or string "KMB"/"Wan")
/// </summary>
public sealed class DpsConverter : IMultiValueConverter
{
    public object Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        const string notAvailable = "N/A";

        if (values == null || values.Length < 2)
        {
            return notAvailable;
        }

        if (!ConverterNumberHelper.TryToDouble(values[0], out var total))
        {
            return notAvailable;
        }

        double seconds;
        if (values[1] is TimeSpan timeSpan)
        {
            seconds = timeSpan.TotalSeconds;
        }
        else if (!ConverterNumberHelper.TryToDouble(values[1], out seconds))
        {
            return notAvailable;
        }

        if (seconds <= 0.0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
        {
            return notAvailable;
        }

        var mode = NumberDisplayMode.KMB;
        if (values.Length > 2 && values[2] != null)
        {
            mode = ConverterNumberHelper.ParseDisplayMode(values[2], mode);
        }
        else if (parameter != null)
        {
            mode = ConverterNumberHelper.ParseDisplayMode(parameter, mode);
        }

        var dps = total / seconds;
        var formatted = ConverterNumberHelper.FormatHumanReadable(dps, mode, culture);
        return formatted;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}