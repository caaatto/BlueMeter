using System.Globalization;
using BlueMeter.WPF.Localization;
using BlueMeter.WPF.Properties;

namespace BlueMeter.WPF.Plugins.BuiltIn;

internal static class PluginLocalizationHelper
{
    public static string GetString(string resourceKey, string? cultureCode)
    {
        var culture = CultureInfo.CurrentUICulture;

        if (!string.IsNullOrWhiteSpace(cultureCode))
        {
            try
            {
                culture = CultureInfo.GetCultureInfo(cultureCode);
            }
            catch (CultureNotFoundException)
            {
            }
        }

        var localized = LocalizationManager.Instance.GetString(resourceKey, culture);
        if (!string.IsNullOrEmpty(localized))
        {
            return localized;
        }

        localized = LocalizationManager.Instance.GetString(resourceKey, CultureInfo.InvariantCulture);
        return !string.IsNullOrEmpty(localized) ? localized : resourceKey;
    }
}
