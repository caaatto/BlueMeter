using System.Windows;

namespace BlueMeter.WPF.Services;

public class ApplicationControlService : IApplicationControlService
{
    public void Shutdown()
    {
        Application.Current.Shutdown();
    }
}