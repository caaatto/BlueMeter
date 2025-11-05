using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using BlueMeter.WPF.Models;
using BlueMeter.WPF.Properties;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Providers;

namespace BlueMeter.WPF.Localization;

/// <summary>
/// Configuration class for localization settings.
/// </summary>
public sealed class LocalizationConfiguration
{
    /// <summary>
    /// The default directory for localization files.
    /// </summary>
    public const string DEFAULT_DIRECTORY = "Data";

    /// <summary>
    /// Gets or sets the directory where localization files are stored.
    /// </summary>
    public string LocalizationDirectory { get; set; } = DEFAULT_DIRECTORY;
}

/// <summary>
/// Manages localization for the WPF application, supporting both ResX and JSON providers.
/// </summary>
public sealed class LocalizationManager
{
    private readonly LocalizationConfiguration _config;
    private readonly ILogger<LocalizationManager> _logger;

    // private readonly string _defaultAssemblyName;
    private readonly CultureInfo _systemDefaultCultureInfo;
    private AggregatedLocalizationProvider _aggregatedProvider;
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationManager"/> class.
    /// </summary>
    /// <param name="config">The localization configuration.</param>
    /// <param name="logger"></param>
    public LocalizationManager(LocalizationConfiguration config, ILogger<LocalizationManager> logger)
    {
        _config = config;
        _logger = logger;
        _systemDefaultCultureInfo = CultureInfo.CurrentUICulture;
        // var assemblyName = typeof(App).Assembly.GetName().Name;
        // _defaultAssemblyName = string.IsNullOrWhiteSpace(assemblyName)
        //     ? Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty
        //     : assemblyName;

        _aggregatedProvider = null!; // Will be initialized in ConfigureProviders
        ConfigureProviders();
        LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
        LocalizeDictionary.Instance.IncludeInvariantCulture = true;
        LocalizeDictionary.Instance.MissingKeyEvent += InstanceOnMissingKeyEvent;
        Instance = this;
    }

    private void InstanceOnMissingKeyEvent(object? sender, MissingKeyEventArgs e)
    {
        _logger.LogWarning(e.Key);
        var ret = GetString(e.Key, CultureInfo.InvariantCulture); // Get the fallback string
        if (!string.IsNullOrEmpty(ret))
        {
            e.MissingKeyResult = ret;
        }
    }

    /// <summary>
    /// Occurs when the culture is changed.
    /// </summary>
    public event EventHandler<CultureInfo>? CultureChanged;

    /// <summary>
    /// Gets the singleton instance of the <see cref="LocalizationManager"/>.
    /// </summary>
    public static LocalizationManager Instance { get; private set; } = new(new LocalizationConfiguration(), NullLogger<LocalizationManager>.Instance); // this instance will be used in design time viewmodel

    /// <summary>
    /// Applies the specified language to the application.
    /// </summary>
    /// <param name="language">The language to apply.</param>
    public void ApplyLanguage(Language language)
    {
        var targetCulture = ResolveCulture(language);
        ApplyCulture(targetCulture);
    }

    /// <summary>
    /// Gets the current language based on the active culture.
    /// </summary>
    /// <returns>The current <see cref="Language"/>.</returns>
    public Language GetCurrentLanguage()
    {
        return CultureAttributeExtensions.FromCultureInfo(LocalizeDictionary.Instance.Culture ??
                                                          CultureInfo.CurrentUICulture);
    }

    /// <summary>
    /// Retrieves the localized string for the given key.
    /// </summary>
    /// <param name="key">The localization key.</param>
    /// <param name="cultureInfo">Force culture info</param>
    /// <returns>The localized string, or the key if not found.</returns>
    public string GetString(string key, CultureInfo? cultureInfo = null)
    {
        var culture = cultureInfo ?? LocalizeDictionary.Instance.Culture ?? CultureInfo.CurrentUICulture;
        var localized = _aggregatedProvider.GetLocalizedObject(key, null, culture) as string;

        return !string.IsNullOrEmpty(localized)
            ? localized
            : key;
    }

    /// <summary>
    /// Initializes the localization manager with the specified language.
    /// </summary>
    /// <param name="language">The initial language.</param>
    public void Initialize(Language language)
    {
        if (_initialized) return;
        ApplyLanguage(language);
        _initialized = true;
    }

    /// <summary>
    /// Resolves the <see cref="CultureInfo"/> for the given language.
    /// </summary>
    /// <param name="language">The language to resolve.</param>
    /// <returns>The corresponding <see cref="CultureInfo"/>.</returns>
    public CultureInfo ResolveCulture(Language language)
    {
        if (language == Language.Auto)
        {
            return _systemDefaultCultureInfo;
        }

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
        LocalizeDictionary.Instance.Culture = culture;
        Resources.Culture = culture;
        SetThreadCulture(culture);
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
        _aggregatedProvider = new AggregatedLocalizationProvider(ResxLocalizationProvider.Instance, jsonProvider);
        LocalizeDictionary.Instance.DefaultProvider = _aggregatedProvider;
    }

    private void OnCultureChanged(CultureInfo e)
    {
        CultureChanged?.Invoke(this, e);
    }
}