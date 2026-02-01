using System.Collections.Immutable;

namespace PathBench;

[Flags]
public enum HistoryType
{
    Recent,
    Worst,
}


public record struct CodePathKey(
    string StartCheckpoint,
    string EndCheckpoint);


public record class InvocationSummary(
    string CounterName,
    long TotalTimes,
    TimeSpan MeanDuration,
    TimeSpan StandardDeviationOfDuration,
    ImmutableDictionary<CodePathKey, CodePathSummary> CodePathSummaries,
    ImmutableDictionary<HistoryType, ImmutableArray<InvocationMeasurement>> Histories
    );


public record class CodePathSummary(
    CodePathKey Key,
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
    ImmutableArray<CodePathMeasurement> CodePathMeasurements);


public record class CodePathMeasurement(
    CodePathKey Key,
    string? Note,
    TimeSpan Duration);

