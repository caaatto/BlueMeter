using System.Windows;
using System.Windows.Controls;

namespace BlueMeter.WPF.Controls;

public class MultiSelectorItemsHost : ItemsControl
{
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is MultiSelectorItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new MultiSelectorItem();
    }
}

internal class MultiSelectorItem : ContentControl
{
}