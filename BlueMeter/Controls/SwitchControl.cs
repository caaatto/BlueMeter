using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;

namespace BlueMeter.Controls;

public class SwitchControl : ToggleButton
{
    public static readonly StyledProperty<object?> OnContentProperty =
        AvaloniaProperty.Register<SwitchControl, object?>(nameof(OnContent), "On");

    public static readonly StyledProperty<object?> OffContentProperty =
        AvaloniaProperty.Register<SwitchControl, object?>(nameof(OffContent), "Off");

    private Border? _track;
    private Border? _thumb;
    private Border? _onBorder;
    private Border? _offBorder;
    private ContentControl? _onLabel;
    private ContentControl? _offLabel;
    private TranslateTransform? _thumbTranslate;
    private TranslateTransform? _onBorderTranslate;
    private TranslateTransform? _offBorderTranslate;

    public object? OnContent
    {
        get => GetValue(OnContentProperty);
        set => SetValue(OnContentProperty, value);
    }

    public object? OffContent
    {
        get => GetValue(OffContentProperty);
        set => SetValue(OffContentProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachTemplateHandlers();

        _track = e.NameScope.Find<Border>("PART_Track");
        _thumb = e.NameScope.Find<Border>("PART_Thumb");
        _onBorder = e.NameScope.Find<Border>("PART_OnBorder");
        _offBorder = e.NameScope.Find<Border>("PART_OffBorder");
        _onLabel = e.NameScope.Find<ContentControl>("PART_OnLabel");
        _offLabel = e.NameScope.Find<ContentControl>("PART_OffLabel");

        _thumbTranslate = EnsureTranslateTransform(_thumb);
        _onBorderTranslate = EnsureTranslateTransform(_onBorder);
        _offBorderTranslate = EnsureTranslateTransform(_offBorder);

        if (_thumb is not null)
        {
            _thumb.SizeChanged += OnTemplatePartSizeChanged;
        }

        if (_track is not null)
        {
            _track.SizeChanged += OnTemplatePartSizeChanged;
        }

        if (_onLabel is not null)
        {
            _onLabel.SizeChanged += OnLabelSizeChanged;
        }

        if (_offLabel is not null)
        {
            _offLabel.SizeChanged += OnLabelSizeChanged;
        }

        Loaded -= OnLoaded;
        Loaded += OnLoaded;
        UpdateThumbPosition(animate: false);
    }

    protected override void OnIsCheckedChanged(RoutedEventArgs e)
    {
        base.OnIsCheckedChanged(e);
        UpdateThumbPosition(animate: true);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        UpdateThumbPosition(animate: false);
    }

    private void OnTemplatePartSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateThumbPosition(animate: false);
    }

    private void OnLabelSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateThumbPosition(animate: false);
    }

    private void DetachTemplateHandlers()
    {
        if (_thumb is not null)
        {
            _thumb.SizeChanged -= OnTemplatePartSizeChanged;
        }

        if (_track is not null)
        {
            _track.SizeChanged -= OnTemplatePartSizeChanged;
        }

        if (_onLabel is not null)
        {
            _onLabel.SizeChanged -= OnLabelSizeChanged;
        }

        if (_offLabel is not null)
        {
            _offLabel.SizeChanged -= OnLabelSizeChanged;
        }
    }

    private void UpdateThumbPosition(bool animate)
    {
        UpdateLabelTransforms(animate);

        if (_thumbTranslate is null)
        {
            return;
        }

        var target = IsChecked == true ? CalculateCheckedOffset() : 0d;

        if (!animate)
        {
            _thumbTranslate.X = target;
            return;
        }

        _ = AnimateTranslateAsync(_thumbTranslate, target, TimeSpan.FromSeconds(0.2));
    }

    private void UpdateLabelTransforms(bool animate)
    {
        var onFontSize = _onLabel?.FontSize ?? FontSize;
        var offFontSize = _offLabel?.FontSize ?? FontSize;

        var onTarget = IsChecked == true ? 0d : -onFontSize;
        var offTarget = IsChecked == true ? offFontSize : 0d;

        ApplyLabelTranslate(_onBorderTranslate, onTarget, animate);
        ApplyLabelTranslate(_offBorderTranslate, offTarget, animate);
    }

    private double CalculateCheckedOffset()
    {
        if (_track is null || _thumb is null)
        {
            return 0d;
        }

        var trackWidth = _track.Bounds.Width;
        var thumbWidth = _thumb.Bounds.Width;
        var margin = _thumb.Margin;

        var offset = trackWidth - thumbWidth - margin.Left - margin.Right;
        return offset > 0 ? offset : 0d;
    }

    private static TranslateTransform? EnsureTranslateTransform(Visual? visual)
    {
        if (visual is null)
        {
            return null;
        }

        if (visual.RenderTransform is TranslateTransform existing)
        {
            return existing;
        }

        var translate = new TranslateTransform();
        visual.RenderTransform = translate;
        return translate;
    }

    private static void ApplyLabelTranslate(TranslateTransform? transform, double target, bool animate)
    {
        if (transform is null)
        {
            return;
        }

        if (!animate)
        {
            transform.X = target;
            return;
        }

        _ = AnimateTranslateAsync(transform, target, TimeSpan.FromMilliseconds(150));
    }

    private static async Task AnimateTranslateAsync(TranslateTransform transform, double target, TimeSpan duration)
    {
        var from = transform.X;
        if (Math.Abs(from - target) < 0.01)
        {
            transform.X = target;
            return;
        }

        var animation = new Animation
        {
            Duration = duration,
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(TranslateTransform.XProperty, from) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(TranslateTransform.XProperty, target) }
                }
            }
        };

        await animation.RunAsync(transform);
        transform.X = target;
    }
}
