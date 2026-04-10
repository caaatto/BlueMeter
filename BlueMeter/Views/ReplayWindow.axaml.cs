using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Combat replay window with playback controls for recorded battle logs.
/// </summary>
public partial class ReplayWindow : Window
{
    private readonly ReplayWindowViewModel? _viewModel;

    public ReplayWindow()
    {
        InitializeComponent();
    }

    public ReplayWindow(ReplayWindowViewModel viewModel)
        : this()
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        _viewModel.RequestClose += OnRequestClose;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnRequestClose()
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (_viewModel is not null)
        {
            _viewModel.RequestClose -= OnRequestClose;
        }
    }
}
