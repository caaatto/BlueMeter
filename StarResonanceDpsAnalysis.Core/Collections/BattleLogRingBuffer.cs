using System.Buffers;
using System.Runtime.CompilerServices;
using StarResonanceDpsAnalysis.Core.Data.Models;

namespace StarResonanceDpsAnalysis.Core.Collections;

/// <summary>
/// Memory-efficient ring buffer for battle logs with ArrayPool usage
/// </summary>
public sealed class BattleLogRingBuffer : IDisposable
{
    private readonly int _capacity;
    private BattleLog[] _buffer;
    private int _head;
    private int _tail;
    private int _count;
    private readonly object _lock = new object();
    private bool _disposed;

    public BattleLogRingBuffer(int capacity = 10000)
    {
        _capacity = capacity;
        _buffer = ArrayPool<BattleLog>.Shared.Rent(capacity);
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    public int Count 
    { 
        get 
        { 
            lock (_lock) 
            { 
                return _count; 
            } 
        } 
    }

    public int Capacity => _capacity;

    /// <summary>
    /// Add a battle log entry, automatically removing oldest if at capacity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in BattleLog log)
    {
        lock (_lock)
        {
            _buffer[_tail] = log;
            _tail = (_tail + 1) % _capacity;

            if (_count < _capacity)
            {
                _count++;
            }
            else
            {
                // Buffer is full, move head forward
                _head = (_head + 1) % _capacity;
            }
        }
    }

    /// <summary>
    /// Get recent logs with efficient memory usage
    /// </summary>
    public Span<BattleLog> GetRecent(int count)
    {
        lock (_lock)
        {
            if (_count == 0 || count <= 0)
                return Span<BattleLog>.Empty;

            var actualCount = Math.Min(count, _count);
            var result = new BattleLog[actualCount];
            
            var startIndex = _count < _capacity ? 0 : _head;
            
            for (int i = 0; i < actualCount; i++)
            {
                var bufferIndex = (startIndex + _count - actualCount + i) % _capacity;
                result[i] = _buffer[bufferIndex];
            }
            
            return result.AsSpan();
        }
    }

    /// <summary>
    /// Efficiently iterate over all entries without allocating
    /// </summary>
    public void ForEach(Action<BattleLog> action)
    {
        lock (_lock)
        {
            if (_count == 0) return;

            var startIndex = _count < _capacity ? 0 : _head;
            
            for (int i = 0; i < _count; i++)
            {
                var bufferIndex = (startIndex + i) % _capacity;
                action(_buffer[bufferIndex]);
            }
        }
    }

    /// <summary>
    /// Clear all entries efficiently
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _head = 0;
            _tail = 0;
            _count = 0;
            // Don't clear the array - just reset pointers for efficiency
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            lock (_lock)
            {
                if (_buffer != null)
                {
                    ArrayPool<BattleLog>.Shared.Return(_buffer);
                    _buffer = null!;
                }
                _disposed = true;
            }
        }
    }
}