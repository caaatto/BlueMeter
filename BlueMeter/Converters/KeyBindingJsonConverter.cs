using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Input;
using KeyBinding = BlueMeter.Models.KeyBinding;

namespace BlueMeter.Converters;

/// <summary>
/// JSON converter for <see cref="KeyBinding"/>. Writes <see cref="Key"/> and
/// <see cref="KeyModifiers"/> as enum-name strings to avoid the historical
/// <c>Key.None</c> ↔ <c>Key.D0</c> confusion when both serialize as <c>0</c>.
///
/// Reads accept legacy WPF spellings so old <c>config.json</c> files keep
/// deserializing — namely <c>"Windows"</c> for the meta key (Avalonia calls it
/// <see cref="KeyModifiers.Meta"/>) and <c>"Control"</c> ⇄ <c>"Ctrl"</c>.
/// </summary>
public class KeyBindingJsonConverter : JsonConverter<KeyBinding>
{
    public override KeyBinding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var key = Key.None;
        var modifiers = KeyModifiers.None;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new KeyBinding(key, modifiers);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            var propertyName = reader.GetString()!;
            reader.Read(); // Move to the value

            switch (propertyName.ToLowerInvariant())
            {
                case "key":
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        var keyString = reader.GetString();
                        if (!string.IsNullOrEmpty(keyString) && Enum.TryParse<Key>(keyString, true, out var parsedKey))
                        {
                            key = parsedKey;
                        }
                        else
                        {
                            key = Key.None;
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        var keyValue = reader.GetInt32();
                        // Explicitly handle 0 as Key.None to avoid D0 confusion
                        key = keyValue == 0 ? Key.None : (Key)keyValue;
                    }
                    break;

                case "modifiers":
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        modifiers = ParseModifiers(reader.GetString());
                    }
                    else if (reader.TokenType == JsonTokenType.Number)
                    {
                        // ModifierKeys (WPF) and KeyModifiers (Avalonia) share numeric values
                        // None=0, Alt=1, Control=2, Shift=4, Windows/Meta=8
                        modifiers = (KeyModifiers)reader.GetInt32();
                    }
                    break;
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, KeyBinding value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write Key as string to avoid confusion between Key.None (0) and Key.D0
        writer.WriteString("key", value.Key.ToString());

        // Write KeyModifiers as string for consistency
        writer.WriteString("modifiers", value.Modifiers.ToString());

        writer.WriteEndObject();
    }

    private static KeyModifiers ParseModifiers(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return KeyModifiers.None;

        var result = KeyModifiers.None;

        // Modifiers may be flag-combined (e.g. "Control, Shift" or "Ctrl+Shift").
        foreach (var raw in value.Split(new[] { ',', '+', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            switch (raw.Trim().ToLowerInvariant())
            {
                case "none":
                    break;
                case "ctrl":
                case "control":
                    result |= KeyModifiers.Control;
                    break;
                case "alt":
                    result |= KeyModifiers.Alt;
                    break;
                case "shift":
                    result |= KeyModifiers.Shift;
                    break;
                case "win":
                case "windows":
                case "meta":
                case "cmd":
                    result |= KeyModifiers.Meta;
                    break;
                default:
                    // Last-resort fallback: let Enum.TryParse take a swing at it.
                    if (Enum.TryParse<KeyModifiers>(raw, true, out var parsed))
                        result |= parsed;
                    break;
            }
        }

        return result;
    }
}
