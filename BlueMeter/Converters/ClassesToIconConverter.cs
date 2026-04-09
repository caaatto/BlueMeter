using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BlueMeter.Core.Models;

namespace BlueMeter.Converters;

internal class ClassesToIconConverter : IValueConverter
{
    private readonly Dictionary<Classes, IImage?> _iconCache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Classes classes) return null;

        if (_iconCache.TryGetValue(classes, out var cached) && cached is not null)
            return cached;

        var app = Application.Current;
        var themeVariant = app?.ActualThemeVariant;
        var keysToTry = new object?[] { value, $"Classes{value}Icon" };

        foreach (var key in keysToTry)
        {
            if (key is null || app is null) continue;
            if (!app.TryGetResource(key, themeVariant, out var res)) continue;
            if (res is not IImage img) continue;
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
