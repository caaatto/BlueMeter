using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using BlueMeter.Services.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMeter.Controls;

/// <summary>
/// Christmas decorations specific to the DPS meter window: festive background
/// layer, frost border, and periodic sparkle effects.
///
/// Port notes: same approach as <see cref="ChristmasDecorations"/> — sparkle
/// animations target a per-image <see cref="ScaleTransform"/> plus the image's
/// Opacity via Avalonia's <see cref="Animation"/> API instead of WPF
/// Storyboards.
/// </summary>
public partial class DpsMeterChristmasDecorations : UserControl
{
    private readonly Random _random = new();
    private DispatcherTimer? _sparkleTimer;
    private Canvas? _sparkleCanvas;

    public DpsMeterChristmasDecorations()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttached;
        DetachedFromVisualTree += OnDetached;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _sparkleCanvas = this.FindControl<Canvas>("SparkleCanvas");
    }

    private void OnAttached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (IsHolidayActive())
        {
            StartSparkles();
        }
    }

    private void OnDetached(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        StopSparkles();
    }

    private static bool IsHolidayActive()
    {
        var provider = App.Host?.Services.GetService<IHolidayThemeProvider>();
        return provider?.IsHolidayActive() ?? false;
    }

    private void StartSparkles()
    {
        _sparkleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _sparkleTimer.Tick += (s, e) => CreateSparkle();
        _sparkleTimer.Start();
    }

    private void StopSparkles()
    {
        _sparkleTimer?.Stop();
        _sparkleTimer = null;
    }

    private void CreateSparkle()
    {
        if (_sparkleCanvas is null)
        {
            return;
        }

        try
        {
            var uri = new Uri("avares://BlueMeter/Assets/Themes/Christmas/Effects/sparkle.png");
            using var stream = AssetLoader.Open(uri);
            var bitmap = new Bitmap(stream);

            var size = _random.Next(8, 16);
            var scale = new ScaleTransform(0.5, 0.5);
            var sparkle = new Image
            {
                Source = bitmap,
                Width = size,
                Height = size,
                Opacity = 0,
                RenderTransform = scale,
                RenderTransformOrigin = RelativePoint.Center
            };

            var left = _random.Next(0, (int)Math.Max(1, Bounds.Width));
            var top = _random.Next(0, (int)Math.Max(1, Bounds.Height));
            Canvas.SetLeft(sparkle, left);
            Canvas.SetTop(sparkle, top);
            _sparkleCanvas.Children.Add(sparkle);

            var duration = TimeSpan.FromMilliseconds(600);

            var opacityAnimation = new Animation
            {
                Duration = duration,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(Visual.OpacityProperty, 0d) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(0.5d),
                        Setters = { new Setter(Visual.OpacityProperty, 0.8) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(Visual.OpacityProperty, 0d) }
                    }
                }
            };

            var scaleAnimation = new Animation
            {
                Duration = duration,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, 0.5),
                            new Setter(ScaleTransform.ScaleYProperty, 0.5)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(0.5d),
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, 1.5),
                            new Setter(ScaleTransform.ScaleYProperty, 1.5)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(ScaleTransform.ScaleXProperty, 0.5),
                            new Setter(ScaleTransform.ScaleYProperty, 0.5)
                        }
                    }
                }
            };

            _ = RunAndRemoveAsync(opacityAnimation, scaleAnimation, sparkle, scale, _sparkleCanvas);
        }
        catch
        {
            // Ignore — asset missing or control torn down.
        }
    }

    private static async System.Threading.Tasks.Task RunAndRemoveAsync(
        Animation opacity,
        Animation scale,
        Image sparkle,
        ScaleTransform scaleTarget,
        Canvas canvas)
    {
        var opacityTask = opacity.RunAsync(sparkle);
        var scaleTask = scale.RunAsync(scaleTarget);
        await System.Threading.Tasks.Task.WhenAll(opacityTask, scaleTask);
        canvas.Children.Remove(sparkle);
    }
}
