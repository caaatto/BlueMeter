using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BlueMeter.Config;
using BlueMeter.Controls;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Settings window code-behind.
///
/// Port notes (WPF → Avalonia):
///   - Nav buttons scroll the content ScrollViewer to the corresponding section
///     marker (a <see cref="CardHeader"/>). WPF used
///     <c>target.TransformToVisual(content).Transform(new Point(0, 0))</c> +
///     <c>ScrollViewer.ScrollToVerticalOffset(y)</c>; Avalonia uses
///     <c>target.TranslatePoint(new Point(0, 0), content)</c> (nullable return)
///     and sets <c>scrollViewer.Offset = new Vector(0, y)</c>.
///   - Theme swatch buttons are built imperatively at Opened time instead of
///     via WPF's <c>FrameworkElementFactory</c> (which has no Avalonia
///     analogue).
///   - The UID-highlight flash animation is an <see cref="Animation"/> with
///     <see cref="PlaybackDirection.Alternate"/> and IterationCount=3, run via
///     <see cref="Animation.RunAsync"/> on the UID border's BorderBrush.
///   - <c>MessageBox.Show</c>-based file pickers become
///     <c>TopLevel.GetTopLevel(this)?.StorageProvider.OpenFilePickerAsync</c>
///     with <see cref="FilePickerOpenOptions"/>.
///   - <see cref="SettingsViewModel.RequestClose"/> is wired at DI-ctor time
///     and unsubscribed in <see cref="OnClosed"/>; the VM's
///     <see cref="SettingsViewModel.Dispose"/> is invoked there too so that
///     the CultureChanged / NetworkChange / AppConfig.PropertyChanged
///     subscriptions don't leak.
/// </summary>
public partial class SettingsView : Window
{
    private SettingsViewModel? _vm;

    private ScrollViewer? _contentScrollViewer;
    private WrapPanel? _themeButtonsPanel;
    private Border? _uidInputBorder;
    private TextBox? _uidTextBox;

    private CardHeader? _sectionLanguage;
    private CardHeader? _sectionBasic;
    private CardHeader? _sectionAlerts;
    private CardHeader? _sectionShortcut;
    private CardHeader? _sectionCombat;
    private CardHeader? _sectionTheme;

    public SettingsView()
    {
        InitializeComponent();

        // Escape-to-close via tunneling key handler (WPF PreviewKeyDown equivalent).
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);

        Opened += OnOpened;
    }

    public SettingsView(SettingsViewModel viewModel) : this()
    {
        _vm = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        _contentScrollViewer = this.FindControl<ScrollViewer>("ContentScrollViewer");
        _themeButtonsPanel = this.FindControl<WrapPanel>("ThemeButtonsPanel");
        _uidInputBorder = this.FindControl<Border>("UidInputBorder");
        _uidTextBox = this.FindControl<TextBox>("UidTextBox");

        _sectionLanguage = this.FindControl<CardHeader>("SectionLanguage");
        _sectionBasic = this.FindControl<CardHeader>("SectionBasic");
        _sectionAlerts = this.FindControl<CardHeader>("SectionAlerts");
        _sectionShortcut = this.FindControl<CardHeader>("SectionShortcut");
        _sectionCombat = this.FindControl<CardHeader>("SectionCombat");
        _sectionTheme = this.FindControl<CardHeader>("SectionTheme");
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        if (_vm?.LoadedCommand.CanExecute(null) == true)
        {
            _vm.LoadedCommand.Execute(null);
        }

        PopulateThemeButtons();
        InitializeActiveThemeButton();
    }

    private void OnRequestClose()
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_vm is not null)
        {
            _vm.RequestClose -= OnRequestClose;
            _vm.Dispose();
        }
        base.OnClosed(e);
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    // ------------------------------------------------------------------
    // Navigation
    // ------------------------------------------------------------------

    private void Nav_Language_Click(object? sender, RoutedEventArgs e) => ScrollToSection(_sectionLanguage);
    private void Nav_Basic_Click(object? sender, RoutedEventArgs e) => ScrollToSection(_sectionBasic);
    private void Nav_Alerts_Click(object? sender, RoutedEventArgs e) => ScrollToSection(_sectionAlerts);
    private void Nav_Shortcut_Click(object? sender, RoutedEventArgs e) => ScrollToSection(_sectionShortcut);
    private void Nav_Combat_Click(object? sender, RoutedEventArgs e) => ScrollToSection(_sectionCombat);
    private void Nav_Theme_Click(object? sender, RoutedEventArgs e) => ScrollToSection(_sectionTheme);

    private void ScrollToSection(Control? target)
    {
        if (target is null || _contentScrollViewer is null)
        {
            return;
        }

        // The scroll content is the StackPanel that hosts every section — it's the
        // direct child of the ScrollViewer. TranslatePoint(target, content) gives
        // the target's offset relative to that content.
        if (_contentScrollViewer.Content is not Control content)
        {
            return;
        }

        var point = target.TranslatePoint(new Point(0, 0), content);
        if (point.HasValue)
        {
            _contentScrollViewer.Offset = new Vector(0, Math.Max(0, point.Value.Y));
        }
    }

    // ------------------------------------------------------------------
    // Theme swatch buttons
    // ------------------------------------------------------------------

    private void PopulateThemeButtons()
    {
        if (_themeButtonsPanel is null)
        {
            return;
        }

        _themeButtonsPanel.Children.Clear();

        foreach (var theme in ThemeDefinitions.Themes)
        {
            // Christmas theme is driven by EnableHolidayThemes, not by the swatch grid.
            if (theme.Id == "Christmas")
            {
                continue;
            }

            Button button = theme.Id switch
            {
                "Transparent" => CreateTransparentButton(),
                "Rainbow" or "Sunset" or "Cyberpunk" => CreateGradientButton(theme.Id),
                _ => CreateSolidColorButton(theme.ColorHex),
            };

            button.Tag = theme.Id;
            ToolTip.SetTip(button, theme.DisplayName);
            button.Classes.Add("themeButton");
            button.Click += ThemeButton_Click;

            _themeButtonsPanel.Children.Add(button);
        }
    }

    private static Button CreateSolidColorButton(string colorHex)
    {
        var color = Color.TryParse(colorHex, out var parsed) ? parsed : Colors.CornflowerBlue;
        return new Button
        {
            Background = new SolidColorBrush(color),
        };
    }

    private static Button CreateTransparentButton()
    {
        return new Button
        {
            Background = Brushes.Transparent,
        };
    }

    private static Button CreateGradientButton(string gradientType)
    {
        // Four-cell mosaic. Matches the WPF preview: each quadrant shows one of
        // the gradient's representative colours, with corner radii on the outer
        // corners so the whole button reads as a single rounded swatch.
        (string tl, string tr, string bl, string br) = gradientType switch
        {
            "Rainbow"   => ("#FF0000", "#FFA500", "#00FF00", "#0000FF"),
            "Sunset"    => ("#FF6B6B", "#FFA500", "#FFD700", "#FF69B4"),
            "Cyberpunk" => ("#FF006E", "#00FFFF", "#39FF14", "#BF40BF"),
            _           => ("#888888", "#888888", "#888888", "#888888"),
        };

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,*"),
            ColumnDefinitions = new ColumnDefinitions("*,*"),
        };

        grid.Children.Add(MakeCell(tl, new CornerRadius(4, 0, 0, 0), 0, 0));
        grid.Children.Add(MakeCell(tr, new CornerRadius(0, 4, 0, 0), 0, 1));
        grid.Children.Add(MakeCell(bl, new CornerRadius(0, 0, 0, 4), 1, 0));
        grid.Children.Add(MakeCell(br, new CornerRadius(0, 0, 4, 0), 1, 1));

        return new Button
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            Content = grid,
        };

        static Border MakeCell(string hex, CornerRadius radius, int row, int col)
        {
            var color = Color.TryParse(hex, out var parsed) ? parsed : Colors.Gray;
            var border = new Border
            {
                Background = new SolidColorBrush(color),
                CornerRadius = radius,
            };
            Grid.SetRow(border, row);
            Grid.SetColumn(border, col);
            return border;
        }
    }

    private void InitializeActiveThemeButton()
    {
        if (_themeButtonsPanel is null || _vm is null)
        {
            return;
        }

        var activeId = _vm.AppConfig.ThemeColor;
        foreach (var child in _themeButtonsPanel.Children)
        {
            if (child is Button btn && btn.Tag is string tag)
            {
                if (string.Equals(tag, activeId, StringComparison.OrdinalIgnoreCase))
                {
                    btn.Classes.Add("active");
                }
                else
                {
                    btn.Classes.Remove("active");
                }
            }
        }
    }

    private void ThemeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button clicked || clicked.Tag is not string themeId || _vm is null)
        {
            return;
        }

        _vm.AppConfig.ThemeColor = themeId;
        UpdateActiveThemeButton(clicked);
    }

    private void UpdateActiveThemeButton(Button clicked)
    {
        if (_themeButtonsPanel is null)
        {
            return;
        }

        foreach (var child in _themeButtonsPanel.Children)
        {
            if (child is Button btn)
            {
                btn.Classes.Remove("active");
            }
        }
        clicked.Classes.Add("active");
    }

    // ------------------------------------------------------------------
    // Background image picker
    // ------------------------------------------------------------------

    private async void SelectBackgroundImage_Click(object? sender, RoutedEventArgs e)
    {
        if (_vm is null)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Background Image",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Image files")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" },
                },
                FilePickerFileTypes.All,
            },
        });

        if (files.Count == 0)
        {
            return;
        }

        var path = files[0].TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
        {
            _vm.AppConfig.BackgroundImagePath = path;
        }
    }

    // ------------------------------------------------------------------
    // Public: show + scroll to UID with a red-border flash.
    // ------------------------------------------------------------------

    /// <summary>
    /// Show the Settings window, scroll the Combat section into view, focus
    /// the Manual Player UID TextBox, and flash its border red to call
    /// attention to it. Called from <see cref="Services.WindowManagementService"/>
    /// when Solo Training is enabled without a configured UID.
    /// </summary>
    public void ShowAndHighlightUidField()
    {
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }
        Show();
        Activate();

        // Defer until the window has laid out so TranslatePoint has valid bounds.
        Dispatcher.UIThread.Post(async () =>
        {
            ScrollToSection(_sectionCombat);
            _uidTextBox?.Focus();

            if (_uidInputBorder is null)
            {
                return;
            }

            var originalBrush = _uidInputBorder.BorderBrush;

            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                IterationCount = new IterationCount(3),
                PlaybackDirection = PlaybackDirection.Alternate,
                Easing = new LinearEasing(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(Border.BorderBrushProperty, originalBrush ?? Brushes.Transparent),
                        },
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters =
                        {
                            new Setter(Border.BorderBrushProperty, Brushes.Red),
                        },
                    },
                },
            };

            await animation.RunAsync(_uidInputBorder);
            _uidInputBorder.BorderBrush = originalBrush;
        }, DispatcherPriority.Loaded);
    }
}
