namespace BlueMeter.Services;

/// <summary>
/// Service for showing file open/save dialogs.
/// The Avalonia implementation (using IStorageProvider) lives in Phase 10.
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// Prompt the user to pick a file to open. Returns the selected path or null.
    /// </summary>
    /// <param name="title">Window title for the dialog.</param>
    /// <param name="filters">Filters in the form (DisplayName, [extensions]) e.g. ("Capture files", new[]{"pcap","pcapng"}).</param>
    Task<string?> ShowOpenFileDialogAsync(string title, IReadOnlyList<(string DisplayName, IReadOnlyList<string> Extensions)> filters);

    /// <summary>
    /// Prompt the user to pick a destination file path. Returns the selected path or null.
    /// </summary>
    Task<string?> ShowSaveFileDialogAsync(string title, string suggestedFileName, IReadOnlyList<(string DisplayName, IReadOnlyList<string> Extensions)> filters);
}
