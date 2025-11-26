# BlueMeter Theme System

## Overview
BlueMeter now has a comprehensive theme system that allows easy customization of colors across the entire application. All themes are defined in a central location and automatically apply to:

- Panels and borders
- Text and foreground colors
- Headers and footers
- DPS Meter
- Module panels
- Buttons and toggles
- App title/name

## Adding a New Theme

### Step 1: Add to ThemeDefinitions.cs

Edit `BlueMeter.WPF/Config/ThemeDefinitions.cs` and add a new entry to the `Themes` list:

```csharp
new ThemeDefinition
{
    Id = "#YourHexColor",           // Unique identifier (hex color code)
    DisplayName = "YourNameMeter",   // App title (e.g., "PinkMeter", "CyberMeter")
    ColorHex = "#YourHexColor"       // The actual color in hex format
}
```

### Example: Adding an Emerald Theme

```csharp
// In ThemeDefinitions.cs, add to Themes list:
new ThemeDefinition
{
    Id = "#50C878",
    DisplayName = "EmeraldMeter",
    ColorHex = "#50C878"
}
```

That's it! The theme will automatically:
- Appear in Settings theme selector
- Update all panels when selected
- Change app title to "EmeraldMeter"
- Update text colors for contrast
- Update borders and accents
- Update DPS meter styling

### Example: Adding a Gradient Theme

For gradient themes, use the gradient name as ID:

```csharp
new ThemeDefinition
{
    Id = "Neon",  // Special gradient name
    DisplayName = "NeonMeter",
    ColorHex = "#FF00FF"  // Primary color used as fallback
}
```

Then update converters to handle the gradient name in their switch statements.

## Theme Architecture

### Core Files

1. **ThemeDefinitions.cs** - Central theme catalog (add themes here!)
2. **ThemeService.cs** - Service to manage theme state
3. **Converters/**
   - `ThemeColorConverter.cs` - Converts theme to color brush
   - `ThemeBackgroundConverter.cs` - Converts theme to background shades
   - `ThemeAccentConverter.cs` - Converts theme to accent color
   - `ThemeForegroundConverter.cs` - Converts theme to text color
   - `ThemeToAppNameConverter.cs` - Converts theme to app name

4. **Styles/ThemeResources.xaml** - Central resource dictionary with all converters

## How It Works

```
User selects theme in Settings
    ↓
AppConfig.ThemeColor = selected theme
    ↓
All XAML bindings with converters update:
    - ThemeColorConverter → Gets primary color
    - ThemeBackgroundConverter → Generates background shades
    - ThemeAccentConverter → Generates bright accent
    - ThemeForegroundConverter → Smart text color
    - ThemeToAppNameConverter → Sets app title
    ↓
Entire UI updates in real-time
```

## Current Themes

- **#1690F8** → BlueMeter (default)
- **#DC143C** → CrimsonMeter
- **#FF1493** → PinkMeter
- **#39FF14** → NeonMeter
- **#BF40BF** → PurpleMeter
- **#00FFFF** → CyberMeter
- **#FFD700** → GoldenMeter
- **#FF6B35** → OrangeMeter
- **#00FF00** → LimeMeter
- **#FF69B4** → FlashMeter
- **#40E0D0** → TurquoiseMeter
- **#FF7F50** → CoralMeter
- **#2F2F2F** → DarkMeter
- **#D0D0D0** → LightMeter
- **Rainbow** → RainbowMeter (gradient)
- **Sunset** → SunsetMeter (gradient)
- **Cyberpunk** → CyberMeter (gradient)

## Using Themes in XAML

Themes are automatically applied via converters. Example:

```xml
<!-- In MainView.xaml or any view -->
<Border Background="{Binding DataContext.AppConfig.ThemeColor,
        RelativeSource={RelativeSource AncestorType=Window},
        Converter={StaticResource ThemeBackgroundConverter},
        ConverterParameter=dark}" />

<TextBlock Foreground="{Binding DataContext.AppConfig.ThemeColor,
        Converter={StaticResource ThemeForegroundConverter},
        ConverterParameter=text}" />
```

## Best Practices

1. **Always use converters** - Don't hardcode colors, use theme converters
2. **Define colors in one place** - Add new themes to ThemeDefinitions.cs only
3. **Test with multiple themes** - Select different themes to ensure consistency
4. **Use ConverterParameter** - Use "dark", "medium", "light" for backgrounds and "text", "label", "accent" for text colors
5. **App name updates automatically** - Just add DisplayName to ThemeDefinition

## Extending Converters for Custom Logic

If you need custom color logic for a theme, edit the converter's switch statement:

```csharp
// In ThemeColorConverter.cs, ThemeBackgroundConverter.cs, etc.
switch (colorString.ToLower())
{
    case "mytheme":
        return new SolidColorBrush(Color.FromRgb(255, 128, 0)); // Custom logic
    // ...
}
```

## Theme Selection in Settings

Users can select themes in:
Settings → Appearance → Window Color

The theme applies instantly to the entire application.

---

**To add a new theme: Edit ThemeDefinitions.cs and add 5 lines of code!**
