using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace BlueMeter.WPF.Controls;

/// <summary>
/// SkillPopupControl.xaml 的交互逻辑
/// </summary>
public partial class SkillPopupControl : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SkillPopupControl),
            new PropertyMetadata("技能详情") // 默认值
        );

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(SkillPopupControl),
            new PropertyMetadata(null)
        );

    public SkillPopupControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 弹窗标题，比如“技能统计”
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// 技能列表数据源，比如 ObservableCollection&lt;SkillItemViewModel&gt;
    /// </summary>
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
}