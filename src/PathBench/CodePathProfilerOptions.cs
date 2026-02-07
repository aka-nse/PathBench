namespace PathBench;


/// <summary>
/// Provides configuration options for the code path profiler, including cache sizes and time provider settings.
/// </summary>
public interface ICodePathProfilerOptions
{
    /// <summary>
    /// Specifies the time provider used for measuring time.
    /// </summary>
    public TimeProvider TimeProvider { get; }

    /// <summary>
    /// Specifies the size of entries to retain in the recent history cache.
    /// </summary>
    public int RecentHistoryCacheSize { get; }

    /// <summary>
    /// Specifies the size of entries to retain in the worst history cache.
    /// </summary>
    public int WorstHistoryCacheSize { get; }
}

/// <summary>
/// Provides configuration options for the code path profiler, including cache sizes and time provider settings.
/// </summary>
public class CodePathProfilerOptions
    : ICodePathProfilerOptions
{
    private sealed class Default_ : ICodePathProfilerOptions
    {
        public TimeProvider TimeProvider => TimeProvider.System;
        public int RecentHistoryCacheSize => 256;
        public int WorstHistoryCacheSize => 256;
    }
    internal static ICodePathProfilerOptions Default { get; } = new Default_();

    /// <inheritdoc/>
    public TimeProvider TimeProvider { get; set; } = Default.TimeProvider;

    /// <inheritdoc/>
    public int RecentHistoryCacheSize { get; set; } = Default.RecentHistoryCacheSize;

    /// <inheritdoc/>
    public int WorstHistoryCacheSize { get; set; } = Default.WorstHistoryCacheSize;

}