using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace BlueMeter.Controls;

public class DpsIndicatorControl : TemplatedControl
{
    // Percentage value in range 0..Maximum. The template binds PART_Indicator.Width
    // to this through PercentToWidthConverter; a Transitions block on PART_Indicator
    // tweens the resulting width change.
    public static readonly StyledProperty<double> PercentageProperty =
        AvaloniaProperty.Register<DpsIndicatorControl, double>(
            nameof(Percentage),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<DpsIndicatorControl, double>(nameof(Maximum), 100d);

    public static readonly StyledProperty<IBrush?> IndicatorBackgroundProperty =
        AvaloniaProperty.Register<DpsIndicatorControl, IBrush?>(
            nameof(IndicatorBackground),
            Brushes.LightGray);

    public static readonly StyledProperty<IBrush?> IndicatorForegroundProperty =
        AvaloniaProperty.Register<DpsIndicatorControl, IBrush?>(
            nameof(IndicatorForeground),
            Brushes.DodgerBlue);

    public static readonly StyledProperty<IDataTemplate?> OverlayTemplateProperty =
        AvaloniaProperty.Register<DpsIndicatorControl, IDataTemplate?>(nameof(OverlayTemplate));

    public static readonly StyledProperty<object?> OverlayContentProperty =
        AvaloniaProperty.Register<DpsIndicatorControl, object?>(nameof(OverlayContent));

    public static readonly StyledProperty<IDataTemplate?> PopupTemplateProperty =
        AvaloniaProperty.Register<DpsIndicatorControl, IDataTemplate?>(nameof(PopupTemplate));

    public static readonly StyledProperty<object?> PopupContentProperty =
        AvaloniaProperty.Register<DpsIndicatorControl, object?>(nameof(PopupContent));

    public static readonly StyledProperty<double> TrackOpacityProperty =
        AvaloniaProperty.Register<DpsIndicatorControl, double>(nameof(TrackOpacity));

    public DpsIndicatorControl()
    {
        // CornerRadius is inherited from TemplatedControl; default it to the WPF value here
        // since the static metadata-override pattern is gone in Avalonia.
        CornerRadius = new CornerRadius(4);

        // Mouse event handlers for debugging
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
    }

    public double TrackOpacity
    {
        get => GetValue(TrackOpacityProperty);
        set => SetValue(TrackOpacityProperty, value);
    }

    public double Percentage
    {
        get => GetValue(PercentageProperty);
        set => SetValue(PercentageProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public IBrush? IndicatorBackground
    {
        get => GetValue(IndicatorBackgroundProperty);
        set => SetValue(IndicatorBackgroundProperty, value);
    }

    public IBrush? IndicatorForeground
    {
        get => GetValue(IndicatorForegroundProperty);
        set => SetValue(IndicatorForegroundProperty, value);
    }

    public IDataTemplate? OverlayTemplate
    {
        get => GetValue(OverlayTemplateProperty);
        set => SetValue(OverlayTemplateProperty, value);
    }

    public object? OverlayContent
    {
        get => GetValue(OverlayContentProperty);
        set => SetValue(OverlayContentProperty, value);
    }

    public IDataTemplate? PopupTemplate
    {
        get => GetValue(PopupTemplateProperty);
        set => SetValue(PopupTemplateProperty, value);
    }

    public object? PopupContent
    {
        get => GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PopupTemplateProperty)
        {
            Debug.WriteLine($"[DpsIndicatorControl] PopupTemplate changed: {change.NewValue?.GetType().Name ?? "null"}");
        }
        else if (change.Property == PopupContentProperty)
        {
            var oldPlayerName = (change.OldValue as dynamic)?.Player?.Name ?? "null";
            var newPlayerName = (change.NewValue as dynamic)?.Player?.Name ?? "null";
            Debug.WriteLine($"[DpsIndicatorControl] PopupContent changed: {oldPlayerName} -> {newPlayerName}");
        }
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        Debug.WriteLine(
            $"[DpsIndicatorControl] PointerEntered - PopupContent: {PopupContent?.GetType().Name ?? "null"}");
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        Debug.WriteLine("[DpsIndicatorControl] PointerExited");
    }
}
