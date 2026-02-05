using System.Diagnostics;

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

    private readonly int _RecentHistoryCacheSize = options?.RecentHistoryCacheSize ?? CodePathCounterOptions.DefaultRecentHistoryCacheSize;
    private readonly int _WorstHistoryCacheSize = options?.WorstHistoryCacheSize ?? CodePathCounterOptions.DefaultWorstHistoryCacheSize;


    public string? ClassName { get; } = className ?? new StackFrame(1, false).GetMethod()?.Name;
    public TimeProvider TimeProvider { get; } = options?.TimeProvider ?? TimeProvider.System;


    public virtual InvocationCounter StartMeasurement(
        string? methodName = null,
        object? argumentsExpressionProvider = null)
    {
        var invocation = new InvocationCounter_(
            this,
            methodName,
            id,
            argumentsExpressionProvider);
        return invocation;
    }


}
