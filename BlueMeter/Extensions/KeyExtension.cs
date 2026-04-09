using Avalonia.Input;

namespace BlueMeter.Extensions;

/// <summary>
/// Helpers for working with Avalonia key + modifier values. Mirrors the WPF version
/// in <c>BlueMeter.WPF/Extensions/KeyExtension.cs</c> but rewritten against
/// <see cref="Avalonia.Input.Key"/> and <see cref="Avalonia.Input.KeyModifiers"/>.
/// </summary>
public static class KeyExtension
{
    /// <summary>
    /// Format a key + modifier combination for display ("Ctrl+Shift+F6"). Returns an
    /// empty string when <paramref name="key"/> is <see cref="Key.None"/>.
    /// </summary>
    public static string KeyToString(this Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        if (key == Key.None)
            return string.Empty;

        var parts = new List<string>();

        if (modifiers.HasFlag(KeyModifiers.Control))
            parts.Add("Ctrl");
        if (modifiers.HasFlag(KeyModifiers.Alt))
            parts.Add("Alt");
        if (modifiers.HasFlag(KeyModifiers.Shift))
            parts.Add("Shift");
        if (modifiers.HasFlag(KeyModifiers.Meta))
            parts.Add("Win");

        // Don't list the modifier physical keys as the "main" key
        if (!key.IsAltKey() && !key.IsControlKey() && !key.IsShiftKey())
        {
            parts.Add(key.ToString());
        }

        return string.Join("+", parts);
    }

    public static bool IsControlKey(this Key key)
    {
        return key is Key.LeftCtrl or Key.RightCtrl;
    }

    public static bool IsAltKey(this Key key)
    {
        return key is Key.LeftAlt or Key.RightAlt;
    }

    public static bool IsShiftKey(this Key key)
    {
        return key is Key.LeftShift or Key.RightShift;
    }

    /// <summary>
    /// Parse a shortcut string like "Ctrl+Shift+F6" into a key + modifier pair.
    /// Accepts both Avalonia ("Control") and the legacy WPF/UI ("Ctrl") spellings so
    /// existing serialized config files keep deserializing.
    /// </summary>
    public static (Key key, KeyModifiers modifiers) ParseShortcutString(this string shortcut)
    {
        var key = Key.None;
        var modifiers = KeyModifiers.None;

        if (string.IsNullOrEmpty(shortcut)) return (key, modifiers);

        foreach (var part in shortcut.Split('+').Select(p => p.Trim()))
        {
            switch (part.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    modifiers |= KeyModifiers.Control;
                    break;
                case "alt":
                    modifiers |= KeyModifiers.Alt;
                    break;
                case "shift":
                    modifiers |= KeyModifiers.Shift;
                    break;
                case "win":
                case "meta":
                case "cmd":
                    modifiers |= KeyModifiers.Meta;
                    break;
                default:
                    if (Enum.TryParse<Key>(part, true, out var parsedKey))
                        key = parsedKey;
                    break;
            }
        }

        return (key, modifiers);
    }
}
