namespace PathBench;

public partial class CodePathCounter
{
    private class MethodCounter(CodePathCounter owner, string? methodName = null)
    {
        private readonly Lock _lockToken = new();
        private readonly Queue<InvocationCounter_> _RecentHistory = new(owner._RecentHistoryCacheSize);
        private readonly SortedList<double, InvocationCounter_> _WorstHistory = new(owner._WorstHistoryCacheSize);

        private long _invocationCount = 0;
        private long _terminatedInvocationCount = 0;
        private double _meanDuration_sec = 0;
        private double _sumSquaredDeviationOfDuration_sec2 = 0;

        public CodePathCounter Owner => owner;

        public InvocationCounter StartMeasurement(object? argumentsExpressionProvider = null)
        {
            var id = Interlocked.Increment(ref _invocationCount);
            var invocation = new InvocationCounter_(
                this,
                methodName,
                id,
                argumentsExpressionProvider);
            return invocation;
        }


        public void TerminateInvocation(InvocationCounter_ invocation)
        {
            // Implementation omitted for brevity.
            using (_lockToken.EnterScope())
            {
                double x = (double)invocation.Duration / Owner.TimeProvider.TimestampFrequency;
                ref var n = ref _terminatedInvocationCount;
                ref var mu = ref _meanDuration_sec;
                ref var ss = ref _sumSquaredDeviationOfDuration_sec2;
                ss = ss + n / (n + 1.0) * (x - mu) * (x - mu);
                mu = mu + (x - mu) / (n + 1.0);
                n++;

                _RecentHistory.Enqueue(invocation);
                if (_RecentHistory.Count > Owner._RecentHistoryCacheSize)
                {
                    _RecentHistory.Dequeue();
                }
                if (x < (_WorstHistory.Values.LastOrDefault()?.Duration ?? double.PositiveInfinity))
                {
                    _WorstHistory.Add(x, invocation);
                    if (_WorstHistory.Count > Owner._WorstHistoryCacheSize)
                    {
                        _WorstHistory.RemoveAt(Owner._WorstHistoryCacheSize - 1);
                    }
                }
            }
        }

        public (TimeSpan meanDuration, TimeSpan standardDeviationOfDuration) CalculateStatistics()
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
}
