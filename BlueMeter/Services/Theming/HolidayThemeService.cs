namespace BlueMeter.Services.Theming;

/// <summary>
/// Default <see cref="IHolidayThemeProvider"/> implementation. Resolves the
/// active holiday from <see cref="DateTime.Now"/>. Mirrors the static
/// <c>BlueMeter.WPF.Services.HolidayThemeService</c>.
/// </summary>
public sealed class HolidayThemeService : IHolidayThemeProvider
{
    public string? GetCurrentHolidayTheme()
    {
        var now = DateTime.Now;

        // Christmas: December 1st - December 30th
        if (now.Month == 12 && now.Day is >= 1 and <= 30)
        {
            return "Christmas";
        }

        // Future holidays can be added here:
        // Halloween: October 25th - October 31st
        // New Year:  December 31st - January 2nd

        return null;
    }

    public bool IsHolidayActive() => GetCurrentHolidayTheme() != null;

    public string? GetCurrentHolidayName()
    {
        return GetCurrentHolidayTheme() switch
        {
            "Christmas" => "Christmas 🎄",
            "Halloween" => "Halloween 🎃",
            "NewYear" => "New Year 🎆",
            _ => null
        };
    }
}
