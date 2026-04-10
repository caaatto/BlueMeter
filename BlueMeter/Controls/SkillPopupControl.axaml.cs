using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BlueMeter.Controls;

/// <summary>
/// Tooltip-style popup that lists per-skill stats. Used by the DPS statistics
/// view to surface skill breakdowns on hover.
/// </summary>
public partial class SkillPopupControl : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SkillPopupControl, string>(nameof(Title), "Skill Details");

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SkillPopupControl, IEnumerable?>(nameof(ItemsSource));

    public SkillPopupControl()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
