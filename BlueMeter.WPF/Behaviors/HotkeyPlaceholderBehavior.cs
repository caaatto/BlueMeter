using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace BlueMeter.WPF.Behaviors;

/// <summary>
/// Behavior to show "Press key combination..." placeholder when TextBox gets focus
/// </summary>
public class HotkeyPlaceholderBehavior : Behavior<TextBox>
{
    private const string PlaceholderText = "Press key combination...";
    private bool _isShowingPlaceholder;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.LostFocus -= OnLostFocus;
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        }
        base.OnDetaching();
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject == null) return;

        // Show placeholder immediately when field gets focus
        _isShowingPlaceholder = true;
        AssociatedObject.Text = PlaceholderText;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isShowingPlaceholder) return;

        // Clear placeholder before the KeyDownCommand fires
        _isShowingPlaceholder = false;
        AssociatedObject.Text = string.Empty;

        // Don't handle the event - let it bubble to KeyDownCommandBehavior
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject == null) return;

        _isShowingPlaceholder = false;

        // Force binding to update from config when losing focus
        AssociatedObject.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
    }
}
