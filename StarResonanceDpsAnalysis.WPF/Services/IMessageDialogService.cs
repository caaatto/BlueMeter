using System.Windows;

namespace StarResonanceDpsAnalysis.WPF.Services;

public interface IMessageDialogService
{
    bool? Show(string title, string content, Window? owner = null);
}

