using System.Collections.Immutable;

namespace PathBench;

/// <summary>
/// Types of invocation measurement history cache.
/// </summary>
[Flags]
public enum HistoryType
{
    /// <summary>
    /// Recent invocation measurements.
    /// </summary>
    Recent,

    /// <summary>
    /// Worst-performance invocation measurements.
    /// </summary>
    Worst,
}

/// <summary>
/// Represents metadata associated with a checkpoint operation.
/// </summary>
/// <param name="Name"></param>
/// <param name="SortKey"></param>
public record class CheckpointMetadata(
    string Name,
    int SortKey);

/// <summary>
/// Key identifying a code path between two checkpoints.
/// </summary>
/// <param name="StartCheckpoint"></param>
/// <param name="EndCheckpoint"></param>
public record struct CheckpointTransitionKey(
    string StartCheckpoint,
    string EndCheckpoint)
{
    /// <inheritdoc />
    public override readonly string ToString() =>
        $"[{StartCheckpoint} -> {EndCheckpoint}]";

    /// <inheritdoc />
    public override readonly int GetHashCode() =>
        StartCheckpoint.GetHashCode() ^ EndCheckpoint.GetHashCode();
}

/// <summary>
/// Summary of method performance measurements.
/// </summary>
/// <param name="CounterName"></param>
/// <param name="TotalTimes"></param>
/// <param name="MeanDuration"></param>
/// <param name="StandardDeviationOfDuration"></param>
/// <param name="MaxDuration"></param>
/// <param name="MinDuration"></param>
/// <param name="FoundCheckpoints"></param>
/// <param name="CodePathSummaries"></param>
/// <param name="Histories"></param>
public record class MethodProfileReport(
    string CounterName,
    long TotalTimes,
    PreciseDuration MeanDuration,
    PreciseDuration StandardDeviationOfDuration,
    PreciseDuration MaxDuration,
    PreciseDuration MinDuration,
    ImmutableDictionary<string, CheckpointMetadata> FoundCheckpoints,
    ImmutableDictionary<CheckpointTransitionKey, CheckpointTransitionProfileReport> CodePathSummaries,
    ImmutableDictionary<HistoryType, ImmutableArray<InvocationMeasurementReport>> Histories
    )
{
    /// <inheritdoc />
    public override string ToString()
    {
        var sw = new StringWriter();
        MethodProfileReportFormatter.Simple.Format(
            this,
            sw);
        return sw.ToString();
    }
}

/// <summary>
/// Summary of performance measurements for a specific two checkpoint transition.
/// </summary>
/// <param name="Key"></param>
/// <param name="TotalTimes"></param>
/// <param name="MeanDuration"></param>
/// <param name="StandardDeviationOfDuration"></param>
/// <param name="MaxDuration"></param>
/// <param name="MinDuration"></param>
public record class CheckpointTransitionProfileReport(
    CheckpointTransitionKey Key,
    long TotalTimes,
    PreciseDuration MeanDuration,
    PreciseDuration StandardDeviationOfDuration,
    PreciseDuration MaxDuration,
    PreciseDuration MinDuration);

/// <summary>
/// Summary of one invocation measurement.
/// </summary>
/// <param name="CounterName"></param>
/// <param name="InvocationId"></param>
/// <param name="StartAt"></param>
/// <param name="ManagedThreadId"></param>
/// <param name="ArgumentsExpression"></param>
/// <param name="Duration"></param>
/// <param name="CodePathMeasurements"></param>
public record class InvocationMeasurementReport(
    string CounterName,
    long InvocationId,
    DateTimeOffset StartAt,
    int ManagedThreadId,
    string? ArgumentsExpression,
    PreciseDuration Duration,
    ImmutableArray<CheckpointTransitionMeasurementReport> CodePathMeasurements);

/// <summary>
/// Summary of one checkpoint transition measurement.
/// </summary>
/// <param name="Key"></param>
/// <param name="Note"></param>
/// <param name="Duration"></param>
public record class CheckpointTransitionMeasurementReport(
    CheckpointTransitionKey Key,
    string? Note,
    PreciseDuration Duration);

