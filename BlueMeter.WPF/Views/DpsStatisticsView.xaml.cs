using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BlueMeter.WPF.ViewModels;

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

    public DpsStatisticsView(DpsStatisticsViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
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
    /// 打桩模式选择 (Training Mode Selection)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PilingMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var me = (MenuItem)sender;
        var owner = ItemsControl.ItemsControlFromItemContainer(me);

        if (DataContext is not DpsStatisticsViewModel viewModel)
            return;

        System.Diagnostics.Debug.WriteLine($"[MENU CLICK] PilingMenuItem clicked. IsChecked={me.IsChecked}, Header={me.Header}");

        if (me.IsChecked)
        {
            // 这次点击后变成 true：把其它都关掉
            foreach (var obj in owner.Items)
            {
                if (owner.ItemContainerGenerator.ContainerFromItem(obj) is MenuItem mi && !ReferenceEquals(mi, me))
                    mi.IsChecked = false;
            }

            // Set training mode based on selected menu item
            var header = me.Header?.ToString() ?? "";
            System.Diagnostics.Debug.WriteLine($"[MENU CLICK] Header string: {header}");

            if (header.Contains("Personal") || header.Contains("Solo") || header.Contains("个人"))
            {
                viewModel.AppConfig.TrainingMode = Models.TrainingMode.Personal;
                System.Diagnostics.Debug.WriteLine($"[MENU CLICK] TrainingMode set to Personal. Value: {viewModel.AppConfig.TrainingMode}");

                // Check if player UID is configured
                if (viewModel.AppConfig.ManualPlayerUid == 0)
                {
                    // Enable player selection mode - user needs to click their character
                    viewModel.IsSelectingPlayer = true;
                    System.Diagnostics.Debug.WriteLine("[MENU CLICK] Player selection mode enabled");
                }
            }
            else if (header.Contains("Faction") || header.Contains("阵营"))
            {
                viewModel.AppConfig.TrainingMode = Models.TrainingMode.Faction;
                System.Diagnostics.Debug.WriteLine($"[MENU CLICK] TrainingMode set to Faction. Value: {viewModel.AppConfig.TrainingMode}");
            }
            else if (header.Contains("Extreme") || header.Contains("极限"))
            {
                viewModel.AppConfig.TrainingMode = Models.TrainingMode.Extreme;
                System.Diagnostics.Debug.WriteLine($"[MENU CLICK] TrainingMode set to Extreme. Value: {viewModel.AppConfig.TrainingMode}");
            }
        }
        else
        {
            // Unchecked - disable training mode and clear player selection
            viewModel.AppConfig.TrainingMode = Models.TrainingMode.None;
            viewModel.AppConfig.ManualPlayerUid = 0;
            viewModel.IsSelectingPlayer = false;
            System.Diagnostics.Debug.WriteLine($"[MENU CLICK] TrainingMode set to None. Value: {viewModel.AppConfig.TrainingMode}");
        }

        System.Diagnostics.Debug.WriteLine($"[MENU CLICK] Final TrainingMode: {viewModel.AppConfig.TrainingMode}");

        // Clear all current data to ensure filter is applied from scratch
        viewModel.ResetSection();

        // Refresh data immediately to apply the new filter
        if (viewModel.RefreshCommand.CanExecute(null))
        {
            viewModel.RefreshCommand.Execute(null);
        }

        // 这次点击后变成 false：允许"全不选"，什么也不做
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

        // 这次点击后变成 false：允许“全不选”，什么也不做
        e.Handled = true;
    }
}