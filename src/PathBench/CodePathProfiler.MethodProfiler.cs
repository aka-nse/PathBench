using System.Collections.Immutable;

namespace PathBench;

partial class CodePathProfiler
{
    private class MethodProfiler(CodePathProfiler owner, string? methodName = null)
    {
        private readonly Lock _lockToken = new();
        private readonly Dictionary<CheckpointTransitionKey, CodePathResult> _CodePathResults = new();
        private readonly Queue<InvocationProfiler_> _RecentHistory = new(owner._recentHistoryCacheSize);
        private readonly SortedList<double, InvocationProfiler_> _WorstHistory = new(owner._worstHistoryCacheSize);

        private long _invocationCount = 0;
        private long _terminatedInvocationCount = 0;
        private double _meanDuration_sec = 0;
        private double _sumSquaredDeviationOfDuration_sec2 = 0;

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
            // Implementation omitted for brevity.
            using (_lockToken.EnterScope())
            {
                var x = (double)invocation.Duration / Owner.TimeProvider.TimestampFrequency;

                // update overall statistics
                InternalHelpers.CalculateStatistics(
                    x,
                    n: ref _terminatedInvocationCount,
                    mu: ref _meanDuration_sec,
                    ss: ref _sumSquaredDeviationOfDuration_sec2);

                // update code path report
                foreach(var (start, end) in invocation.Checkpoints.Zip(invocation.Checkpoints.Skip(1)))
                {
                    var key = new CheckpointTransitionKey(start.Name, end.Name);
                    var y = (double)(end.DurationTimestamp - start.DurationTimestamp) / Owner.TimeProvider.TimestampFrequency;
                    if (!_CodePathResults.TryGetValue(key, out var result))
                    {
                        _CodePathResults.Add(key, result = new(key));
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
                    _WorstHistory.Add(x, invocation);
                    if (_WorstHistory.Count > Owner._worstHistoryCacheSize)
                    {
                        _WorstHistory.RemoveAt(Owner._worstHistoryCacheSize - 1);
                    }
                }
            }
        }


        public MethodProfileReport CreateReport()
        {
            double mean_sec;
            double sd_sec;
            ImmutableDictionary<CheckpointTransitionKey, CheckPointTransitionProfileReport> codePathSummaries;
            InvocationProfiler_[] recentHistory;
            InvocationProfiler_[] worstHistory;
            using (_lockToken.EnterScope())
            {
                mean_sec = _meanDuration_sec;
                sd_sec = Math.Sqrt(_sumSquaredDeviationOfDuration_sec2 / Math.Max(1, _terminatedInvocationCount - 1));
                codePathSummaries = _CodePathResults.ToImmutableDictionary(
                    static kv => kv.Key,
                    static kv => kv.Value.CreateSummary());
                recentHistory = [.. _RecentHistory];
                worstHistory = [.. _WorstHistory.Values];
            }
            var histories = ImmutableDictionary.CreateBuilder<HistoryType, ImmutableArray<InvocationMeasurement>>();
            histories.Add(HistoryType.Recent, [.. recentHistory.Select(static x => x.CreateMeasurement())]);
            histories.Add(HistoryType.Worst, [.. worstHistory.Select(static x => x.CreateMeasurement())]);
            return new MethodProfileReport(
                CounterName: $"{Owner.ClassName}.{methodName}",
                TotalTimes: _terminatedInvocationCount,
                MeanDuration: TimeSpan.FromSeconds(mean_sec),
                StandardDeviationOfDuration: TimeSpan.FromSeconds(sd_sec),
                CodePathSummaries: codePathSummaries,
                Histories: histories.ToImmutable());
        }


        private class CodePathResult(CheckpointTransitionKey key)
        {
            public CheckpointTransitionKey Key { get; } = key;
            private long _totalTimes;
            private double _meanDuration_sec;
            private double _sumSquaredDeviationOfDuration_sec2;

            public void IncrementResult(double duration_sec) =>
                InternalHelpers.CalculateStatistics(
                    duration_sec,
                    n: ref _totalTimes,
                    mu: ref _meanDuration_sec,
                    ss: ref _sumSquaredDeviationOfDuration_sec2);

            public CheckPointTransitionProfileReport CreateSummary()
            {
                var sd = Math.Sqrt(_sumSquaredDeviationOfDuration_sec2 / Math.Max(1, _totalTimes - 1));
                return new( Key, _totalTimes, TimeSpan.FromSeconds(_meanDuration_sec), TimeSpan.FromSeconds(sd));
            }
        }
    }
}
