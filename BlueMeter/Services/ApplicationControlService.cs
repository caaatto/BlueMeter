using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace BlueMeter.Services;

public class ApplicationControlService : IApplicationControlService
{
    public void Shutdown()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
