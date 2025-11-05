using System.Windows;

namespace StarResonanceDpsAnalysis.WPF.Services;

public class ApplicationControlService : IApplicationControlService
{
    public void Shutdown()
    {
        Application.Current.Shutdown();
    }
}