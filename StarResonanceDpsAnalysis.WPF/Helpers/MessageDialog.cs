using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StarResonanceDpsAnalysis.WPF.Services;

namespace StarResonanceDpsAnalysis.WPF.Helpers;

public static class MessageDialog
{
    public static bool? Show(string title, string content, Window? owner = null)
    {
        var provider = App.Host?.Services ?? throw new InvalidOperationException("App host not initialized");
        var svc = provider.GetRequiredService<IMessageDialogService>();
        return svc.Show(title, content, owner);
    }
}

