using System.Collections.Generic;
using System.Linq;

namespace BlueMeter.WPF.Config;

/// <summary>
/// Centralized theme definitions. Easy to extend with new themes.
/// </summary>
public class ThemeDefinition
{
    public string Id { get; set; } = string.Empty; // e.g., "blue", "cyberpunk", "sunset"
    public string DisplayName { get; set; } = string.Empty; // e.g., "BlueMeter", "CyberMeter", "SunsetMeter"
    public string ColorHex { get; set; } = string.Empty; // e.g., "#1690F8"
    public string? AccentColor { get; set; } // Optional override for accent color
    public string? DarkVariant { get; set; } // Optional override for dark background
}

/// <summary>
/// Theme catalog - ADD NEW THEMES HERE!
/// </summary>
public static class ThemeDefinitions
{
    // Default themes - Easy to add more!
    public static readonly List<ThemeDefinition> Themes = new()
    {
        // Classic Blue (Default)
        new ThemeDefinition
        {
            Id = "#1690F8",
            DisplayName = "BlueMeter",
            ColorHex = "#1690F8"
        },

        // Reds
        new ThemeDefinition
        {
            Id = "#DC143C",
            DisplayName = "CrimsonMeter",
            ColorHex = "#DC143C"
        },

        // Pinks
        new ThemeDefinition
        {
            Id = "#FF1493",
            DisplayName = "PinkMeter",
            ColorHex = "#FF1493"
        },

        // Greens
        new ThemeDefinition
        {
            Id = "#39FF14",
            DisplayName = "NeonMeter",
            ColorHex = "#39FF14"
        },

        // Purples
        new ThemeDefinition
        {
            Id = "#BF40BF",
            DisplayName = "PurpleMeter",
            ColorHex = "#BF40BF"
        },

        // Cyan
        new ThemeDefinition
        {
            Id = "#00FFFF",
            DisplayName = "CyberMeter",
            ColorHex = "#00FFFF"
        },

        // Gold
        new ThemeDefinition
        {
            Id = "#FFD700",
            DisplayName = "GoldenMeter",
            ColorHex = "#FFD700"
        },

        // Orange
        new ThemeDefinition
        {
            Id = "#FF6B35",
            DisplayName = "OrangeMeter",
            ColorHex = "#FF6B35"
        },

        // Lime
        new ThemeDefinition
        {
            Id = "#00FF00",
            DisplayName = "LimeMeter",
            ColorHex = "#00FF00"
        },

        // Hot Pink
        new ThemeDefinition
        {
            Id = "#FF69B4",
            DisplayName = "FlashMeter",
            ColorHex = "#FF69B4"
        },

        // Turquoise
        new ThemeDefinition
        {
            Id = "#40E0D0",
            DisplayName = "TurquoiseMeter",
            ColorHex = "#40E0D0"
        },

        // Coral
        new ThemeDefinition
        {
            Id = "#FF7F50",
            DisplayName = "CoralMeter",
            ColorHex = "#FF7F50"
        },

        // Dark
        new ThemeDefinition
        {
            Id = "#2F2F2F",
            DisplayName = "DarkMeter",
            ColorHex = "#2F2F2F"
        },

        // Light
        new ThemeDefinition
        {
            Id = "#D0D0D0",
            DisplayName = "LightMeter",
            ColorHex = "#D0D0D0"
        },

        // Special Gradient Themes
        new ThemeDefinition
        {
            Id = "Rainbow",
            DisplayName = "RainbowMeter",
            ColorHex = "#FF007F"
        },

        new ThemeDefinition
        {
            Id = "Sunset",
            DisplayName = "SunsetMeter",
            ColorHex = "#FF6B6B"
        },

        new ThemeDefinition
        {
            Id = "Cyberpunk",
            DisplayName = "CyberMeter",
            ColorHex = "#FF006E"
        },

        new ThemeDefinition
        {
            Id = "Transparent",
            DisplayName = "BlueMeter",
            ColorHex = "#1690F8" // Fallback to blue
        }
    };

    /// <summary>
    /// Get theme definition by color/ID
    /// </summary>
    public static ThemeDefinition? GetTheme(string? colorOrId)
    {
        if (string.IsNullOrEmpty(colorOrId))
            return Themes.FirstOrDefault(); // Default to first (blue)

        return Themes.FirstOrDefault(t =>
            t.Id.Equals(colorOrId, System.StringComparison.OrdinalIgnoreCase) ||
            t.ColorHex.Equals(colorOrId, System.StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Get app display name based on theme
    /// </summary>
    public static string GetAppName(string? colorOrId)
    {
        if (string.IsNullOrEmpty(colorOrId))
            return "BlueMeter";

        var theme = GetTheme(colorOrId);
        if (theme != null)
            return theme.DisplayName;

        // If theme not found in definitions, generate a name from the color
        // This handles custom colors that users might set
        if (colorOrId.StartsWith('#') && colorOrId.Length == 7)
        {
            return "CustomMeter";
        }

        return "BlueMeter";
    }
}
