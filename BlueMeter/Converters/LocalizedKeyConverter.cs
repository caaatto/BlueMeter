using System.Globalization;
using Avalonia.Data.Converters;
using BlueMeter.Localization;

namespace BlueMeter.Converters;

/// <summary>
/// Composes a localization resource key from a <c>{Prefix}_{value}</c>
/// pattern and returns the localized string. The prefix comes in via the
/// <see cref="IValueConverter"/> parameter — e.g. <c>ConverterParameter=ClassSpec</c>
/// turns a <c>ClassSpec.ShieldKnightRecovery</c> binding into the key
/// <c>ClassSpec_ShieldKnightRecovery</c>, then looks that up against the
/// active <see cref="LocalizationManager"/>.
///
/// Use this for bindings where the enum/model type lives in a project that
/// can't depend on <c>BlueMeter.Models.LocalizedDescriptionAttribute</c>
/// (e.g. <c>BlueMeter.Core</c>), so you can't put <c>[LocalizedDescription]</c>
/// on the enum itself.
/// </summary>
public class LocalizedKeyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        var prefix = parameter as string;
        var name = value.ToString();
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var key = string.IsNullOrEmpty(prefix) ? name : $"{prefix}_{name}";
        return LocalizationManager.Instance.GetString(key);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
