using Avalonia.Data;

namespace BlueMeter.Localization;

/// <summary>
/// XAML markup extension that resolves a localization key against the active
/// <see cref="LocalizationManager"/>.
///
/// Usage:
/// <code>
/// xmlns:loc="using:BlueMeter.Localization"
/// ...
/// &lt;TextBlock Text="{loc:Localize Window_About_Title}" /&gt;
/// </code>
///
/// The extension returns a one-way <see cref="Binding"/> against the manager's
/// string indexer (<c>LocalizationManager.Instance[Key]</c>), so cycling the
/// active culture re-evaluates every bound text. The manager raises
/// <c>PropertyChanged("Item[]")</c> on culture change which is the magic
/// property name both WPF and Avalonia use to invalidate indexer bindings.
///
/// Replaces WPF's <c>{lex:Loc Key=...}</c> from WPFLocalizeExtension.
/// </summary>
public sealed class LocalizeExtension
{
    public LocalizeExtension()
    {
    }

    public LocalizeExtension(string key)
    {
        Key = key;
    }

    public string? Key { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding($"[{Key}]")
        {
            Source = LocalizationManager.Instance,
            Mode = BindingMode.OneWay,
        };
    }
}
