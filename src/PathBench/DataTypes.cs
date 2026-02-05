using System.Collections.Immutable;

namespace PathBench;

/// <summary>
/// Types of invocation measurement history cache.
/// </summary>
[Flags]
public enum HistoryType
{
    Recent,
    Worst,
}

/// <summary>
/// Key identifying a code path between two checkpoints.
/// </summary>
/// <param name="StartCheckpoint"></param>
/// <param name="EndCheckpoint"></param>
public record struct CheckpointTransitionKey(
    string StartCheckpoint,
    string EndCheckpoint);

/// <summary>
/// Summary of method performance measurements.
/// </summary>
/// <param name="CounterName"></param>
/// <param name="TotalTimes"></param>
/// <param name="MeanDuration"></param>
/// <param name="StandardDeviationOfDuration"></param>
/// <param name="CodePathSummaries"></param>
/// <param name="Histories"></param>
public record class MethodProfileReport(
    string CounterName,
    long TotalTimes,
    TimeSpan MeanDuration,
    TimeSpan StandardDeviationOfDuration,
    ImmutableDictionary<CheckpointTransitionKey, CheckPointTransitionProfileReport> CodePathSummaries,
    ImmutableDictionary<HistoryType, ImmutableArray<InvocationMeasurement>> Histories
    );


public record class CheckPointTransitionProfileReport(
    CheckpointTransitionKey Key,
    long TotalTimes,
    TimeSpan MeanDuration,
    TimeSpan StandardDeviationOfDuration);


public record class InvocationMeasurement(
    string CounterName,
    long InvocationId,
    DateTimeOffset StartAt,
    int ManagedThreadId,
    string? ArgumentsExpression,
    TimeSpan Duration,
    ImmutableArray<CheckpointTransitionMeasurement> CodePathMeasurements);


public record class CheckpointTransitionMeasurement(
    CheckpointTransitionKey Key,
    string? Note,
    TimeSpan Duration);

