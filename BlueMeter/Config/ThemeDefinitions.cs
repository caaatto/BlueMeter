namespace BlueMeter.Config;

/// <summary>
/// Centralized theme definitions. Easy to extend with new themes.
/// </summary>
public class ThemeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string? AccentColor { get; set; }
    public string? DarkVariant { get; set; }
}

/// <summary>
/// Theme catalog — add new themes here.
/// </summary>
public static class ThemeDefinitions
{
    public static readonly List<ThemeDefinition> Themes = new()
    {
        // Classic Blue (Default) - Cobalt Blue
        new ThemeDefinition { Id = "#0047AB", DisplayName = "BlueMeter",       ColorHex = "#0047AB" },
        new ThemeDefinition { Id = "#DC143C", DisplayName = "CrimsonMeter",    ColorHex = "#DC143C" },
        new ThemeDefinition { Id = "#FF1493", DisplayName = "PinkMeter",       ColorHex = "#FF1493" },
        new ThemeDefinition { Id = "#39FF14", DisplayName = "NeonMeter",       ColorHex = "#39FF14" },
        new ThemeDefinition { Id = "#BF40BF", DisplayName = "PurpleMeter",     ColorHex = "#BF40BF" },
        new ThemeDefinition { Id = "#00FFFF", DisplayName = "CyberMeter",      ColorHex = "#00FFFF" },
        new ThemeDefinition { Id = "#FFD700", DisplayName = "GoldenMeter",     ColorHex = "#FFD700" },
        new ThemeDefinition { Id = "#FF6B35", DisplayName = "OrangeMeter",     ColorHex = "#FF6B35" },
        new ThemeDefinition { Id = "#00FF00", DisplayName = "LimeMeter",       ColorHex = "#00FF00" },
        new ThemeDefinition { Id = "#FF69B4", DisplayName = "FlashMeter",      ColorHex = "#FF69B4" },
        new ThemeDefinition { Id = "#40E0D0", DisplayName = "TurquoiseMeter",  ColorHex = "#40E0D0" },
        new ThemeDefinition { Id = "#FF7F50", DisplayName = "CoralMeter",      ColorHex = "#FF7F50" },
        new ThemeDefinition { Id = "#2F2F2F", DisplayName = "DarkMeter",       ColorHex = "#2F2F2F" },
        new ThemeDefinition { Id = "#D0D0D0", DisplayName = "LightMeter",      ColorHex = "#D0D0D0" },

        // Special Gradient Themes
        new ThemeDefinition { Id = "Rainbow",     DisplayName = "RainbowMeter", ColorHex = "#FF007F" },
        new ThemeDefinition { Id = "Sunset",      DisplayName = "SunsetMeter",  ColorHex = "#FF6B6B" },
        new ThemeDefinition { Id = "Cyberpunk",   DisplayName = "CyberMeter",   ColorHex = "#FF006E" },
        new ThemeDefinition { Id = "Transparent", DisplayName = "BlueMeter",    ColorHex = "#1690F8" },

        // 🎄 Special: Christmas Theme
        new ThemeDefinition
        {
            Id = "Christmas",
            DisplayName = "ChristmasMeter 🎄",
            ColorHex = "#C41E3A",
            AccentColor = "#165B33",
            DarkVariant = "#8B0000"
        }
    };

    /// <summary>
    /// Get theme definition by color/ID. Returns the default (first) theme when no match is found.
    /// </summary>
    public static ThemeDefinition? GetTheme(string? colorOrId)
    {
        if (string.IsNullOrEmpty(colorOrId))
            return Themes.FirstOrDefault();

        return Themes.FirstOrDefault(t =>
            t.Id.Equals(colorOrId, StringComparison.OrdinalIgnoreCase) ||
            t.ColorHex.Equals(colorOrId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get app display name based on theme.
    /// </summary>
    public static string GetAppName(string? colorOrId)
    {
        if (string.IsNullOrEmpty(colorOrId))
            return "BlueMeter";

        var theme = GetTheme(colorOrId);
        if (theme != null)
            return theme.DisplayName;

        // Custom hex color → "CustomMeter"
        if (colorOrId.StartsWith('#') && colorOrId.Length == 7)
        {
            return "CustomMeter";
        }

        return "BlueMeter";
    }
}
