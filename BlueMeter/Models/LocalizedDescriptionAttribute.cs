using System.ComponentModel;
using BlueMeter.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlueMeter.Models;

/// <summary>
/// Custom attribute for localized display names.
/// Resolves the resource string from the DI-managed <see cref="LocalizationManager"/>
/// and falls back to a default-constructed manager when DI is unavailable (design-time).
/// </summary>
/// <param name="resourceKey">Resource key (typically a constant from <c>ResourcesKeys</c>).</param>
public class LocalizedDescriptionAttribute(string resourceKey) : DescriptionAttribute
{
    public override string Description
    {
        get
        {
            // Try to resolve from DI
            var provider = App.Host?.Services;
            var loc = provider?.GetService<LocalizationManager>();
            if (loc != null) return loc.GetString(resourceKey);
            // Fallback for design-time
            var opts = new LocalizationConfiguration();
            return new LocalizationManager(opts, NullLogger<LocalizationManager>.Instance).GetString(resourceKey);
        }
    }
}
