using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

/// <summary>
/// MultiValueConverter that prefers the alternation index (first binding) and falls back to the item's Id (second binding).
/// It returns a string representation (1-based index when alternation index is available).
/// </summary>
public class ItemIndexMultiConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        // values[0] => alternation index (int)
        // values[1] => Id (object)
        if (values is null || values.Count < 2)
            return string.Empty;

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
}
