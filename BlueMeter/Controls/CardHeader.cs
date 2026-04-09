using Avalonia;
using Avalonia.Controls.Primitives;

namespace BlueMeter.Controls;

/// <summary>
/// Represents a header control for cards, containing a title and subtitle.
/// </summary>
public class CardHeader : TemplatedControl
{
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<CardHeader, object?>(nameof(Title));

    public static readonly StyledProperty<object?> SubtitleProperty =
        AvaloniaProperty.Register<CardHeader, object?>(nameof(Subtitle));

    /// <summary>
    /// Gets or sets the title of the card header.
    /// </summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the subtitle of the card header.
    /// </summary>
    public object? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TitleProperty)
        {
            OnTitleChanged(change.OldValue, change.NewValue);
        }
        else if (change.Property == SubtitleProperty)
        {
            OnSubtitleChanged(change.OldValue, change.NewValue);
        }
    }

    /// <summary>
    /// Called when the Title property changes.
    /// </summary>
    protected virtual void OnTitleChanged(object? oldValue, object? newValue)
    {
        // Override in derived classes if needed
    }

    /// <summary>
    /// Called when the Subtitle property changes.
    /// </summary>
    protected virtual void OnSubtitleChanged(object? oldValue, object? newValue)
    {
        // Override in derived classes if needed
    }
}
