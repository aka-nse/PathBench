using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PathBench;

/// <summary>
/// Performance measurement tool for counting code paths.
/// One instance of this type should be related to one type definition.
/// </summary>
public partial class CodePathProfiler(
    string? className = null,
    CodePathProfilerOptions? options = null)
{
    /// <summary> The name of the start checkpoint. </summary>
    public const string StartCheckpointName = "#<start>";

    /// <summary> The name of the end checkpoint. </summary>
    public const string EndCheckpointName = "#<end>";

    private readonly int _recentHistoryCacheSize = options?.RecentHistoryCacheSize ?? CodePathProfilerOptions.DefaultRecentHistoryCacheSize;
    private readonly int _worstHistoryCacheSize = options?.WorstHistoryCacheSize ?? CodePathProfilerOptions.DefaultWorstHistoryCacheSize;

    private ImmutableDictionary<string, MethodProfiler> _methodCounters = ImmutableDictionary<string, MethodProfiler>.Empty;

    /// <summary> The name of the class which this instance related to. </summary>
    public string? ClassName { get; } = className ?? new StackFrame(1, false).GetMethod()?.DeclaringType?.FullName;

    /// <summary> The time provider used for measuring time. </summary>
    public TimeProvider TimeProvider { get; } = options?.TimeProvider ?? TimeProvider.System;

    /// <summary>
    /// Starts performance measurement of one invocation of the method.
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="argumentsExpressionProvider"></param>
    /// <returns></returns>
    public virtual InvocationProfiler StartMeasurement(
        [CallerMemberName] string methodName = "",
        object? argumentsExpressionProvider = null)
    {
        var methodCounter = InternalHelpers.GetOrAdd(
            ref _methodCounters,
            methodName,
            () => new MethodProfiler(this, methodName));
        var invocation = methodCounter.StartMeasurement(argumentsExpressionProvider);
        return invocation;
    }

    /// <summary>
    /// Creates profile reports for all measured methods.
    /// </summary>
    /// <returns></returns>
    public virtual ImmutableDictionary<string, MethodProfileReport> CreateProfileReports()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, MethodProfileReport>();
        foreach (var kvp in _methodCounters)
        {
            var report = kvp.Value.CreateReport();
            builder.Add(kvp.Key, report);
        }
        return builder.ToImmutable();
    }
}
