using BlueMeter.Config;

namespace BlueMeter.Services;

/// <summary>
/// Design-time implementation of IGlobalHotkeyService for the previewer.
/// All methods are no-ops since hotkey registration is not needed at design time.
/// </summary>
internal sealed class DesignTimeGlobalHotkeyService : IGlobalHotkeyService
{
    public void Start()
    {
    }

    public void Stop()
    {
    }

    public void UpdateFromConfig(AppConfig config)
    {
    }
}
