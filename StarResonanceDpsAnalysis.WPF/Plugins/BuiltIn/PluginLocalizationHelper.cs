using System.Globalization;
using StarResonanceDpsAnalysis.WPF.Localization;
using StarResonanceDpsAnalysis.WPF.Properties;

namespace StarResonanceDpsAnalysis.WPF.Plugins.BuiltIn;

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
