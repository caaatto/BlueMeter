using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace StarResonanceDpsAnalysis.Core.Caching;

/// <summary>
/// Memory-efficient LRU cache for frequently accessed data
/// </summary>
public sealed class LRUCache<TKey, TValue> : IDisposable where TKey : notnull
{
    private readonly int _maxSize;
    private readonly ConcurrentDictionary<TKey, LinkedListNode<CacheItem>> _dictionary;
    private readonly LinkedList<CacheItem> _linkedList;
    private readonly ReaderWriterLockSlim _lock;
    private bool _disposed;

    public LRUCache(int maxSize = 1000)
    {
        _maxSize = maxSize;
        _dictionary = new ConcurrentDictionary<TKey, LinkedListNode<CacheItem>>();
        _linkedList = new LinkedList<CacheItem>();
        _lock = new ReaderWriterLockSlim();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(TKey key, out TValue value)
    {
        _lock.EnterReadLock();
        try
        {
            if (_dictionary.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                
                // Move to front (most recently used)
                _lock.ExitReadLock();
                _lock.EnterWriteLock();
                try
                {
                    _linkedList.Remove(node);
                    _linkedList.AddFirst(node);
                }
                finally
                {
                    _lock.ExitWriteLock();
                    _lock.EnterReadLock();
                }
                
                return true;
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        value = default!;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(TKey key, TValue value)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_dictionary.TryGetValue(key, out var existingNode))
            {
                // Update existing
                existingNode.Value.Value = value;
                _linkedList.Remove(existingNode);
                _linkedList.AddFirst(existingNode);
            }
            else
            {
                // Add new
                var newItem = new CacheItem { Key = key, Value = value };
                var newNode = new LinkedListNode<CacheItem>(newItem);
                
                _dictionary[key] = newNode;
                _linkedList.AddFirst(newNode);

                // Remove oldest if over capacity
                if (_linkedList.Count > _maxSize)
                {
                    var lastNode = _linkedList.Last!;
                    _linkedList.RemoveLast();
                    _dictionary.TryRemove(lastNode.Value.Key, out _);
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _dictionary.Clear();
            _linkedList.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _linkedList.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    private class CacheItem
    {
        public TKey Key { get; set; } = default!;
        public TValue Value { get; set; } = default!;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _lock?.Dispose();
            _disposed = true;
        }
    }
}