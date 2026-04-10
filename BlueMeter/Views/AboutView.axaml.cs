using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace BlueMeter.Views;

/// <summary>
/// Modal "About" dialog showing app version, contributors and project links.
///
/// Port notes:
/// - WPF used <c>&lt;Hyperlink&gt;</c> inside <c>TextBlock.Inlines</c>; Avalonia
///   has no inline Hyperlink, so the markup uses <see cref="HyperlinkButton"/>
///   wrapped in <c>InlineUIContainer</c>. <c>HyperlinkButton</c> handles the
///   shell launch itself, so the WPF <c>Hyperlink_RequestNavigate</c> handler
///   is gone.
/// - The Footer's OK/Cancel buttons just close the window — they were already
///   non-modal in WPF (no DialogResult), so no <c>Close(result)</c> here.
/// </summary>
public partial class AboutView : Window
{
    public AboutView()
    {
        InitializeComponent();
    }

    public static string Version
    {
        get
        {
            var v = Assembly
                .GetEntryAssembly()
                ?.GetCustomAttribute<AssemblyFileVersionAttribute>()?
                .Version ?? "-.-.-";
            return $"v{v.Split('+')[0]}";
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
