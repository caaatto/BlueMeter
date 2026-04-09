using System.Globalization;

namespace BlueMeter.Localization;

/// <summary>
/// Aggregates the JSON and ResX providers, walking the culture chain so requests for
/// e.g. "zh-Hant" fall back to "zh" and finally to invariant.
/// </summary>
public sealed class AggregatedLocalizationProvider : IDataLocalizationProvider
{
    private readonly IDataLocalizationProvider _jsonProvider;
    private readonly IDataLocalizationProvider _resxProvider;

    public AggregatedLocalizationProvider(IDataLocalizationProvider resxProvider, IDataLocalizationProvider jsonProvider)
    {
        _resxProvider = resxProvider ?? throw new ArgumentNullException(nameof(resxProvider));
        _jsonProvider = jsonProvider ?? throw new ArgumentNullException(nameof(jsonProvider));

        AvailableCultures = _jsonProvider.AvailableCultures
            .Concat(_resxProvider.AvailableCultures)
            .DistinctBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public string? GetLocalizedString(string key, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(key)) return null;

        var current = culture;

        while (!Equals(current, CultureInfo.InvariantCulture))
        {
            // Try JSON first (more specific game data), then fall back to ResX (UI strings).
            var fromJson = _jsonProvider.GetLocalizedString(key, current);
            if (fromJson != null) return fromJson;

            var fromResx = _resxProvider.GetLocalizedString(key, current);
            if (fromResx != null) return fromResx;

            current = current.Parent;
        }

        var invariantJson = _jsonProvider.GetLocalizedString(key, CultureInfo.InvariantCulture);
        return invariantJson ?? _resxProvider.GetLocalizedString(key, CultureInfo.InvariantCulture);
    }

    public IReadOnlyCollection<CultureInfo> AvailableCultures { get; }

    public void OnCultureChanged(CultureInfo culture)
    {
        _jsonProvider.OnCultureChanged(culture);
        _resxProvider.OnCultureChanged(culture);
    }
}
