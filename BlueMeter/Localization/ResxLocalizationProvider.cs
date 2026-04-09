using System.Globalization;
using System.Resources;
using BlueMeter.Properties;

namespace BlueMeter.Localization;

/// <summary>
/// Wraps the strongly-typed <see cref="Resources"/> ResX manager so it can be aggregated
/// alongside the JSON provider.
/// </summary>
public sealed class ResxLocalizationProvider : IDataLocalizationProvider
{
    private readonly ResourceManager _resourceManager;

    public ResxLocalizationProvider()
    {
        _resourceManager = Resources.ResourceManager;
    }

    public string? GetLocalizedString(string key, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(key)) return null;
        try
        {
            return _resourceManager.GetString(key, culture);
        }
        catch (MissingManifestResourceException)
        {
            return null;
        }
    }

    /// <summary>
    /// ResX uses satellite assemblies for cultures, so we cannot enumerate them cheaply at
    /// runtime. Returning the languages we ship with is fine — the aggregator de-duplicates.
    /// </summary>
    public IReadOnlyCollection<CultureInfo> AvailableCultures { get; } = new[]
    {
        CultureInfo.GetCultureInfo("en-US"),
        CultureInfo.GetCultureInfo("zh-CN"),
        CultureInfo.GetCultureInfo("pt-BR"),
    };

    public void OnCultureChanged(CultureInfo culture)
    {
        // ResourceManager picks up the thread culture automatically; nothing to evict.
    }
}
