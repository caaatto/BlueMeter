using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BlueMeter.ViewModels;

namespace BlueMeter.Controls;

/// <summary>
/// Tooltip-style popup that lists per-skill stats in the footer/overlay style
/// (smaller text, dark semi-transparent background). Used from the DPS meter
/// row hover path.
/// </summary>
public partial class DpsDetailPopup : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<DpsDetailPopup, string>(nameof(Title), "Skill Details");

    public static readonly StyledProperty<IEnumerable<SkillItemViewModel>?> SkillListProperty =
        AvaloniaProperty.Register<DpsDetailPopup, IEnumerable<SkillItemViewModel>?>(nameof(SkillList));

    public DpsDetailPopup()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IEnumerable<SkillItemViewModel>? SkillList
    {
        get => GetValue(SkillListProperty);
        set => SetValue(SkillListProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
