using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.WinForm.Plugin
{
    public sealed class EvictedEventArgs<TKey, TValue>(TKey key, TValue value) : EventArgs
    {
        public TKey Key { get; } = key;
        public TValue Value { get; } = value;
    }

    public sealed class ExpiringCache<TKey, TValue> : IDisposable where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, Entry> _map;
        private readonly TimeSpan _idleExpiration;
        private readonly System.Threading.Timer _sweeper;
        private readonly bool _disposeValueIfIDisposable;

        private int _disposed;
        private int _sweeping;

        public event EventHandler<EvictedEventArgs<TKey, TValue>>? Evicted;


        private sealed class Entry(TValue value)
        {
            public TValue Value = value;
            public long LastAccessTicksUtc = DateTime.UtcNow.Ticks; // DateTime.UtcNow.Ticks
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idleExpiration">条目在未被访问超过此时间后过期</param>
        /// <param name="sweepInterval">清扫周期; 默认: idleExpiration 的一半 (最小 1s)</param>
        /// <param name="onEvicted">条目被清除时的回调 (过期或显式移除/替换)</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ExpiringCache(TimeSpan idleExpiration, TimeSpan? sweepInterval = null, bool disposeValueIfIDisposable = false)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(idleExpiration, TimeSpan.Zero);

            _idleExpiration = idleExpiration;
            _map = new ConcurrentDictionary<TKey, Entry>();
            _disposeValueIfIDisposable = disposeValueIfIDisposable;

            var interval = sweepInterval ?? TimeSpan.FromMilliseconds(Math.Max(1000, _idleExpiration.TotalMilliseconds / 2d));
            _sweeper = new System.Threading.Timer(Sweep, null, interval, interval);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long UtcNowTicks() => DateTime.UtcNow.Ticks;

        // 访问即续期
        public bool TryGet(TKey key, out TValue? value)
        {
            ObjectDisposedException.ThrowIf(_disposed == 1, this);

            if (_map.TryGetValue(key, out var e))
            {
                Volatile.Write(ref e.LastAccessTicksUtc, UtcNowTicks());
                value = e.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// 设置/覆盖
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(TKey key, TValue value)
        {
            ObjectDisposedException.ThrowIf(_disposed == 1, this);

            _map.AddOrUpdate(
                key,
                _ => new Entry(value),
                (_, old) =>
                {
                    var prev = old.Value;
                    old.Value = value;
                    Volatile.Write(ref old.LastAccessTicksUtc, UtcNowTicks());
                    EvictValue(key, prev);
                    return old;
                });
        }

        /// <summary>
        /// 获取/添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        public TValue GetOrAdd(TKey key, Func<TValue> valueFactory)
        {
            ObjectDisposedException.ThrowIf(_disposed == 1, this);

            var entry = _map.AddOrUpdate(
                key,
                _ => new Entry(valueFactory()),
                (_, existing) =>
                {
                    Volatile.Write(ref existing.LastAccessTicksUtc, UtcNowTicks());
                    return existing;
                });

            return entry.Value;
        }

        // 显式移除
        public bool Remove(TKey key)
        {
            if (_disposed == 1) return false;

            if (_map.TryRemove(key, out var entry))
            {
                EvictValue(key, entry.Value);
                return true;
            }
            return false;
        }

        public bool ContainsKey(TKey key) => _map.ContainsKey(key);

        public int Count => _map.Count;

        public void Clear()
        {
            foreach (var kv in _map)
            {
                if (_map.TryRemove(kv.Key, out var entry))
                {
                    EvictValue(kv.Key, entry.Value);
                }
            }
        }

        private void Sweep(object? _)
        {
            // 防重入
            if (Interlocked.Exchange(ref _sweeping, 1) == 1) return;

            try
            {
                if (_disposed == 1) return;

                var nowTicks = UtcNowTicks();
                foreach (var kv in _map)
                {
                    var lastTicks = Volatile.Read(ref kv.Value.LastAccessTicksUtc);
                    if (new TimeSpan(nowTicks - lastTicks) > _idleExpiration)
                    {
                        // 双重检查: TryRemove 成功才回调
                        if (_map.TryRemove(kv.Key, out var removed))
                        {
                            EvictValue(kv.Key, removed.Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"""
                    Exception thrown in ExpiringCache.Sweep(object? _)
                    Message: {ex.Message}
                    StackTrace:{ex.StackTrace}
                    """);
            }
            finally
            {
                Volatile.Write(ref _sweeping, 0);
            }
        }

        private void EvictValue(TKey key, TValue value)
        {
            try
            {
                OnEvicted(new EvictedEventArgs<TKey, TValue>(key, value));
            }
            finally
            {
                if (_disposeValueIfIDisposable && value is IDisposable d)
                {
                    try { d.Dispose(); }
                    catch { }
                }
            }
        }

        private void OnEvicted(EvictedEventArgs<TKey, TValue> e)
        {
            // 拷贝委托，避免并发订阅变更引起 NRE
            var handler = Evicted;
            if (handler == null) return;

            try
            {
                handler(this, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"""
                    Exception thrown when ExpiringCache.OnEvicted(EvictedEventArgs<TKey, TValue> e)
                    Message: {ex.Message}
                    StackTrace:{ex.StackTrace}
                    """);
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
            _sweeper.Dispose();
            Clear();
        }
    }
}
