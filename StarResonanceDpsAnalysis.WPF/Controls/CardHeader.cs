using System.Windows;
using System.Windows.Controls;

namespace StarResonanceDpsAnalysis.WPF.Controls;

/// <summary>
/// Represents a header control for cards, containing a title and subtitle.
/// </summary>
public class CardHeader : Control
{
    static CardHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CardHeader), new FrameworkPropertyMetadata(typeof(CardHeader)));
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(object), typeof(CardHeader), new FrameworkPropertyMetadata(null, OnTitleChanged));

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardHeader control)
        {
            control.OnTitleChanged(e.OldValue, e.NewValue);
        }
    }

    /// <summary>
    /// Called when the Title property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the Title property.</param>
    /// <param name="newValue">The new value of the Title property.</param>
    protected virtual void OnTitleChanged(object? oldValue, object? newValue)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Gets or sets the title of the card header.
    /// </summary>
    public object Title
    {
        get { return GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
        nameof(Subtitle), typeof(object), typeof(CardHeader), new FrameworkPropertyMetadata(null, OnSubtitleChanged));

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardHeader control)
        {
            control.OnSubtitleChanged(e.OldValue, e.NewValue);
        }
    }

    /// <summary>
    /// Called when the Subtitle property changes.
    /// </summary>
    /// <param name="oldValue">The old value of the Subtitle property.</param>
    /// <param name="newValue">The new value of the Subtitle property.</param>
    protected virtual void OnSubtitleChanged(object? oldValue, object? newValue)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Gets or sets the subtitle of the card header.
    /// </summary>
    public object Subtitle
    {
        get { return GetValue(SubtitleProperty); }
        set { SetValue(SubtitleProperty, value); }
    }
}