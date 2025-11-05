using System.Globalization;
using System.Windows.Data;

namespace StarResonanceDpsAnalysis.WPF.Converters;

public class PercentToWidthConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 3)
            return 0d;

        if (!TryToDouble(values[0], out var value))
            return 0d;

        if (!TryToDouble(values[1], out var maximum) || maximum <= 0)
            return 0d;

        if (!TryToDouble(values[2], out var totalWidth))
            return 0d;

        var ratio = Math.Max(0d, Math.Min(1d, value / maximum));
        return ratio * totalWidth;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private bool TryToDouble(object input, out double result)
    {
        if (input is double d)
        {
            result = d;
            return true;
        }

        if (input is float f)
        {
            result = f;
            return true;
        }

        if (input is int i)
        {
            result = i;
            return true;
        }

        if (input is long l)
        {
            result = l;
            return true;
        }

        if (double.TryParse(System.Convert.ToString(input, CultureInfo.InvariantCulture), NumberStyles.Any,
                CultureInfo.InvariantCulture, out var parsed))
        {
            result = parsed;
            return true;
        }

        result = 0d;
        return false;
    }
}