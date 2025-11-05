using System.Windows;

namespace StarResonanceDpsAnalysis.WPF.Services;

public interface ITrayService
{
    void Initialize(string? toolTip = null);
    void MinimizeToTray();
    void Restore();
    void Exit();
}
