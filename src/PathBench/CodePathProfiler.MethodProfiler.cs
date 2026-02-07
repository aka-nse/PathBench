using System.Collections.Immutable;

namespace PathBench;

partial class CodePathProfiler
{
    private class MethodProfiler(CodePathProfiler owner, string? methodName = null)
    {
        private record struct WorstHistoryKey(double DurationSec, long InvocationId);
        private sealed class WorstHistoryKeyComparer : IComparer<WorstHistoryKey>
        {
            public static WorstHistoryKeyComparer Instance { get; } = new();

            public int Compare(WorstHistoryKey x, WorstHistoryKey y)
            {
                var c = x.DurationSec.CompareTo(y.DurationSec);
                if (c != 0) return c;
                return x.InvocationId.CompareTo(y.InvocationId);
            }
        }

        private readonly Lock _lockToken = new();
        private readonly Dictionary<CheckpointTransitionKey, CodePathResult> _CodePathResults = new();
        private readonly Queue<InvocationProfiler_> _RecentHistory = new(owner._recentHistoryCacheSize);
        private readonly SortedList<WorstHistoryKey, InvocationProfiler_> _WorstHistory = new(owner._worstHistoryCacheSize, WorstHistoryKeyComparer.Instance);

        private long _invocationCount = 0;
        private WelfordStatistics _overallDurations;

        public CodePathProfiler Owner => owner;
        public string Name { get; } = $"{owner.ClassName}.{methodName}";


        public InvocationProfiler StartMeasurement(object? argumentsExpressionProvider = null)
        {
            var id = Interlocked.Increment(ref _invocationCount);
            var invocation = new InvocationProfiler_(
                this,
                methodName,
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
                foreach(var (start, end) in invocation.Checkpoints.Zip(invocation.Checkpoints.Skip(1)))
                {
                    var key = new CheckpointTransitionKey(start.Name, end.Name);
                    var y = (double)(end.DurationTimestamp - start.DurationTimestamp) / Owner.TimeProvider.TimestampFrequency;
                    if (!_CodePathResults.TryGetValue(key, out var result))
                    {
                        _CodePathResults.Add(key, result = new(key, start.SortKey, end.SortKey));
                    }
                    result.IncrementResult(y);
                }

                // update recent history
                _RecentHistory.Enqueue(invocation);
                if (_RecentHistory.Count > Owner._recentHistoryCacheSize)
                {
                    _RecentHistory.Dequeue();
                }

                // update worst history
                if (x < (_WorstHistory.Values.LastOrDefault()?.Duration ?? double.PositiveInfinity))
                {
                    _WorstHistory.Add(new(x, invocation.InvocationIndex), invocation);
                    if (_WorstHistory.Count > Owner._worstHistoryCacheSize)
                    {
                        _WorstHistory.RemoveAt(Owner._worstHistoryCacheSize - 1);
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
            var foundCheckpoints =ImmutableDictionary.CreateBuilder<string, CheckpointMetadata>();
            var codePathSummaries = ImmutableDictionary.CreateBuilder<CheckpointTransitionKey, CheckpointTransitionProfileReport>();
            using (_lockToken.EnterScope())
            {
                (times, mean_sec, sd_sec) = _overallDurations;
                foreach(var (key, result) in _CodePathResults)
                {
                    foundCheckpoints.TryAdd(key.StartCheckpoint, new CheckpointMetadata(key.StartCheckpoint, result.StartCheckpointSortKey));
                    foundCheckpoints.TryAdd(key.EndCheckpoint, new CheckpointMetadata(key.EndCheckpoint, result.EndCheckpointSortKey));
                    codePathSummaries.Add(key, result.CreateSummary());
                }
                recentHistory = [.. _RecentHistory];
                worstHistory = [.. _WorstHistory.Values];
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
