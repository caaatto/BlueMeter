using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BlueMeter.WPF.ViewModels;
using BlueMeter.WPF.Services;
using BlueMeter.WPF.Config;

namespace BlueMeter.WPF.Views;

/// <summary>
///     DpsStatisticsForm.xaml 的交互逻辑
/// </summary>
public partial class DpsStatisticsView : Window
{
    public static readonly DependencyProperty CollapseProperty =
        DependencyProperty.Register(
            nameof(Collapse),
            typeof(bool),
            typeof(DpsStatisticsView),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    private double _beforePilingHeight;
    private readonly IWindowManagementService _windowManagement;
    private readonly WindowSettings _windowSettings;
    private bool _isLoadingPosition;

    public DpsStatisticsView(DpsStatisticsViewModel vm, IWindowManagementService windowManagement)
    {
        DataContext = vm;
        _windowManagement = windowManagement;
        _windowSettings = WindowSettings.Load();
        InitializeComponent();

        // Subscribe to window events for position saving
        Loaded += DpsStatisticsView_Loaded;
        LocationChanged += DpsStatisticsView_LocationChanged;
        SizeChanged += DpsStatisticsView_SizeChanged;
        Closing += DpsStatisticsView_Closing;
    }

    public bool Collapse
    {
        get => (bool)GetValue(CollapseProperty);
        set => SetValue(CollapseProperty, value);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void PullButton_Click(object sender, RoutedEventArgs e)
    {
        Collapse = !Collapse;

        if (Collapse)
        {
            // 防止用户手动缩小窗体到一定大小后, 折叠功能看似失效的问题
            if (ActualHeight < 60)
            {
                Collapse = false;
                _beforePilingHeight = 360;
            }
            else
            {
                _beforePilingHeight = ActualHeight;
            }
        }

        // BaseStyle.CardHeaderHeight(25) + BaseStyle.ShadowWindowBorder.Margin((Top)5 + (Bottom)5)
        var baseHeight = 25 + 5 + 5;

        var sb = new Storyboard { FillBehavior = FillBehavior.HoldEnd };
        var duration = new Duration(TimeSpan.FromMilliseconds(300));
        var easingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };
        var animationHeight = new DoubleAnimation
        {
            From = ActualHeight,
            To = Collapse ? baseHeight : _beforePilingHeight,
            Duration = duration,
            EasingFunction = easingFunction
        };
        Storyboard.SetTarget(animationHeight, this);
        Storyboard.SetTargetProperty(animationHeight, new PropertyPath(HeightProperty));
        sb.Children.Add(animationHeight);

        var pullButtonTransformDA = new DoubleAnimation
        {
            To = Collapse ? 180 : 0,
            Duration = duration,
            EasingFunction = easingFunction
        };
        Storyboard.SetTarget(pullButtonTransformDA, PullButton);
        Storyboard.SetTargetProperty(pullButtonTransformDA,
            new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)"));
        sb.Children.Add(pullButtonTransformDA);

        sb.Begin();
    }

    /// <summary>
    /// Solo Training Mode Toggle
    /// </summary>
    private void PilingMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var me = (MenuItem)sender;

        if (DataContext is not DpsStatisticsViewModel viewModel)
            return;

        Console.WriteLine($"[SOLO TRAINING] Clicked. IsChecked={me.IsChecked}");

        if (me.IsChecked)
        {
            // Enabling Solo Training - check if UID is configured
            if (viewModel.AppConfig.ManualPlayerUid == 0)
            {
                Console.WriteLine("[SOLO TRAINING] UID not configured - opening Settings");
                _windowManagement.SettingsView.ShowAndHighlightUidField();
                me.IsChecked = false;
                return;
            }

            // Enable Solo Training mode
            viewModel.AppConfig.TrainingMode = Models.TrainingMode.Personal;
            Console.WriteLine($"[SOLO TRAINING] Enabled. UID: {viewModel.AppConfig.ManualPlayerUid}");
        }
        else
        {
            // Disable Solo Training mode
            viewModel.AppConfig.TrainingMode = Models.TrainingMode.None;
            Console.WriteLine("[SOLO TRAINING] Disabled");
        }

        // Refresh data to apply the filter
        if (viewModel.RefreshCommand.CanExecute(null))
        {
            viewModel.RefreshCommand.Execute(null);
        }

        e.Handled = true;
    }

    /// <summary>
    /// 测伤模式
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AxisMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var me = (MenuItem)sender;
        var owner = ItemsControl.ItemsControlFromItemContainer(me);

        if (me.IsChecked)
        {
            // 这次点击后变成 true：把其它都关掉
            foreach (var obj in owner.Items)
            {
                if (owner.ItemContainerGenerator.ContainerFromItem(obj) is MenuItem mi && !ReferenceEquals(mi, me))
                    mi.IsChecked = false;
            }
            // me 已经是 true，不用再设
        }

        // 这次点击后变成 false：允许"全不选"，什么也不做
        e.Handled = true;
    }

    private void DpsStatisticsView_Loaded(object sender, RoutedEventArgs e)
    {
        // Restore window position if enabled
        if (_windowSettings.SaveDpsWindowPosition)
        {
            _isLoadingPosition = true;

            if (_windowSettings.DpsWindowLeft.HasValue)
                Left = _windowSettings.DpsWindowLeft.Value;

            if (_windowSettings.DpsWindowTop.HasValue)
                Top = _windowSettings.DpsWindowTop.Value;

            if (_windowSettings.DpsWindowWidth.HasValue)
                Width = _windowSettings.DpsWindowWidth.Value;

            if (_windowSettings.DpsWindowHeight.HasValue)
                Height = _windowSettings.DpsWindowHeight.Value;

            // Ensure window is visible on screen
            EnsureWindowIsVisible();

            _isLoadingPosition = false;
        }
    }

    private void DpsStatisticsView_LocationChanged(object? sender, EventArgs e)
    {
        if (!_isLoadingPosition && _windowSettings.SaveDpsWindowPosition && WindowState == WindowState.Normal)
        {
            _windowSettings.DpsWindowLeft = Left;
            _windowSettings.DpsWindowTop = Top;
        }
    }

    private void DpsStatisticsView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!_isLoadingPosition && _windowSettings.SaveDpsWindowPosition && WindowState == WindowState.Normal)
        {
            _windowSettings.DpsWindowWidth = Width;
            _windowSettings.DpsWindowHeight = Height;
        }
    }

    private void DpsStatisticsView_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Save position on close (like Details SavedVariables)
        if (_windowSettings.SaveDpsWindowPosition && WindowState == WindowState.Normal)
        {
            _windowSettings.DpsWindowLeft = Left;
            _windowSettings.DpsWindowTop = Top;
            _windowSettings.DpsWindowWidth = Width;
            _windowSettings.DpsWindowHeight = Height;

            _windowSettings.Save();
        }
    }

    private void EnsureWindowIsVisible()
    {
        // Check if window is visible on ANY screen (multi-monitor support)
        bool isVisible = false;

        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            var bounds = screen.WorkingArea;

            // Check if at least part of the window is visible on this screen
            if (Left + Width > bounds.Left &&
                Left < bounds.Right &&
                Top + Height > bounds.Top &&
                Top < bounds.Bottom)
            {
                isVisible = true;
                break;
            }
        }

        if (!isVisible)
        {
            // Window is not visible on any screen - reset to primary screen
            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen?.WorkingArea ?? System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
            Left = (primaryScreen.Width - Width) / 2 + primaryScreen.Left;
            Top = (primaryScreen.Height - Height) / 2 + primaryScreen.Top;
        }
    }
}