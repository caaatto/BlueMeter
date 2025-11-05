using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BlueMeter.WPF.Controls;

/// <summary>
/// Footer.xaml 的交互逻辑
/// </summary>
public partial class Footer : UserControl
{
    public static readonly DependencyProperty ConfirmCommandProperty = DependencyProperty.Register(
        nameof(ConfirmCommand), typeof(ICommand), typeof(Footer), new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(
        nameof(CancelCommand), typeof(ICommand), typeof(Footer), new PropertyMetadata(default(ICommand)));


    public Footer()
    {
        InitializeComponent();

    }

    public ICommand ConfirmCommand
    {
        get => (ICommand)GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    public ICommand CancelCommand
    {
        get => (ICommand)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    // Note: Footer uses fixed bottom corner radius via XAML style.

    public event RoutedEventHandler? ConfirmClick;
    public event RoutedEventHandler? CancelClick;

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        ConfirmClick?.Invoke(sender, e);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        CancelClick?.Invoke(sender, e);
    }
}
