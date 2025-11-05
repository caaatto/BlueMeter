using System.Windows;
using System.Windows.Controls;

namespace BlueMeter.WPF.Controls;

public class CardControl : HeaderedContentControl
{
    static CardControl()
    {
        // Ensure the style lookup targets this control type
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CardControl),
            new FrameworkPropertyMetadata(typeof(CardControl)));
    }
}