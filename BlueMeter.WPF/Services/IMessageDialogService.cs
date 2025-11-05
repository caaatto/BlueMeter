using System.Windows;

namespace BlueMeter.WPF.Services;

public interface IMessageDialogService
{
    bool? Show(string title, string content, Window? owner = null);
}

