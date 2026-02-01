namespace PathBench;

/// <summary>
/// Performance measurement tool for counting code paths.
/// One instance of this type should be related to one type definition.
/// </summary>
public partial class CodePathCounter(
    string? className = null,
    CodePathCounterOptions? options = null)
{
    public const string StartCheckpointName = "#<start>";
    public const string EndCheckpointName = "#<end>";

    private readonly Lock _lockToken = new();
    private readonly int _RecentHistoryCacheSize = options?.RecentHistoryCacheSize ?? CodePathCounterOptions.DefaultRecentHistoryCacheSize;
    private readonly int _WorstHistoryCacheSize = options?.WorstHistoryCacheSize ?? CodePathCounterOptions.DefaultWorstHistoryCacheSize;
    private readonly Queue<InvocationCounter_> _RecentHistory = new(options?.RecentHistoryCacheSize ?? CodePathCounterOptions.DefaultRecentHistoryCacheSize);
    private readonly SortedList<double, InvocationCounter_> _WorstHistory = new(options?.WorstHistoryCacheSize ?? CodePathCounterOptions.DefaultWorstHistoryCacheSize);

    private long _invocationCount = 0;
    private long _terminatedInvocationCount = 0;
    private double _meanDuration_sec = 0;
    private double _sumSquaredDeviationOfDuration_sec2 = 0;

    public string? ClassName { get; } = className;
    public TimeProvider TimeProvider { get; } = options?.TimeProvider ?? TimeProvider.System;

    public virtual InvocationCounter StartMeasurement(
        string? methodName = null,
        object? argumentsExpressionProvider = null)
    {
        var id = Interlocked.Increment(ref _invocationCount);
        var invocation = new InvocationCounter_(
            this,
            methodName,
            id,
            argumentsExpressionProvider);
        return invocation;
    }


    private void TerminateInvocation(InvocationCounter_ invocation)
    {
        // Implementation omitted for brevity.
        using(_lockToken.EnterScope())
        {
            double x = invocation.Duration;
            ref var n = ref _terminatedInvocationCount;
            ref var mu = ref _meanDuration_sec;
            ref var ss = ref _sumSquaredDeviationOfDuration_sec2;
            ss = ss + n / (n + 1.0) * (x - mu) * (x - mu);
            mu = mu + (x - mu) / (n + 1.0);
            n++;

            _RecentHistory.Enqueue(invocation);
            if(_RecentHistory.Count > _RecentHistoryCacheSize)
            {
                _RecentHistory.Dequeue();
            }
            if(x < (_WorstHistory.Values.LastOrDefault()?.Duration ?? double.PositiveInfinity))
            {
                _WorstHistory.Add(x, invocation);
                if(_WorstHistory.Count > _WorstHistoryCacheSize)
                {
                    _WorstHistory.RemoveAt(_WorstHistoryCacheSize - 1);
                }
            }
        }
    }

    private (TimeSpan meanDuration, TimeSpan standardDeviationOfDuration) CalculateStatistics()
    {
        double meanDuration_sec;
        double standardDeviationOfDuration_sec;
        using (_lockToken.EnterScope())
        {
            meanDuration_sec = _meanDuration_sec;
            standardDeviationOfDuration_sec = Math.Sqrt(_sumSquaredDeviationOfDuration_sec2 / (_terminatedInvocationCount - 1));
        }
        return (TimeSpan.FromSeconds(meanDuration_sec), TimeSpan.FromSeconds(standardDeviationOfDuration_sec));
    }
}
