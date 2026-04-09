using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using BlueMeter.Models;
using BlueMeter.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlueMeter.Localization;

/// <summary>
/// Configuration for the localization layer.
/// </summary>
public sealed class LocalizationConfiguration
{
    /// <summary>
    /// Default folder (under <see cref="AppContext.BaseDirectory"/>) for JSON localization files.
    /// </summary>
    public const string DEFAULT_DIRECTORY = "Data";

    /// <summary>
    /// Directory where the JSON localization files live. May be absolute or relative to the app base directory.
    /// </summary>
    public string LocalizationDirectory { get; set; } = DEFAULT_DIRECTORY;
}

/// <summary>
/// Avalonia-friendly localization manager. Holds the active culture, raises a change event,
/// resolves keys via the aggregated provider, and exposes a string indexer so XAML can bind
/// to <c>{Binding [SomeKey], Source={x:Static loc:LocalizationManager.Instance}}</c> in later phases.
/// </summary>
public sealed class LocalizationManager : INotifyPropertyChanged
{
    private readonly LocalizationConfiguration _config;
    private readonly ILogger<LocalizationManager> _logger;
    private readonly CultureInfo _systemDefaultCultureInfo;
    private AggregatedLocalizationProvider _aggregatedProvider;
    private CultureInfo _currentCulture;
    private bool _initialized;

    public LocalizationManager(LocalizationConfiguration config, ILogger<LocalizationManager> logger)
    {
        _config = config;
        _logger = logger;
        _systemDefaultCultureInfo = CultureInfo.CurrentUICulture;
        _currentCulture = _systemDefaultCultureInfo;
        _aggregatedProvider = null!; // assigned in ConfigureProviders
        ConfigureProviders();
        Instance = this;
    }

    /// <summary>
    /// Singleton instance — populated when DI constructs the manager. Defaults to a design-time
    /// instance so XAML and attribute lookups still resolve when running outside the host.
    /// </summary>
    public static LocalizationManager Instance { get; private set; } =
        new(new LocalizationConfiguration(), NullLogger<LocalizationManager>.Instance);

    /// <summary>
    /// Raised when the active culture changes. Carries the new <see cref="CultureInfo"/>.
    /// </summary>
    public event EventHandler<CultureInfo>? CultureChanged;

    /// <summary>
    /// Raised when the indexer needs to re-evaluate (after a culture change).
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Currently active culture.
    /// </summary>
    public CultureInfo CurrentCulture => _currentCulture;

    /// <summary>
    /// Indexer used by XAML bindings: <c>{Binding [Settings_Title], Source={x:Static loc:LocalizationManager.Instance}}</c>.
    /// </summary>
    public string this[string key] => GetString(key);

    /// <summary>
    /// Apply a language. Uses the system culture for <see cref="Language.Auto"/>.
    /// </summary>
    public void ApplyLanguage(Language language)
    {
        ApplyCulture(ResolveCulture(language));
    }

    /// <summary>
    /// Returns the <see cref="Language"/> matching the active culture (or <see cref="Language.Auto"/>).
    /// </summary>
    public Language GetCurrentLanguage()
    {
        return CultureAttributeExtensions.FromCultureInfo(_currentCulture);
    }

    /// <summary>
    /// Look up a localized string. Returns the key itself if no translation is found.
    /// </summary>
    public string GetString(string key, CultureInfo? cultureInfo = null)
    {
        var culture = cultureInfo ?? _currentCulture;
        var localized = _aggregatedProvider.GetLocalizedString(key, culture);
        if (string.IsNullOrEmpty(localized))
        {
            _logger.LogWarning("Missing localization key: {Key}", key);
            return key;
        }
        return localized;
    }

    /// <summary>
    /// First-time initialization. Subsequent calls are no-ops.
    /// </summary>
    public void Initialize(Language language)
    {
        if (_initialized) return;
        ApplyLanguage(language);
        _initialized = true;
    }

    /// <summary>
    /// Resolves the <see cref="CultureInfo"/> for a <see cref="Language"/>, falling back to the
    /// system culture for <see cref="Language.Auto"/> or unknown cultures.
    /// </summary>
    public CultureInfo ResolveCulture(Language language)
    {
        if (language == Language.Auto)
            return _systemDefaultCultureInfo;

        try
        {
            var ret = language.GetCultureInfo();
            Debug.Assert(ret != null, nameof(ret) + " != null");
            return ret;
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.CurrentUICulture;
        }
    }

    private static void SetThreadCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private void ApplyCulture(CultureInfo culture)
    {
        _currentCulture = culture;
        Resources.Culture = culture;
        SetThreadCulture(culture);
        _aggregatedProvider.OnCultureChanged(culture);
        OnCultureChanged(culture);
    }

    private void ConfigureProviders()
    {
        var baseDir = AppContext.BaseDirectory;
        var locDir = string.IsNullOrWhiteSpace(_config.LocalizationDirectory)
            ? "Localization"
            : _config.LocalizationDirectory;
        var path = Path.IsPathRooted(locDir) ? locDir : Path.Combine(baseDir, locDir);

        var jsonProvider = new JsonLocalizationProvider(path);
        var resxProvider = new ResxLocalizationProvider();
        _aggregatedProvider = new AggregatedLocalizationProvider(resxProvider, jsonProvider);
    }

    private void OnCultureChanged(CultureInfo e)
    {
        CultureChanged?.Invoke(this, e);
        // Item[] is the magic property name that re-evaluates indexer bindings in WPF/Avalonia.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }
}
