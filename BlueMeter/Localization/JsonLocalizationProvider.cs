using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace BlueMeter.Localization;

/// <summary>
/// Loads game-data translations (monsters, debug data) from JSON files on disk.
/// File naming convention: <c>{resource}\{resource}.{culture}.json</c>.
/// </summary>
public sealed class JsonLocalizationProvider : IDataLocalizationProvider
{
    private readonly string _basePath;
    private readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> _resources = new();

    private readonly (string resourceName, string pattern)[] _filenamePatterns =
    [
        ("Monster", "Monster\\monster.{0}.json"),
        ("DebugData", "DebugData\\debugData.{0}.json"),
    ];

    public JsonLocalizationProvider(string basePath)
    {
        _basePath = basePath;
        AvailableCultures = GetAvailableCulturesInternal().ToArray();
    }

    public IReadOnlyCollection<CultureInfo> AvailableCultures { get; }

    public string? GetLocalizedString(string key, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(key)) return null;

        // Support fully qualified keys like "Assembly:Resource:Key" or "Assembly:Resource:Sub:Key".
        // Last segment is the actual lookup key; second segment (if present) filters the resource file.
        var lookupKey = key;
        string? resourceName = null;

        var parts = key.Split(':');
        if (parts.Length >= 2)
        {
            lookupKey = parts[^1];
            resourceName = parts[1].ToLowerInvariant();
        }

        var current = culture;

        // Walk up the culture chain: requested -> parent -> ... -> invariant
        while (!Equals(current, CultureInfo.InvariantCulture))
        {
            var lang = current.Name;
            EnsureResourcesLoaded(lang);

            if (_resources.TryGetValue(lang, out var resDicts))
            {
                string? value = null;
                if (!string.IsNullOrEmpty(resourceName) && resDicts.TryGetValue(resourceName, out var dict))
                {
                    dict.TryGetValue(lookupKey, out value);
                }
                else if (string.IsNullOrEmpty(resourceName))
                {
                    foreach (var resDict in resDicts.Values)
                    {
                        if (resDict.TryGetValue(lookupKey, out value))
                            break;
                    }
                }

                if (value != null) return value;
            }

            current = current.Parent;
        }

        return null;
    }

    public void OnCultureChanged(CultureInfo culture)
    {
        // Evict cached entries for this specific culture so on-disk edits during a session take effect.
        _resources.Remove(culture.Name);
    }

    private void EnsureResourcesLoaded(string lang)
    {
        if (_resources.ContainsKey(lang))
            return;

        var resDicts = new Dictionary<string, Dictionary<string, string>>();
        _resources[lang] = resDicts;

        foreach (var (resName, pattern) in _filenamePatterns)
        {
            var path = Path.Combine(_basePath, string.Format(pattern, lang));
            if (!File.Exists(path))
                continue;

            try
            {
                var text = File.ReadAllText(path);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                if (dict != null)
                {
                    resDicts[resName.ToLowerInvariant()] = dict;
                }
            }
            catch
            {
                // Ignore malformed files
            }
        }
    }

    private IEnumerable<CultureInfo> GetAvailableCulturesInternal()
    {
        if (!Directory.Exists(_basePath))
            yield break;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (resName, pattern) in _filenamePatterns)
        {
            var dir = Path.GetDirectoryName(pattern);
            if (string.IsNullOrEmpty(dir))
                continue;

            var fullDir = Path.Combine(_basePath, dir);
            if (!Directory.Exists(fullDir))
                continue;

            foreach (var file in Directory.GetFiles(fullDir, "*.json"))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var parts = fileName.Split('.');
                if (parts.Length < 2 || !string.Equals(parts[0], resName, StringComparison.OrdinalIgnoreCase))
                    continue;

                var cultureCode = parts[1];
                CultureInfo? culture = null;
                try
                {
                    culture = new CultureInfo(cultureCode);
                }
                catch
                {
                    // Skip invalid culture codes
                }

                if (culture != null && seen.Add(culture.Name))
                    yield return culture;
            }
        }
    }
}
