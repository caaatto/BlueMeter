using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

public sealed class EqualsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count < 2) return false;
        return Equals(values[0], values[1]);
    }
}
