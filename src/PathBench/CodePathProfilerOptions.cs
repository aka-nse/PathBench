namespace PathBench;

public class CodePathProfilerOptions
{
    public const int DefaultRecentHistoryCacheSize = 256;
    public const int DefaultWorstHistoryCacheSize = 256;

    public TimeProvider? TimeProvider { get; set; } = null;
    public int RecentHistoryCacheSize { get; set; } = DefaultRecentHistoryCacheSize;
    public int WorstHistoryCacheSize { get; set; } = DefaultWorstHistoryCacheSize;
}
