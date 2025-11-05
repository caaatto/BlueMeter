using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StarResonanceDpsAnalysis.WPF.Models;

/// <summary>
/// ObservableCollection with bulk operation support to minimize UI notifications
/// </summary>
/// <typeparam name="T">The type of elements in the collection</typeparam>
public class BulkObservableCollection<T> : ObservableCollection<T> where T : notnull
{
    private bool _isUpdating;

    /// <summary>
    /// Begins a bulk update operation. Notifications are suppressed until EndUpdate is called.
    /// </summary>
    public void BeginUpdate()
    {
        _isUpdating = true;
    }

    /// <summary>
    /// Ends a bulk update operation and raises a Reset notification.
    /// </summary>
    public void EndUpdate()
    {
        _isUpdating = false;
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    /// Adds multiple items to the collection in bulk
    /// </summary>
    /// <param name="items">Items to add</param>
    public void AddRange(IEnumerable<T> items)
    {
        BeginUpdate();
        try
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
        finally
        {
            EndUpdate();
        }
    }

    /// <summary>
    /// Replaces all items in the collection
    /// </summary>
    /// <param name="items">New items for the collection</param>
    public void ReplaceAll(IEnumerable<T> items)
    {
        BeginUpdate();
        try
        {
            Clear();
            foreach (var item in items)
            {
                Add(item);
            }
        }
        finally
        {
            EndUpdate();
        }
    }

    /// <summary>
    /// Sorts the collection in place using the provided comparison function
    /// </summary>
    /// <param name="comparison">Comparison function</param>
    public void Sort(Comparison<T> comparison)
    {
        var sortedList = Items.ToList();
        sortedList.Sort(comparison);

        BeginUpdate();
        try
        {
            Items.Clear();
            foreach (var item in sortedList)
            {
                Items.Add(item);
            }
        }
        finally
        {
            EndUpdate();
        }
    }

    /// <summary>
    /// Sorts the collection in place using IComparer
    /// </summary>
    /// <param name="comparer">Comparer to use for sorting</param>
    public void Sort(IComparer<T> comparer)
    {
        Sort(comparer.Compare);
    }

    /// <summary>
    /// Sorts the collection in place using a key selector
    /// </summary>
    /// <param name="keySelector">Function to extract the sort key</param>
    /// <param name="descending">Whether to sort in descending order</param>
    public void SortBy<TKey>(Func<T, TKey> keySelector, bool descending = false) where TKey : IComparable<TKey>
    {
        if (Items.Count <= 1) return;

        // Create the list of items in the desired order (references preserved)
        var sortedList = descending
            ? Items.OrderByDescending(keySelector).ToList()
            : Items.OrderBy(keySelector).ToList();

        // If already in desired order, nothing to do
        var same = true;
        for (var i = 0; i < sortedList.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(Items[i], sortedList[i])) continue;
            same = false;
            break;
        }

        if (same) return;

        // Build an index map for quick lookup. Use reference-equality for reference types to avoid relying on Equals overrides.
        var comparer = typeof(T).IsValueType
            ? EqualityComparer<T>.Default
            : (IEqualityComparer<T>)new ReferenceEqualityComparer<T>();
        var indexMap = new Dictionary<T, int>(Items.Count, comparer);
        for (var i = 0; i < Items.Count; i++)
        {
            indexMap[Items[i]] = i;
        }

        // Reorder the underlying collection by moving items to their target indices.
        for (var targetIndex = 0; targetIndex < sortedList.Count; targetIndex++)
        {
            var desiredItem = sortedList[targetIndex];
            if (!indexMap.TryGetValue(desiredItem, out var currentIndex))
                continue; // item not found for some reason

            if (currentIndex == targetIndex) continue;

            // Perform move and then update indexMap for affected range
            Move(currentIndex, targetIndex);

            var start = Math.Min(currentIndex, targetIndex);
            var end = Math.Max(currentIndex, targetIndex);

            for (var i = start; i <= end; i++)
            {
                indexMap[Items[i]] = i;
            }
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (!_isUpdating)
        {
            base.OnCollectionChanged(e);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!_isUpdating)
        {
            base.OnPropertyChanged(e);
        }
    }

    // Reference-equality comparer for reference types to use object identity in dictionary keys
    private sealed class ReferenceEqualityComparer<TRef> : IEqualityComparer<TRef>
    {
        public bool Equals(TRef? x, TRef? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(TRef obj)
        {
            return RuntimeHelpers.GetHashCode(obj!);
        }
    }

    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
        internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");

        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged =
            new(NotifyCollectionChangedAction.Reset);
    }
}