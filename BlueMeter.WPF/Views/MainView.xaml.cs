using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using BlueMeter.WPF.Themes.SystemThemes;
using BlueMeter.WPF.ViewModels;

namespace BlueMeter.WPF.Views;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
public partial class MainView : Window
{
    public MainView(MainViewModel viewModel, SystemThemeWatcher watcher)
    {
        watcher.Watch(this);
        InitializeComponent();
        DataContext = viewModel;

        Loaded += (_, _) => viewModel.InitializeTrayCommand.Execute(null);
        StateChanged += (_, _) =>
        {
            if (WindowState == WindowState.Minimized)
            {
                viewModel.MinimizeToTrayCommand.Execute(null);
            }
        };
        Closing += (s, e) =>
        {
            // default: hide instead of exit; user can Exit from tray menu
            e.Cancel = true;
            viewModel.MinimizeToTrayCommand.Execute(null);
        };
    }

    public bool IsDebugContentVisible { get; } =
#if DEBUG
        true;
#else
        false;
#endif

    private void Footer_OnConfirmClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    // MEMORY LEAK FIX: Ensure ViewModel is disposed when window is actually closed.
    // Without this, the Dispose() method we added to MainViewModel would never be called,
    // and the CultureChanged event subscription would never be cleaned up.
    // Note: This is only called on actual application shutdown, not when minimizing to tray.
    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Dispose();
        }

        base.OnClosed(e);
    }

}
