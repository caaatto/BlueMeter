namespace BlueMeter.ViewModels;

/// <summary>
/// Player selection item for dropdowns (chart focus, replay filter, …).
/// </summary>
public class PlayerSelectionItem
{
    public long PlayerId { get; init; }
    public string PlayerName { get; init; } = string.Empty;

    public override string ToString() => PlayerName;
}
