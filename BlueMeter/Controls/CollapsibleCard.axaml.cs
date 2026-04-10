using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace BlueMeter.Controls;

/// <summary>
/// Titled card with a header bar and a content area.
///
/// Port notes: WPF declared an <c>IsExpanded</c> dependency property but never
/// wired it to anything — the control always rendered expanded. The property
/// is preserved here for binding compatibility with callers that still set
/// <c>IsExpanded="True"</c>, but has no visual effect.
/// </summary>
public partial class CollapsibleCard : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<CollapsibleCard, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<CollapsibleCard, bool>(nameof(IsExpanded), defaultValue: true);

    public static readonly StyledProperty<object?> CardContentProperty =
        AvaloniaProperty.Register<CollapsibleCard, object?>(nameof(CardContent));

    public CollapsibleCard()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    [Content]
    public object? CardContent
    {
        get => GetValue(CardContentProperty);
        set => SetValue(CardContentProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
