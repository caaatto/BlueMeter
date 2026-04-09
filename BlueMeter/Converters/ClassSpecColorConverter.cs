using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using BlueMeter.Core.Models;

namespace BlueMeter.Converters;

internal sealed class ClassSpecColorConverter : IValueConverter
{
    private readonly Dictionary<ClassSpec, IBrush?> _brushCache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ClassSpec classSpec) return null;

        if (_brushCache.TryGetValue(classSpec, out var cached) && cached is not null)
            return cached;

        var app = Application.Current;
        var themeVariant = app?.ActualThemeVariant;

        // Try to find resource for specific ClassSpec
        var keysToTry = new object?[]
        {
            $"ClassSpec{classSpec}Brush",
            $"ClassSpec{classSpec}Color",
            // Fallback to base class colors if spec-specific not found
            GetBaseClassFromSpec(classSpec).ToString() + "Brush",
            $"Classes{GetBaseClassFromSpec(classSpec)}Brush",
            $"Classes{GetBaseClassFromSpec(classSpec)}Color",
        };

        foreach (var key in keysToTry)
        {
            if (key is null || app is null) continue;
            if (!app.TryGetResource(key, themeVariant, out var resource)) continue;

            if (resource is IBrush brush)
            {
                _brushCache[classSpec] = brush;
                return brush;
            }

            if (resource is Color color)
            {
                var solidBrush = new SolidColorBrush(color);
                _brushCache[classSpec] = solidBrush;
                return solidBrush;
            }
        }

        // Fallback to unknown color
        if (app is not null && app.TryGetResource("ClassesUnknownBrush", themeVariant, out var fallback) && fallback is IBrush fallbackBrush)
        {
            _brushCache[classSpec] = fallbackBrush;
            return fallbackBrush;
        }

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("ClassSpecColorConverter does not support ConvertBack.");
    }

    /// <summary>
    /// Extract base class from ClassSpec
    /// </summary>
    private static Classes GetBaseClassFromSpec(ClassSpec spec) => spec switch
    {
        ClassSpec.ShieldKnightRecovery or ClassSpec.ShieldKnightShield => Classes.ShieldKnight,
        ClassSpec.HeavyGuardianEarthfort or ClassSpec.HeavyGuardianBlock => Classes.HeavyGuardian,
        ClassSpec.StormbladeIaidoSlash or ClassSpec.StormbladeMoonStrike => Classes.Stormblade,
        ClassSpec.WindKnightVanGuard or ClassSpec.WindKnightSkyward => Classes.WindKnight,
        ClassSpec.FrostMageIcicle or ClassSpec.FrostMageFrostBeam => Classes.FrostMage,
        ClassSpec.MarksmanWildpack or ClassSpec.MarksmanFalconry => Classes.Marksman,
        ClassSpec.VerdantOracleSmite or ClassSpec.VerdantOracleLifeBind => Classes.VerdantOracle,
        ClassSpec.SoulMusicianDissonance or ClassSpec.SoulMusicianConcerto => Classes.SoulMusician,
        _ => Classes.Unknown,
    };
}
