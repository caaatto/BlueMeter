namespace BlueMeter.Services.Theming;

/// <summary>
/// Resolves the currently active holiday theme based on the calendar.
/// Pulled out of the static <c>HolidayThemeService</c> in BlueMeter.WPF so
/// <see cref="BlueMeter.Config.AppConfig"/> (a JSON-deserialized POCO) can
/// resolve it lazily through the DI container instead of via a hard static
/// reference. This also makes the seasonal-decoration logic testable.
/// </summary>
public interface IHolidayThemeProvider
{
    /// <summary>
    /// The active holiday theme id (e.g. <c>"Christmas"</c>) or <c>null</c> when
    /// no holiday window is active.
    /// </summary>
    string? GetCurrentHolidayTheme();

    /// <summary>
    /// The display name for the active holiday (e.g. <c>"Christmas 🎄"</c>) or
    /// <c>null</c> when no holiday is active.
    /// </summary>
    string? GetCurrentHolidayName();

    /// <summary>
    /// Convenience: <c>true</c> when <see cref="GetCurrentHolidayTheme"/> is non-null.
    /// </summary>
    bool IsHolidayActive();
}
