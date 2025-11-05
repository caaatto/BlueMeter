using System.Globalization;
using System.Windows.Data;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// MultiValueConverter that prefers the alternation index (first binding) and falls back to the item's Id (second binding).
/// It returns a string representation (1-based index when alternation index is available).
/// </summary>
public class ItemIndexMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] => alternation index (int)
        // values[1] => Id (object)
        if (values == null || values.Length < 2)
            return "";

        try
        {
            if (values[0] is int alt && alt >= 0)
            {
                // display as 1-based
                return (alt + 1).ToString(culture);
            }

            // fallback to Id
            var id = values[1];
            return id?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}