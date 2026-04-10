using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using BlueMeter.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMeter.Services;

/// <summary>
/// Avalonia implementation of <see cref="IMessageDialogService"/>.
///
/// The WPF version exposed a single <c>Show(title, content, owner)</c> returning
/// <c>bool?</c>. The Avalonia contract splits this into four semantic methods —
/// Information / Warning / Error / Confirmation — but all four funnel through a
/// shared <see cref="MessageView.ShowDialog{bool}(Window)"/> call today. Visual
/// differentiation (severity icons / colored headers) can be layered on later by
/// extending <see cref="MessageViewModel"/>.
/// </summary>
public class MessageDialogService : IMessageDialogService
{
    public Task ShowInformationAsync(string title, string message) =>
        ShowCoreAsync(title, message);

    public Task ShowWarningAsync(string title, string message) =>
        ShowCoreAsync(title, message);

    public Task ShowErrorAsync(string title, string message) =>
        ShowCoreAsync(title, message);

    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var result = await ShowCoreAsync(title, message);
        return result;
    }

    private static async Task<bool> ShowCoreAsync(string title, string message)
    {
        // Dialogs must be shown on the UI thread. Marshal if called from background.
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() => ShowCoreAsync(title, message));
        }

        var view = new MessageView(title, message);
        var owner = FindOwnerWindow(view);

        if (owner is not null)
        {
            return await view.ShowDialog<bool>(owner);
        }

        // No owner window available (e.g. during startup before MainWindow is set).
        // Avalonia requires an owner for ShowDialog; fall back to a modeless Show and
        // await Close via a TaskCompletionSource.
        var tcs = new TaskCompletionSource<bool>();
        view.Closed += (_, _) => tcs.TrySetResult(false);
        view.Show();
        return await tcs.Task;
    }

    private static Window? FindOwnerWindow(Window excluding)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return null;
        }

        // Prefer the currently-active window (matches the WPF preference), falling
        // back to MainWindow if nothing is active.
        var active = desktop.Windows.FirstOrDefault(w => w.IsActive && w != excluding);
        if (active is not null)
        {
            return active;
        }

        return desktop.MainWindow != excluding ? desktop.MainWindow : null;
    }
}

public static class MessageDialogServiceExtensions
{
    public static IServiceCollection AddMessageDialogService(this IServiceCollection services)
    {
        services.AddSingleton<IMessageDialogService, MessageDialogService>();
        return services;
    }
}
