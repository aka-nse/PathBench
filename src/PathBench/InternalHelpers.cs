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


    public static void CalculateStatistics(double x, ref long n, ref double mu, ref double ss)
    {
        ss = ss + n / (n + 1.0) * (x - mu) * (x - mu);
        mu = mu + (x - mu) / (n + 1.0);
        n++;
    }
}
