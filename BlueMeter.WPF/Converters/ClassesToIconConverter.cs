using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using BlueMeter.Core.Models;

namespace BlueMeter.WPF.Converters;

internal class ClassesToIconConverter : IValueConverter
{
    private readonly Dictionary<Classes, ImageSource?> _iconCache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Classes classes) return null;

        if (_iconCache.TryGetValue(classes, out var cached) && cached is not null)
            return cached;

        var app = Application.Current;
        var keysToTry = new[] { value, $"Classes{value}Icon" };

        foreach (var key in keysToTry)
        {
            var res = app?.TryFindResource(key);
            if (res is not ImageSource img) continue;
            _iconCache[classes] = img;
            return img;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ClassesToIconConverter does not support ConvertBack.");
    }
}