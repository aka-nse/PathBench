namespace PathBench;

partial class CodePathProfiler
{
    partial class Impl_
    {
        private sealed class InvocationProfiler_ : InvocationProfiler
        {
            private MethodProfiler Owner { get; }
            private TimeProvider TimeProvider { get; }
            private string Name { get; }
            public DateTimeOffset StartAt { get; }
            public long InvocationIndex { get; }
            public int ManagedThreadId { get; }
            public object? ArgumentsExpressionProvider { get; }
            public long Duration => _endAtTimestamp >= 0 ? _endAtTimestamp - _startAtTimestamp : -1;
            public List<CheckpointMeasurement> Checkpoints => _checkpoints ?? _freezedCheckpoints!;

            private List<CheckpointMeasurement>? _checkpoints;
            private readonly long _startAtTimestamp;
            private List<CheckpointMeasurement>? _freezedCheckpoints;
            private long _endAtTimestamp = -1;

            public InvocationProfiler_(
                MethodProfiler owner,
                long invocationIndex,
                object? argumentsExpressionProvider)
            {
                Owner = owner;
                Name = owner.Name;
                TimeProvider = owner.Owner.TimeProvider;
                StartAt = TimeProvider.GetUtcNow();
                InvocationIndex = invocationIndex;
                ArgumentsExpressionProvider = argumentsExpressionProvider;
                ManagedThreadId = Environment.CurrentManagedThreadId;
                _startAtTimestamp = owner.Owner.TimeProvider.GetTimestamp();

                _checkpoints = [];
                MarkCheckpoint(StartCheckpointName, int.MinValue, null);
            }

            public override void Dispose()
            {
                Volatile.Write(ref _endAtTimestamp, TimeProvider.GetTimestamp());
                MarkCheckpoint(EndCheckpointName, int.MaxValue, null);
                (_freezedCheckpoints, _checkpoints) = (_checkpoints, null);
                Owner.TerminateInvocation(this);
            }

            public override void MarkCheckpoint(string name, int orderingKey, object? noteProvider = null) =>
                MarkCheckpoint(name, orderingKey, TimeProvider.GetTimestamp(), noteProvider);

            internal protected override InvocationMeasurementReport CreateMeasurementReport()
            {
                var path = Checkpoints.Zip(Checkpoints.Skip(1), static (start, end) =>
                    new CheckpointTransitionMeasurementReport(
                        Key: new CheckpointTransitionKey(start.Name, end.Name),
                        Note: start.NoteProvider?.ToString(),
                        Duration: TimeSpan.FromTicks(end.DurationTimestamp - start.DurationTimestamp)));
                return new(
                    CounterName: Name,
                    InvocationId: InvocationIndex,
                    StartAt: StartAt,
                    ManagedThreadId: ManagedThreadId,
                    ArgumentsExpression: ArgumentsExpressionProvider?.ToString(),
                    Duration: TimeSpan.FromTicks(Duration),
                    CodePathMeasurements: [.. path]);
            }

            private void MarkCheckpoint(string name, int sortKey, long timestamp, object? noteProvider)
            {
                var checkpoints = _checkpoints ?? throw new ObjectDisposedException(null);
                checkpoints.Add(new(name, sortKey, noteProvider, timestamp));
            }
        }

        private record class CheckpointMeasurement(
            string Name,
            int SortKey,
            object? NoteProvider,
            long DurationTimestamp);
    }
}