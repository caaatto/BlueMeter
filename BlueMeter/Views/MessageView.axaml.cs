using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Modal message dialog with a title, body text and OK / Cancel footer.
///
/// Port notes: WPF's <c>DialogResult</c> setter is gone. To return a result
/// from a modal dialog in Avalonia, call <see cref="Window.Close(object?)"/>
/// with the result and await <c>ShowDialog&lt;bool&gt;()</c> on the caller side.
/// </summary>
public partial class MessageView : Window
{
    public MessageView()
    {
        InitializeComponent();
    }

    public MessageView(string title, string content)
        : this()
    {
        DataContext = new MessageViewModel
        {
            Title = title,
            Content = content,
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
