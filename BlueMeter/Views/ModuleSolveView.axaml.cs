using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Module solver tool window.
///
/// Port notes (WPF → Avalonia):
///   - <c>WindowChrome</c> + <c>Style="{StaticResource TransparentWindow}"</c>
///     replaced by the chromeless quartet on the <see cref="Window"/> element.
///   - <c>PreviewKeyDown</c> Escape-to-close becomes a tunneling
///     <c>AddHandler(KeyDownEvent, ..., RoutingStrategies.Tunnel)</c> in the ctor.
///   - The Footer's <c>CancelClick</c>/<c>ConfirmClick</c> events are wired in
///     XAML (the Phase 7 batch 2 Footer port already exposes them).
///   - <c>vm.Dispose()</c> in <see cref="OnClosed"/> tears down the
///     <see cref="Services.ModuleSolver.PacketCaptureService"/> + OCR capture
///     timer/service so they don't keep running after the window closes.
/// </summary>
public partial class ModuleSolveView : Window
{
    private ModuleSolveViewModel? _vm;

    public ModuleSolveView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnWindowKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }

    public ModuleSolveView(ModuleSolveViewModel viewModel) : this()
    {
        _vm = viewModel;
        DataContext = viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    private void Footer_ConfirmClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Footer_CancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is ModuleSolveViewModel vm)
        {
            vm.Dispose();
        }
        base.OnClosed(e);
    }
}
