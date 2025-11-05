using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BlueMeter.WPF.Controls;

public class AnimatedStackPanel : Panel
{
    private readonly Dictionary<UIElement, Rect> _previousBounds = new();

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = new Size();
        foreach (UIElement child in Children)
        {
            child.Measure(availableSize);
            size.Height += child.DesiredSize.Height;
            size.Width = Math.Max(size.Width, child.DesiredSize.Width);
        }

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double y = 0;
        foreach (UIElement child in Children)
        {
            var newBounds = new Rect(0, y, finalSize.Width, child.DesiredSize.Height);

            // Animate if the item is already being tracked and has moved
            if (_previousBounds.ContainsKey(child))
            {
                var oldBounds = _previousBounds[child];
                if (oldBounds != newBounds)
                {
                    var translateTransform = new TranslateTransform();
                    child.RenderTransform = translateTransform;

                    var animation = new DoubleAnimation(
                        oldBounds.Y - newBounds.Y, 0, new Duration(TimeSpan.FromSeconds(0.3)));
                    translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);
                }
            }

            child.Arrange(newBounds);
            y += child.DesiredSize.Height;
            _previousBounds[child] = newBounds;
        }

        return finalSize;
    }
}