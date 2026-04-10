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
    /// Map an <see cref="Avalonia.Input.Key"/> to its Win32 virtual-key code for use
    /// with <c>RegisterHotKey</c>. Returns <c>0</c> for keys that don't have a
    /// stable VK mapping (Avalonia's Key enum includes values that aren't bound to
    /// Win32 VKs, e.g. OEM/IME-specific keys we don't support as hotkeys).
    /// </summary>
    /// <remarks>
    /// WPF used <c>KeyInterop.VirtualKeyFromKey(Key)</c> for the same job. Avalonia
    /// doesn't ship a KeyInterop helper, and its <see cref="Avalonia.Input.Key"/>
    /// enum values are NOT identical to Win32 VK codes (the enum is numbered from 0
    /// in declaration order), so this explicit switch covers every key the app
    /// actually allows as a global hotkey target.
    /// </remarks>
    public static uint ToVirtualKey(this Key key)
    {
        // F1..F24
        if (key >= Key.F1 && key <= Key.F24)
        {
            return (uint)(0x70 + (key - Key.F1));
        }

        // A..Z
        if (key >= Key.A && key <= Key.Z)
        {
            return (uint)(0x41 + (key - Key.A));
        }

        // D0..D9 (top-row digits)
        if (key >= Key.D0 && key <= Key.D9)
        {
            return (uint)(0x30 + (key - Key.D0));
        }

        // NumPad0..NumPad9
        if (key >= Key.NumPad0 && key <= Key.NumPad9)
        {
            return (uint)(0x60 + (key - Key.NumPad0));
        }

        return key switch
        {
            Key.Back => 0x08,
            Key.Tab => 0x09,
            Key.Clear => 0x0C,
            Key.Enter => 0x0D, // a.k.a. Return
            Key.Pause => 0x13,
            Key.CapsLock => 0x14,
            Key.Escape => 0x1B,
            Key.Space => 0x20,
            Key.PageUp => 0x21,
            Key.PageDown => 0x22,
            Key.End => 0x23,
            Key.Home => 0x24,
            Key.Left => 0x25,
            Key.Up => 0x26,
            Key.Right => 0x27,
            Key.Down => 0x28,
            Key.Select => 0x29,
            Key.Print => 0x2A,
            Key.Execute => 0x2B,
            Key.PrintScreen => 0x2C, // Snapshot
            Key.Insert => 0x2D,
            Key.Delete => 0x2E,
            Key.Help => 0x2F,
            Key.LWin => 0x5B,
            Key.RWin => 0x5C,
            Key.Apps => 0x5D,
            Key.Sleep => 0x5F,
            Key.Multiply => 0x6A,
            Key.Add => 0x6B,
            Key.Separator => 0x6C,
            Key.Subtract => 0x6D,
            Key.Decimal => 0x6E,
            Key.Divide => 0x6F,
            Key.NumLock => 0x90,
            Key.Scroll => 0x91,
            _ => 0u,
        };
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
