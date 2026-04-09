namespace BlueMeter.Services;

/// <summary>
/// Service for showing user-facing dialogs.
/// The Avalonia implementation lives in Phase 10 (alongside the Views).
/// </summary>
public interface IMessageDialogService
{
    /// <summary>
    /// Show an informational dialog with an OK button.
    /// </summary>
    Task ShowInformationAsync(string title, string message);

    /// <summary>
    /// Show a warning dialog with an OK button.
    /// </summary>
    Task ShowWarningAsync(string title, string message);

    /// <summary>
    /// Show an error dialog with an OK button.
    /// </summary>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Show a confirmation dialog. Returns true if the user confirmed.
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);
}
