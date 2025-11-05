using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BlueMeter.WPF.Helpers;

/// <summary>
/// Attached properties that add placeholder text support to <see cref="TextBox"/> controls.
/// </summary>
public static class TextBoxHelper
{
    public static string? GetPlaceholder(DependencyObject obj) =>
        (string?)obj.GetValue(PlaceholderProperty);

    public static void SetPlaceholder(DependencyObject obj, string? value) =>
        obj.SetValue(PlaceholderProperty, value);

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.RegisterAttached(
            "Placeholder",
            typeof(string),
            typeof(TextBoxHelper),
            new FrameworkPropertyMetadata(
                defaultValue: null,
                propertyChangedCallback: OnPlaceholderChanged));

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBoxControl)
            return;

        if (!textBoxControl.IsLoaded)
        {
            textBoxControl.Loaded -= TextBoxControl_Loaded;
            textBoxControl.Loaded += TextBoxControl_Loaded;
        }
        else if (GetOrCreateAdorner(textBoxControl, out var loadedAdorner) &&
                 loadedAdorner is not null)
        {
            loadedAdorner.InvalidateVisual();
            UpdatePlaceholderVisibility(textBoxControl, loadedAdorner);
        }

        textBoxControl.TextChanged -= TextBoxControl_TextChanged;
        textBoxControl.TextChanged += TextBoxControl_TextChanged;
    }

    private static void TextBoxControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBoxControl)
            return;

        textBoxControl.Loaded -= TextBoxControl_Loaded;

        if (GetOrCreateAdorner(textBoxControl, out var adorner) &&
            adorner is not null)
        {
            adorner.InvalidateVisual();
            UpdatePlaceholderVisibility(textBoxControl, adorner);
        }
    }

    private static void TextBoxControl_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBoxControl &&
            GetOrCreateAdorner(textBoxControl, out var adorner) &&
            adorner is not null)
        {
            UpdatePlaceholderVisibility(textBoxControl, adorner);
        }
    }

    private static bool GetOrCreateAdorner(TextBox textBoxControl, out PlaceholderAdorner? adorner)
    {
        AdornerLayer? layer = AdornerLayer.GetAdornerLayer(textBoxControl);

        if (layer is null)
        {
            adorner = null;
            return false;
        }

        adorner = layer.GetAdorners(textBoxControl)?.OfType<PlaceholderAdorner>().FirstOrDefault();

        if (adorner is null)
        {
            adorner = new PlaceholderAdorner(textBoxControl);
            layer.Add(adorner);
        }

        return true;
    }

    private static void UpdatePlaceholderVisibility(TextBox textBoxControl, PlaceholderAdorner? adorner)
    {
        if (adorner is null)
            return;

        if (string.IsNullOrEmpty(GetPlaceholder(textBoxControl)))
        {
            adorner.Visibility = Visibility.Collapsed;
            return;
        }

        adorner.Visibility = string.IsNullOrEmpty(textBoxControl.Text)
            ? Visibility.Visible
            : Visibility.Hidden;
    }

    public sealed class PlaceholderAdorner : Adorner
    {
        public PlaceholderAdorner(TextBox textBox)
            : base(textBox)
        {
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Visibility != Visibility.Visible)
                return;

            if (AdornedElement is not TextBox textBoxControl)
                return;

            string? placeholderValue = GetPlaceholder(textBoxControl);

            if (string.IsNullOrEmpty(placeholderValue))
                return;

            var formattedText = new FormattedText(
                placeholderValue,
                CultureInfo.CurrentCulture,
                textBoxControl.FlowDirection,
                new Typeface(
                    textBoxControl.FontFamily,
                    textBoxControl.FontStyle,
                    textBoxControl.FontWeight,
                    textBoxControl.FontStretch),
                textBoxControl.FontSize,
                SystemColors.InactiveCaptionBrush,
                VisualTreeHelper.GetDpi(textBoxControl).PixelsPerDip);

            formattedText.TextAlignment = textBoxControl.TextAlignment;

            Rect contentRect;

            if (textBoxControl.Template.FindName("PART_ContentHost", textBoxControl) is FrameworkElement contentHost)
            {
                Point hostTopLeft = contentHost.TransformToAncestor(textBoxControl).Transform(new Point(0, 0));
                contentRect = new Rect(
                    hostTopLeft.X + textBoxControl.Padding.Left,
                    hostTopLeft.Y + textBoxControl.Padding.Top,
                    Math.Max(0, contentHost.ActualWidth - textBoxControl.Padding.Left - textBoxControl.Padding.Right),
                    Math.Max(0, contentHost.ActualHeight - textBoxControl.Padding.Top - textBoxControl.Padding.Bottom));
            }
            else
            {
                contentRect = new Rect(
                    textBoxControl.Padding.Left,
                    textBoxControl.Padding.Top,
                    Math.Max(0, textBoxControl.ActualWidth - textBoxControl.Padding.Left - textBoxControl.Padding.Right),
                    Math.Max(0, textBoxControl.ActualHeight - textBoxControl.Padding.Top - textBoxControl.Padding.Bottom));
            }

            formattedText.MaxTextWidth = Math.Max(contentRect.Width, 10);
            formattedText.MaxTextHeight = Math.Max(contentRect.Height, 10);

            Point drawPoint = new(contentRect.X, contentRect.Y);

            double extraHeight = contentRect.Height - formattedText.Height;
            if (extraHeight > 0)
            {
                switch (textBoxControl.VerticalContentAlignment)
                {
                    case VerticalAlignment.Center:
                        drawPoint.Y += extraHeight / 2d;
                        break;
                    case VerticalAlignment.Bottom:
                        drawPoint.Y += extraHeight;
                        break;
                }
            }

            double extraWidth = contentRect.Width - formattedText.Width;
            if (extraWidth > 0)
            {
                switch (textBoxControl.HorizontalContentAlignment)
                {
                    case HorizontalAlignment.Center:
                        drawPoint.X += extraWidth / 2d;
                        break;
                    case HorizontalAlignment.Right:
                        drawPoint.X += extraWidth;
                        break;
                }
            }

            drawingContext.DrawText(formattedText, drawPoint);
        }
    }
}
