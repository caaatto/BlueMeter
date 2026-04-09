using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BlueMeter.Core.Models;

namespace BlueMeter.Converters;

internal sealed class ClassesColorConverter : IValueConverter
{
    private readonly Dictionary<Classes, IBrush?> _brushCache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Classes classes) return null;

        if (_brushCache.TryGetValue(classes, out var cached) && cached is not null)
            return cached;

        var app = Application.Current;
        var themeVariant = app?.ActualThemeVariant;

        var keysToTry = new object?[]
        {
            value,
            $"Classes{value}Brush",
            $"{value}Brush",
            $"Classes{value}Color",
            $"{value}Color"
        };

        foreach (var key in keysToTry)
        {
            if (key is null || app is null) continue;
            if (!app.TryGetResource(key, themeVariant, out var resource)) continue;

            if (resource is IBrush brush)
            {
                _brushCache[classes] = brush;
                return brush;
            }

            if (resource is Color color)
            {
                var solidBrush = new SolidColorBrush(color);
                _brushCache[classes] = solidBrush;
                return solidBrush;
            }
        }

        if (app is not null && app.TryGetResource("ClassesUnknownBrush", themeVariant, out var fallback) && fallback is IBrush fallbackBrush)
        {
            _brushCache[classes] = fallbackBrush;
            return fallbackBrush;
        }

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ClassesColorConverter does not support ConvertBack.");
    }
}
