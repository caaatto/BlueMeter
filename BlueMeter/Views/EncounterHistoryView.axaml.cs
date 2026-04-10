using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

public partial class EncounterHistoryView : Window
{
    public EncounterHistoryView()
    {
        InitializeComponent();
        Opened += OnOpened;
    }

    public EncounterHistoryView(EncounterHistoryViewModel viewModel)
        : this()
    {
        DataContext = viewModel;
        viewModel.RequestClose += Close;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        var grid = this.FindControl<DataGrid>("EncounterDataGrid");
        if (grid is not null)
        {
            grid.DoubleTapped += OnGridDoubleTapped;
        }
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (DataContext is EncounterHistoryViewModel vm && vm.LoadedCommand.CanExecute(null))
        {
            vm.LoadedCommand.Execute(null);
        }
    }

    private void OnGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is EncounterHistoryViewModel vm && vm.LoadSelectedEncounterCommand.CanExecute(null))
        {
            vm.LoadSelectedEncounterCommand.Execute(null);
        }
    }
}
