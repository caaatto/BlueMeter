using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using StarResonanceDpsAnalysis.Core.Models;

namespace StarResonanceDpsAnalysis.WPF.Converters;

internal sealed class ClassesColorConverter : IValueConverter
{
    private readonly Dictionary<Classes, Brush?> _brushCache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Classes classes) return null;

        if (_brushCache.TryGetValue(classes, out var cached) && cached is not null)
            return cached;

        var app = Application.Current;
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
            var resource = app?.TryFindResource(key!);
            if (resource is Brush brush)
            {
                _brushCache[classes] = brush;
                return brush;
            }

            if (resource is Color color)
            {
                var solidBrush = new SolidColorBrush(color);
                if (solidBrush.CanFreeze)
                {
                    solidBrush.Freeze();
                }

                _brushCache[classes] = solidBrush;
                return solidBrush;
            }
        }

        if (app?.TryFindResource("ClassesUnknownBrush") is Brush fallback)
        {
            _brushCache[classes] = fallback;
            return fallback;
        }

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ClassesColorConverter does not support ConvertBack.");
    }
}
