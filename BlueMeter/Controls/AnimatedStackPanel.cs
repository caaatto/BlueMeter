using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace BlueMeter.Controls;

/// <summary>
/// A vertical stack panel that animates its children when their position changes between
/// arrange passes (e.g. after sorting).
/// </summary>
public class AnimatedStackPanel : Panel
{
    private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(300);

    private readonly Dictionary<Control, Rect> _previousBounds = new();

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = new Size();
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            size = new Size(
                Math.Max(size.Width, child.DesiredSize.Width),
                size.Height + child.DesiredSize.Height);
        }

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double y = 0;
        foreach (var child in Children)
        {
            var newBounds = new Rect(0, y, finalSize.Width, child.DesiredSize.Height);

            // Animate if the item is already being tracked and has moved
            if (_previousBounds.TryGetValue(child, out var oldBounds) && oldBounds != newBounds)
            {
                var startOffset = oldBounds.Y - newBounds.Y;
                _ = AnimateChildAsync(child, startOffset);
            }

            child.Arrange(newBounds);
            y += child.DesiredSize.Height;
            _previousBounds[child] = newBounds;
        }

        return finalSize;
    }

    private static async Task AnimateChildAsync(Control child, double startOffset)
    {
        // Avalonia uses RenderTransform on the control directly. Wrap in a TranslateTransform
        // (or reuse one if already present) and animate its Y from `startOffset` to 0.
        if (child.RenderTransform is not TranslateTransform translate)
        {
            translate = new TranslateTransform();
            child.RenderTransform = translate;
        }

        translate.Y = startOffset;

        var animation = new Animation
        {
            Duration = AnimationDuration,
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(TranslateTransform.YProperty, startOffset) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(TranslateTransform.YProperty, 0d) }
                }
            }
        };

        await animation.RunAsync(translate);
        translate.Y = 0;
    }
}
