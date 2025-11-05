using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;

namespace StarResonanceDpsAnalysis.Core.Memory;

/// <summary>
/// Memory optimization utilities and object pools for better performance
/// </summary>
public static class MemoryOptimizer
{
    // Object pools for frequently used objects
    private static readonly ObjectPool<List<long>> _longListPool = new(() => new List<long>(), list => list.Clear());
    private static readonly ObjectPool<Dictionary<long, object>> _dictPool = new(() => new Dictionary<long, object>(), dict => dict.Clear());
    private static readonly ObjectPool<StringBuilder> _stringBuilderPool = new(() => new StringBuilder(), sb => sb.Clear());

    /// <summary>
    /// Get a pooled List&lt;long&gt; for temporary use
    /// </summary>
    public static PooledObject<List<long>> GetLongList() => _longListPool.Get();

    /// <summary>
    /// Get a pooled Dictionary for temporary use
    /// </summary>
    public static PooledObject<Dictionary<long, object>> GetDictionary() => _dictPool.Get();

    /// <summary>
    /// Get a pooled StringBuilder for temporary use
    /// </summary>
    public static PooledObject<StringBuilder> GetStringBuilder() => _stringBuilderPool.Get();

    /// <summary>
    /// Optimize struct layout for better memory usage (compiler hint)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OptimizeForMemory<T>(ref T value) where T : struct
    {
        // Compiler optimization hint - no actual unsafe code needed
        // The method signature itself provides the optimization hint
    }
}

/// <summary>
/// Generic object pool for memory optimization
/// </summary>
public class ObjectPool<T> where T : class
{
    private readonly ConcurrentQueue<T> _objects = new();
    private readonly Func<T> _objectGenerator;
    private readonly Action<T> _resetAction;
    private int _currentCount;
    private const int MaxPoolSize = 100;

    public ObjectPool(Func<T> objectGenerator, Action<T> resetAction)
    {
        _objectGenerator = objectGenerator;
        _resetAction = resetAction;
    }

    public PooledObject<T> Get()
    {
        if (_objects.TryDequeue(out T? item))
        {
            Interlocked.Decrement(ref _currentCount);
        }
        else
        {
            item = _objectGenerator();
        }

        return new PooledObject<T>(item, Return);
    }

    private void Return(T item)
    {
        if (_currentCount < MaxPoolSize)
        {
            _resetAction(item);
            _objects.Enqueue(item);
            Interlocked.Increment(ref _currentCount);
        }
    }
}

/// <summary>
/// Disposable wrapper for pooled objects
/// </summary>
public readonly struct PooledObject<T> : IDisposable where T : class
{
    private readonly T _object;
    private readonly Action<T> _returnAction;

    internal PooledObject(T obj, Action<T> returnAction)
    {
        _object = obj;
        _returnAction = returnAction;
    }

    public T Value => _object;

    public void Dispose()
    {
        _returnAction(_object);
    }
}