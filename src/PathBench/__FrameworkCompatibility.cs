#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0130 // Namespace does not match folder structure

#if !NET5_0_OR_GREATER
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit;
}

namespace System.Runtime.InteropServices
{
    internal static class CollectionsMarshal
    {
        private sealed class ListCompat<T>
        {
            internal T[]? _items = default; // Do not rename (binary serialization)
            internal int _size = default; // Do not rename (binary serialization)
            internal int _version = default; // Do not rename (binary serialization)
        }

        public static ReadOnlySpan<T> AsSpan<T>(List<T>? list)
        {
            Span<T> span = default;
            if (list is not null)
            {
                var listCompat = Unsafe.As<List<T>, ListCompat<T>>(ref list);
                int size = listCompat._size;
                T[] items = listCompat._items!;

                if ((uint)size > (uint)items.Length)
                {
                    throw new InvalidOperationException();
                }

                span = items.AsSpan(0, size);
            }

            return span;
        }
    }
}
#endif

#if !NET7_0_OR_GREATER
namespace System
{
    internal static class TimeSpanExtensions
    {
        extension(TimeSpan ts)
        {
            public static long TicksPerMicrosecond => TimeSpan.TicksPerMillisecond / 1000;
        }
    }
}
#endif

#if !NET9_0_OR_GREATER
namespace System.Threading
{
    internal class Lock
    {
        private sealed class Scope(object token) : IDisposable
        {
            public void Dispose()
            {
                Monitor.Exit(token);
            }
        }

        private readonly object _token = new();

        internal IDisposable EnterScope()
        {
            Monitor.Enter(_token);
            return new Scope(_token);
        }
    }
}
#endif

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_0_OR_GREATER
namespace System.Collections.Generic
{
    internal static class CollectionExtensions
    {
        extension<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            public bool TryAdd(TKey key, TValue value)
            {
                if (dict.ContainsKey(key)) return false;
                dict.Add(key, value);
                return true;
            }
        }

        extension<TKey, TValue>(KeyValuePair<TKey, TValue> kv)
        {
            public void Deconstruct(out TKey key, out TValue value)
            {
                key = kv.Key;
                value = kv.Value;
            }
        }
    }
}
#endif

#pragma warning restore IDE0130 // Namespace does not match folder structure
#pragma warning restore IDE0079 // Remove unnecessary suppression