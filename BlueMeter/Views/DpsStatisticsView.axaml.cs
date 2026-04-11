using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using BlueMeter.Config;
using BlueMeter.Services;
using BlueMeter.ViewModels;

namespace BlueMeter.Views;

/// <summary>
/// Frameless DPS statistics window. Hosts <see cref="SortedDpsControl"/> for
/// per-player rows plus a top bar (metric-type selector, settings context menu,
/// topmost toggle, minimize, collapse toggle) and a bottom bar (battle duration,
/// history indicator, current player stats).
/// </summary>
public partial class DpsStatisticsView : Window
{
    /// <summary>
    /// Toggled by the "pull" (collapse) button to shrink the window down to just
    /// the title bar. Bindable (two-way) so consumers can drive it externally.
    /// </summary>
    public static readonly StyledProperty<bool> CollapseProperty =
        AvaloniaProperty.Register<DpsStatisticsView, bool>(
            nameof(Collapse),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public bool Collapse
    {
        get => GetValue(CollapseProperty);
        set => SetValue(CollapseProperty, value);
    }

    private double _beforePilingHeight;
    private readonly IWindowManagementService? _windowManagement;
    private readonly IMousePenetrationService? _mousePenetration;
    private readonly IConfigManager? _configManager;
    private readonly WindowSettings _windowSettings;
    private bool _isLoadingPosition;
    private Button? _pullButton;
    private RotateTransform? _pullButtonRotate;

    /// <summary>
    /// Parameterless constructor required for the Avalonia XAML loader. Not used
    /// in practice at runtime — the DI ctor below is what resolves through the
    /// Microsoft.Extensions.Hosting container.
    /// </summary>
    public DpsStatisticsView()
    {
        _windowSettings = WindowSettings.Load();
        InitializeComponent();

        _pullButton = this.FindControl<Button>("PullButton");
        if (_pullButton?.RenderTransform is RotateTransform rt)
        {
            _pullButtonRotate = rt;
        }

        Opened += OnOpened;
        PositionChanged += OnPositionChanged;
        // Window.SizeChanged is the Control event in Avalonia 11 — same semantics as WPF's.
        this.GetObservable(BoundsProperty).Subscribe(_ => OnBoundsChanged());
        Closing += OnClosing;
    }

    public DpsStatisticsView(
        DpsStatisticsViewModel vm,
        IWindowManagementService windowManagement,
        IMousePenetrationService mousePenetration,
        IConfigManager configManager) : this()
    {
        DataContext = vm;
        _windowManagement = windowManagement;
        _mousePenetration = mousePenetration;
        _configManager = configManager;
    }

    /// <summary>
    /// WPF's <c>TitleBar_MouseLeftButtonDown</c> + <c>DragMove()</c> is expressed
    /// as a pointer-pressed handler calling <see cref="Window.BeginMoveDrag"/> on
    /// Avalonia. Wired in the XAML via <c>PointerPressed="OnTitleBarPointerPressed"</c>.
    /// </summary>
    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    /// <summary>
    /// Collapse / expand the window body. WPF used a <c>Storyboard</c> with two
    /// <c>DoubleAnimation</c>s targeting <c>Window.Height</c> and the pull button's
    /// <c>RotateTransform.Angle</c>. The Avalonia port fires two
    /// <see cref="Animation"/> instances via <see cref="Animation.RunAsync"/>
    /// in parallel.
    /// </summary>
    private async void OnPullButtonClick(object? sender, RoutedEventArgs e)
    {
        Collapse = !Collapse;

        if (Collapse)
        {
            // Guard against the "collapse looks broken" case where the user has
            // already manually shrunk the window below the collapsed footprint.
            if (Bounds.Height < 60)
            {
                Collapse = false;
                _beforePilingHeight = 360;
            }
            else
            {
                _beforePilingHeight = Bounds.Height;
            }
        }

        // BaseStyle MainCornerRadius (card header 25) + shadow window margin (top 5, bottom 5).
        const double baseHeight = 25 + 5 + 5;

        var targetHeight = Collapse ? baseHeight : _beforePilingHeight;
        var targetAngle = Collapse ? 180.0 : 0.0;
        var duration = TimeSpan.FromMilliseconds(300);
        var easing = new QuadraticEaseInOut();

        // Animate Window.Height. Avalonia's `Animation` targets the object it's
        // run against (this Window), so we use the Height property directly.
        var heightAnim = new Animation
        {
            Duration = duration,
            Easing = easing,
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(HeightProperty, Bounds.Height) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(HeightProperty, targetHeight) }
                }
            }
        };

        Task heightTask = heightAnim.RunAsync(this);

        Task rotateTask = Task.CompletedTask;
        if (_pullButton is not null && _pullButtonRotate is not null)
        {
            var rotateAnim = new Animation
            {
                Duration = duration,
                Easing = easing,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(RotateTransform.AngleProperty, _pullButtonRotate.Angle) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(RotateTransform.AngleProperty, targetAngle) }
                    }
                }
            };
            // RunAsync target must be the owning Visual, not the RotateTransform.
            // Avalonia's TransformAnimator walks from the Visual's RenderTransform
            // down to the matching Transform subclass — passing the transform
            // directly crashes inside TransformAnimator.Apply with
            // "Unable to cast object of type 'RotateTransform' to type 'Visual'".
            rotateTask = rotateAnim.RunAsync(_pullButton);
        }

        await Task.WhenAll(heightTask, rotateTask);
    }

    /// <summary>
    /// Solo Training mode toggle. If the user enables it without a ManualPlayerUid
    /// configured, route them to the Settings window and highlight the UID field.
    /// </summary>
    private void OnSoloTrainingMenuItemClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem || DataContext is not DpsStatisticsViewModel viewModel)
        {
            return;
        }

        // Avalonia MenuItem has no IsCheckable — the item drives its own check state
        // from C#. Toggle IsChecked here to match the WPF UX.
        menuItem.Icon = null;
        menuItem.IsChecked = !menuItem.IsChecked;

        if (menuItem.IsChecked)
        {
            if (viewModel.AppConfig.ManualPlayerUid == 0)
            {
                _windowManagement?.ShowSettingsAndHighlightUidField();
                menuItem.IsChecked = false;
                return;
            }

            viewModel.AppConfig.TrainingMode = Models.TrainingMode.Personal;
        }
        else
        {
            viewModel.AppConfig.TrainingMode = Models.TrainingMode.None;
        }

        if (viewModel.RefreshCommand.CanExecute(null))
        {
            viewModel.RefreshCommand.Execute(null);
        }

        e.Handled = true;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        // Fire the VM's LoadedCommand (WPF used a b:Interaction.Triggers Loaded hook).
        if (DataContext is DpsStatisticsViewModel vm && vm.LoadedCommand.CanExecute(null))
        {
            vm.LoadedCommand.Execute(null);
        }

        // Restore saved window position.
        if (_windowSettings.SaveDpsWindowPosition)
        {
            _isLoadingPosition = true;

            if (_windowSettings.DpsWindowLeft.HasValue && _windowSettings.DpsWindowTop.HasValue)
            {
                Position = new PixelPoint(
                    (int)_windowSettings.DpsWindowLeft.Value,
                    (int)_windowSettings.DpsWindowTop.Value);
            }

            if (_windowSettings.DpsWindowWidth.HasValue)
            {
                Width = _windowSettings.DpsWindowWidth.Value;
            }

            if (_windowSettings.DpsWindowHeight.HasValue)
            {
                Height = _windowSettings.DpsWindowHeight.Value;
            }

            EnsureWindowIsVisible();

            _isLoadingPosition = false;
        }

        // Apply saved mouse-through state so the window's click-through flag
        // matches the persisted config at startup.
        if (_configManager?.CurrentConfig.MouseThroughEnabled == true)
        {
            _mousePenetration?.SetMousePenetrate(this, true);
        }
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (!_isLoadingPosition && _windowSettings.SaveDpsWindowPosition && WindowState == WindowState.Normal)
        {
            _windowSettings.DpsWindowLeft = e.Point.X;
            _windowSettings.DpsWindowTop = e.Point.Y;
        }
    }

    private void OnBoundsChanged()
    {
        if (!_isLoadingPosition && _windowSettings.SaveDpsWindowPosition && WindowState == WindowState.Normal)
        {
            _windowSettings.DpsWindowWidth = Bounds.Width;
            _windowSettings.DpsWindowHeight = Bounds.Height;
        }
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_windowSettings.SaveDpsWindowPosition && WindowState == WindowState.Normal)
        {
            _windowSettings.DpsWindowLeft = Position.X;
            _windowSettings.DpsWindowTop = Position.Y;
            _windowSettings.DpsWindowWidth = Bounds.Width;
            _windowSettings.DpsWindowHeight = Bounds.Height;
            _windowSettings.Save();
        }

        if (DataContext is DpsStatisticsViewModel vm && vm.UnloadedCommand.CanExecute(null))
        {
            vm.UnloadedCommand.Execute(null);
        }
    }

    /// <summary>
    /// Multi-monitor safety check: if the saved position puts the window fully
    /// off every connected screen, recenter it on the primary. Avalonia exposes
    /// screens via <see cref="Window.Screens"/> (a <c>Screens</c> instance with
    /// an <c>All</c> collection and a <c>Primary</c> accessor) instead of WPF's
    /// <c>System.Windows.Forms.Screen.AllScreens</c>.
    /// </summary>
    private void EnsureWindowIsVisible()
    {
        var screens = Screens;
        if (screens is null || screens.All.Count == 0)
        {
            return;
        }

        var windowLeft = Position.X;
        var windowTop = Position.Y;
        var windowRight = windowLeft + (int)Bounds.Width;
        var windowBottom = windowTop + (int)Bounds.Height;

        bool isVisible = false;
        foreach (var screen in screens.All)
        {
            var wa = screen.WorkingArea;
            if (windowRight > wa.X &&
                windowLeft < wa.X + wa.Width &&
                windowBottom > wa.Y &&
                windowTop < wa.Y + wa.Height)
            {
                isVisible = true;
                break;
            }
        }

        if (!isVisible)
        {
            var primary = screens.Primary ?? screens.All[0];
            var wa = primary.WorkingArea;
            var newLeft = wa.X + (wa.Width - (int)Bounds.Width) / 2;
            var newTop = wa.Y + (wa.Height - (int)Bounds.Height) / 2;
            Position = new PixelPoint(newLeft, newTop);
        }
    }
}
