using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using BlueMeter.WPF.ViewModels;

namespace BlueMeter.WPF.Views;

/// <summary>
///     SettingForm.xaml 的交互逻辑
/// </summary>
public partial class SettingsView : Window
{
    private readonly SettingsViewModel _vm;

    public SettingsView(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        _vm = vm;
        _vm.RequestClose += Vm_RequestClose;
    }

    private void Vm_RequestClose()
    {
        Close();
    }

    // FIX: Theme color button click handler - allows users to change window theme color
    // Updates the AppConfig property which triggers PropertyChanged notification
    private void ThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Background is SolidColorBrush brush)
        {
            // Update the property - this will trigger PropertyChanged through ObservableProperty
            _vm.AppConfig.ThemeColor = brush.Color.ToString();

            // Force property change notification to ensure UI updates
            // This is needed because AppConfig is not directly observable from MainViewModel
            System.Windows.Data.BindingOperations.GetBindingExpression(
                button, Button.BackgroundProperty)?.UpdateSource();
        }
    }

    // FIX: Background image selection handler - opens file dialog to select image
    private void SelectBackgroundImage_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All files (*.*)|*.*",
            Title = "Select Background Image"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _vm.AppConfig.BackgroundImagePath = openFileDialog.FileName;
        }
    }

    private void Footer_ConfirmClick(object sender, RoutedEventArgs e) { /* handled by VM command */ }

    private void Footer_CancelClick(object sender, RoutedEventArgs e) { /* handled by VM command */ }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    private void ScrollToSection(FrameworkElement? target)
    {
        if (target == null) return;

        if (ContentScrollViewer?.Content is FrameworkElement content)
        {
            var p = target.TransformToVisual(content).Transform(new Point(0, 0));
            var y = Math.Max(p.Y, 0);
            ContentScrollViewer.ScrollToVerticalOffset(y);
        }
        else
        {
            target.BringIntoView();
        }
    }

    private void Nav_Language_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionLanguage);
    private void Nav_Basic_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionBasic);
    private void Nav_Shortcut_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionShortcut);
    private void Nav_Combat_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionCombat);
    private void Nav_Theme_Click(object sender, RoutedEventArgs e) => ScrollToSection(SectionTheme);

    // MEMORY LEAK FIX: Dispose ViewModel when window closes to clean up event subscriptions.
    // SettingsViewModel subscribes to LocalizationManager and NetworkChange static events.
    // Without this, the ViewModel would never be garbage collected, causing memory leaks.
    protected override void OnClosed(EventArgs e)
    {
        _vm.RequestClose -= Vm_RequestClose;
        _vm.Dispose();
        base.OnClosed(e);
    }
}
