using Avalonia.Input;

namespace BlueMeter.Models;

/// <summary>
/// Persistable key binding (shortcut). Uses Avalonia input enums so it can be passed
/// straight into hotkey services without conversion.
/// </summary>
public record KeyBinding(Key Key, KeyModifiers Modifiers);
