using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BlueMeter.Services.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMeter.Controls;

/// <summary>
/// Christmas decorations: falling snow in a canvas, twinkling light strip at
/// the top, and a swinging bell in the bottom-right corner.
///
/// Port notes: WPF used Storyboards targeting Canvas.Top / Opacity / RotateTransform.Angle
/// directly. Avalonia's <see cref="Animation"/> API targets properties via
/// <see cref="KeyFrame"/>s run against a specific target object, so the port
/// animates a <see cref="TranslateTransform"/> on the snowflake image (instead
/// of the Canvas.Top attached property) and a <see cref="RotateTransform"/>
/// on the bell image. The music-playback Easter egg (Carol of Bells) is
/// intentionally dropped — the Avalonia port keeps only the visual decorations
/// and the click-to-swing bell animation.
/// </summary>
public partial class ChristmasDecorations : UserControl
{
    private static readonly string[] SnowflakeImages =
    {
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_small_01.png",
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_small_02.png",
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_small_03.png",
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_small_04.png",
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_small_05.png",
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_small_06.png",
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_small_07.png",
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_small_08.png",
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_small_09.png",
        "avares://BlueMeter/Assets/Themes/Christmas/Snowflakes/snowflake_medium_01.png"
    };

    private readonly Random _random = new();
    private DispatcherTimer? _snowTimer;
    private Canvas? _snowCanvas;
    private Image? _christmasLights;
    private Image? _christmasBell;

    public ChristmasDecorations()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttached;
        DetachedFromVisualTree += OnDetached;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _snowCanvas = this.FindControl<Canvas>("SnowCanvas");
        _christmasLights = this.FindControl<Image>("ChristmasLights");
        _christmasBell = this.FindControl<Image>("ChristmasBell");
    }

    private void OnAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (IsHolidayActive())
        {
            StartSnowfall();
            StartTwinkling();
        }
    }

    private void OnDetached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        StopSnowfall();
    }

    private static bool IsHolidayActive()
    {
        // AppConfig resolves IHolidayThemeProvider through App.Host lazily; do the
        // same here rather than taking a constructor injection (UserControls are
        // instantiated by the XAML loader and can't take DI constructor args).
        var provider = App.Host?.Services.GetService<IHolidayThemeProvider>();
        return provider?.IsHolidayActive() ?? false;
    }

    private void StartSnowfall()
    {
        _snowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _snowTimer.Tick += (s, e) => CreateSnowflake();
        _snowTimer.Start();

        for (int i = 0; i < 10; i++)
        {
            CreateSnowflake();
        }
    }

    private void StopSnowfall()
    {
        _snowTimer?.Stop();
        _snowTimer = null;
    }

    private void CreateSnowflake()
    {
        // Prefer the parent window's MainSnowCanvas if present (so snow covers
        // the whole window, not just this control). Fall back to our own canvas.
        Canvas? canvas = null;
        var window = this.GetVisualRoot() as Window;
        if (window is not null)
        {
            canvas = FindCanvas(window, "MainSnowCanvas");
        }
        canvas ??= _snowCanvas;
        if (canvas is null)
        {
            return;
        }

        try
        {
            var targetHeight = window?.Bounds.Height ?? 600;
            var targetWidth = window?.Bounds.Width ?? 800;

            var uri = new Uri(SnowflakeImages[_random.Next(SnowflakeImages.Length)]);
            using var stream = AssetLoader.Open(uri);
            var bitmap = new Bitmap(stream);

            var size = _random.Next(12, 28);
            var translate = new TranslateTransform(0, 0);
            var snowflake = new Image
            {
                Source = bitmap,
                Width = size,
                Height = size,
                Opacity = 0,
                RenderTransform = translate
            };

            var left = _random.Next(0, (int)Math.Max(1, targetWidth));
            Canvas.SetLeft(snowflake, left);
            Canvas.SetTop(snowflake, -200);
            canvas.Children.Add(snowflake);

            var durationSeconds = _random.Next(10, 20);
            var duration = TimeSpan.FromSeconds(durationSeconds);

            var fallAnimation = new Animation
            {
                Duration = duration,
                Easing = new QuadraticEaseIn(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(TranslateTransform.YProperty, 0d) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(TranslateTransform.YProperty, targetHeight + 300) }
                    }
                }
            };

            var opacityAnimation = new Animation
            {
                Duration = duration,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(Visual.OpacityProperty, 0d) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(0.05d),
                        Setters = { new Setter(Visual.OpacityProperty, 0.7) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(0.9d),
                        Setters = { new Setter(Visual.OpacityProperty, 0.7) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(Visual.OpacityProperty, 0d) }
                    }
                }
            };

            _ = RunAndRemoveAsync(fallAnimation, opacityAnimation, translate, snowflake, canvas);
        }
        catch
        {
            // Ignore — control being torn down, asset missing, etc.
        }
    }

    private static async System.Threading.Tasks.Task RunAndRemoveAsync(
        Animation fall,
        Animation opacity,
        TranslateTransform translate,
        Image snowflake,
        Canvas canvas)
    {
        var fallTask = fall.RunAsync(translate);
        var opacityTask = opacity.RunAsync(snowflake);
        await System.Threading.Tasks.Task.WhenAll(fallTask, opacityTask);
        canvas.Children.Remove(snowflake);
    }

    private void StartTwinkling()
    {
        if (_christmasLights is null)
        {
            return;
        }

        var twinkle = new Animation
        {
            Duration = TimeSpan.FromSeconds(2),
            IterationCount = IterationCount.Infinite,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(Visual.OpacityProperty, 0.3) }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.25d),
                    Setters = { new Setter(Visual.OpacityProperty, 1.0) }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.5d),
                    Setters = { new Setter(Visual.OpacityProperty, 0.3) }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.75d),
                    Setters = { new Setter(Visual.OpacityProperty, 0.8) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(Visual.OpacityProperty, 0.3) }
                }
            }
        };

        _ = twinkle.RunAsync(_christmasLights);
    }

    private void OnBellPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsHolidayActive())
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        TriggerBellRing();
    }

    private void TriggerBellRing()
    {
        if (_christmasBell?.RenderTransform is not RotateTransform rotate)
        {
            return;
        }

        // 6 half-cycles of 100ms = 3 full rings, matching the WPF RepeatBehavior(3) + AutoReverse.
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(600),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(RotateTransform.AngleProperty, -15d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.167d),
                    Setters = { new Setter(RotateTransform.AngleProperty, 15d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.333d),
                    Setters = { new Setter(RotateTransform.AngleProperty, -15d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.5d),
                    Setters = { new Setter(RotateTransform.AngleProperty, 15d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.667d),
                    Setters = { new Setter(RotateTransform.AngleProperty, -15d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(0.833d),
                    Setters = { new Setter(RotateTransform.AngleProperty, 15d) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(RotateTransform.AngleProperty, 0d) }
                }
            }
        };

        _ = animation.RunAsync(rotate);
    }

    /// <summary>
    /// Walks the logical visual tree looking for a named <see cref="Canvas"/>.
    /// Avalonia has no direct <c>LogicalTreeHelper</c>/<c>VisualTreeHelper</c>
    /// equivalent with the same shape, so this is a simple recursive descent.
    /// </summary>
    private static Canvas? FindCanvas(Visual root, string name)
    {
        foreach (var child in root.GetVisualChildren())
        {
            if (child is Canvas c && c.Name == name)
            {
                return c;
            }
            if (FindCanvas(child, name) is { } found)
            {
                return found;
            }
        }
        return null;
    }
}
