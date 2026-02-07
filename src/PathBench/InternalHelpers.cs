using System.Collections.Immutable;

namespace PathBench;

internal static class InternalHelpers
{
    public static TValue GetOrAdd<TKey, TValue>(
        ref ImmutableDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue> factory)
        where TKey : notnull
    {
        while (true)
        {
            var snapshot = dictionary;
            if (snapshot.TryGetValue(key, out var existingValue))
            {
                return existingValue;
            }
            var newValue = factory();
            var newDictionary = snapshot.Add(key, newValue);
            var originalDictionary = Interlocked.CompareExchange(
                ref dictionary,
                newDictionary,
                snapshot);
            if(ReferenceEquals(originalDictionary, snapshot))
            {
                return newValue;
            }
        }
    }
}
