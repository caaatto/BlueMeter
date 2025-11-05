using System.Windows;

namespace BlueMeter.WPF.Services;

public interface ITrayService
{
    void Initialize(string? toolTip = null);
    void MinimizeToTray();
    void Restore();
    void Exit();
}
