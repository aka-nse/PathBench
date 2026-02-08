using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace PathBench;

partial class CodePathProfiler
{
    partial class Impl_
    {
        private class MethodProfiler(Impl_ owner, string? methodName = null)
        {
            private record struct WorstHistoryKey(double DurationSec, long InvocationId);
            private sealed class WorstHistoryKeyComparer : IComparer<WorstHistoryKey>
            {
                public static WorstHistoryKeyComparer Instance { get; } = new();

                public int Compare(WorstHistoryKey x, WorstHistoryKey y)
                {
                    var c = x.DurationSec.CompareTo(y.DurationSec);
                    if (c != 0) return -c;
                    return -x.InvocationId.CompareTo(y.InvocationId);
                }
            }

            private readonly Lock _lockToken = new();
            private readonly Dictionary<CheckpointTransitionKey, CodePathResult> _codePathResults = [];
            private readonly Queue<InvocationProfiler_> _recentHistory = new(owner._recentHistoryCacheSize);
            private readonly SortedList<WorstHistoryKey, InvocationProfiler_> _worstHistory = new(owner._worstHistoryCacheSize, WorstHistoryKeyComparer.Instance);

            private long _invocationCount = 0;
            private WelfordStatistics _overallDurations;

            public Impl_ Owner => owner;
            public string Name { get; } = $"{owner.ClassName}.{methodName}";


            public InvocationProfiler StartMeasurement(object? argumentsExpressionProvider = null)
            {
                var id = Interlocked.Increment(ref _invocationCount);
                var invocation = new InvocationProfiler_(
                    this,
                    id,
                    argumentsExpressionProvider);
                return invocation;
            }


            public void TerminateInvocation(InvocationProfiler_ invocation)
            {
                using (_lockToken.EnterScope())
                {
                    var x = (double)invocation.Duration / Owner.TimeProvider.TimestampFrequency;

                    // update overall statistics
                    _overallDurations.IncrementResult(x);

                    // update code path report
                    var checkpoints = CollectionsMarshal.AsSpan(invocation.Checkpoints);
                    for (var i = 0; i < checkpoints.Length - 1; ++i)
                    {
                        var start = invocation.Checkpoints[i];
                        var end = invocation.Checkpoints[i + 1];
                        var key = new CheckpointTransitionKey(start.Name, end.Name);
                        var y = (double)(end.DurationTimestamp - start.DurationTimestamp) / Owner.TimeProvider.TimestampFrequency;
                        if (!_codePathResults.TryGetValue(key, out var result))
                        {
                            result = new(key, start.SortKey, end.SortKey);
                            _codePathResults.Add(key, result);
                        }
                        result.IncrementResult(y);
                    }

                    // update recent history
                    _recentHistory.Enqueue(invocation);
                    if (_recentHistory.Count > Owner._recentHistoryCacheSize)
                    {
                        _recentHistory.Dequeue();
                    }

                    // update worst history
                    if (x < (_worstHistory.Values.LastOrDefault()?.Duration ?? double.PositiveInfinity))
                    {
                        _worstHistory.Add(new(x, invocation.InvocationIndex), invocation);
                        if (_worstHistory.Count > Owner._worstHistoryCacheSize)
                        {
                            _worstHistory.RemoveAt(Owner._worstHistoryCacheSize - 1);
                        }
                    }
                }
            }


            public MethodProfileReport CreateReport()
            {
                long times;
                double mean_sec;
                double sd_sec;
                InvocationProfiler_[] recentHistory;
                InvocationProfiler_[] worstHistory;
                var foundCheckpoints = ImmutableDictionary.CreateBuilder<string, CheckpointMetadata>();
                var codePathSummaries = ImmutableDictionary.CreateBuilder<CheckpointTransitionKey, CheckpointTransitionProfileReport>();
                using (_lockToken.EnterScope())
                {
                    (times, mean_sec, sd_sec) = _overallDurations;
                    foreach (var (key, result) in _codePathResults)
                    {
                        foundCheckpoints.TryAdd(key.StartCheckpoint, new CheckpointMetadata(key.StartCheckpoint, result.StartCheckpointSortKey));
                        foundCheckpoints.TryAdd(key.EndCheckpoint, new CheckpointMetadata(key.EndCheckpoint, result.EndCheckpointSortKey));
                        codePathSummaries.Add(key, result.CreateSummary());
                    }
                    recentHistory = [.. _recentHistory];
                    worstHistory = [.. _worstHistory.Values];
                }
                var histories = ImmutableDictionary.CreateBuilder<HistoryType, ImmutableArray<InvocationMeasurementReport>>();
                histories.Add(HistoryType.Recent, [.. recentHistory.Select(static x => x.CreateMeasurementReport())]);
                histories.Add(HistoryType.Worst, [.. worstHistory.Select(static x => x.CreateMeasurementReport())]);
                return new MethodProfileReport(
                    CounterName: $"{Owner.ClassName}.{methodName}",
                    TotalTimes: times,
                    MeanDuration: TimeSpan.FromSeconds(mean_sec),
                    StandardDeviationOfDuration: double.IsNaN(sd_sec) ? null : TimeSpan.FromSeconds(sd_sec),
                    FoundCheckpoints: foundCheckpoints.ToImmutable(),
                    CodePathSummaries: codePathSummaries.ToImmutable(),
                    Histories: histories.ToImmutable());
            }


            private class CodePathResult(CheckpointTransitionKey key, int startCheckpointSortKey, int endCheckpointSortKey)
            {
                public CheckpointTransitionKey Key { get; } = key;
                public int StartCheckpointSortKey { get; } = startCheckpointSortKey;
                public int EndCheckpointSortKey { get; } = endCheckpointSortKey;
                private WelfordStatistics _durations;

                public void IncrementResult(double duration_sec) =>
                    _durations.IncrementResult(duration_sec);

                public CheckpointTransitionProfileReport CreateSummary()
                {
                    var (times, mean_sec, sd_sec) = _durations;
                    return new(Key, times, TimeSpan.FromSeconds(mean_sec), double.IsNaN(sd_sec) ? null : TimeSpan.FromSeconds(sd_sec));
                }
            }
        }
    }
}