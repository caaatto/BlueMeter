using System.Reflection;

namespace StarResonanceDpsAnalysis.WPF.Models;

/// <summary>
/// Enum helper class
/// </summary>
public static class EnumLocalizer
{
    public static string GetLocalizedDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<LocalizedDescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    public static IEnumerable<KeyValuePair<object, string>> GetLocalizedValues(Type enumType)
    {
        return Enum.GetValues(enumType)
            .Cast<object>()
            .Select(value => new KeyValuePair<object, string>(
                value,
                GetLocalizedDescription((Enum)value)
            ));
    }
}