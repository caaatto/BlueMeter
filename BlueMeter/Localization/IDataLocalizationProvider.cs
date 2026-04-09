using System.Globalization;

namespace BlueMeter.Localization;

/// <summary>
/// Avalonia-friendly localization provider contract. Replaces the WPFLocalizeExtension
/// <c>ILocalizationProvider</c> interface, which depended on <c>DependencyObject</c>.
/// </summary>
public interface IDataLocalizationProvider
{
    /// <summary>
    /// Looks up a localized string for the given key and culture.
    /// Returns <c>null</c> when no value is found in this provider.
    /// </summary>
    string? GetLocalizedString(string key, CultureInfo culture);

    /// <summary>
    /// Cultures that this provider has data for.
    /// </summary>
    IReadOnlyCollection<CultureInfo> AvailableCultures { get; }

    /// <summary>
    /// Notifies the provider that the active culture changed and any cached
    /// per-culture state should be evicted/reloaded.
    /// </summary>
    void OnCultureChanged(CultureInfo culture);
}
