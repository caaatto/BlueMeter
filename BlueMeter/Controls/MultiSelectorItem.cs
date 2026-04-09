using Avalonia.Controls;

namespace BlueMeter.Controls;

public class MultiSelectorItemsHost : ItemsControl
{
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not MultiSelectorItem;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new MultiSelectorItem();
    }
}

internal class MultiSelectorItem : ContentControl
{
}
