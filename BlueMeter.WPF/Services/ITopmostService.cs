using System.Windows;

namespace BlueMeter.WPF.Services;

/// <summary>
/// Provides an API to toggle the Topmost state for a WPF <see cref="Window"/>.
/// </summary>
public interface ITopmostService
{
    /// <summary>
    /// Sets the Topmost flag for the specified window. Safe to call before handle creation.
    /// </summary>
    /// <param name="window">Target window</param>
    /// <param name="enable">true to set topmost; false to clear</param>
    void SetTopmost(Window window, bool enable);

    /// <summary>
    /// Toggles the Topmost state of the specified window. Returns the new state.
    /// </summary>
    /// <param name="window">Target window</param>
    /// <returns>The new Topmost state.</returns>
    bool ToggleTopmost(Window window);
}
