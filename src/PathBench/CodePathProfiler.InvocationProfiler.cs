namespace PathBench;

partial class CodePathProfiler
{
    private sealed class InvocationProfiler_ : InvocationProfiler
    {
        private MethodProfiler Owner { get; }
        public string? MethodName { get; }
        public DateTimeOffset StartAt { get; }
        public long InvocationIndex { get; }
        public int ManagedThreadId { get; }
        public object? ArgumentsExpressionProvider { get; }
        public long Duration => _endAtTimestamp >= 0 ? _endAtTimestamp - _startAtTimestamp : -1;
        public IReadOnlyList<CheckpointMeasurement> Checkpoints => _checkpoints ?? _freezedCheckpoints!;

        private List<CheckpointMeasurement>? _checkpoints;
        private readonly long _startAtTimestamp;
        private List<CheckpointMeasurement>? _freezedCheckpoints;
        private long _endAtTimestamp = -1;

        public InvocationProfiler_(
            MethodProfiler owner,
            string? methodName,
            long invocationIndex,
            object? argumentsExpressionProvider)
        {
            Owner = owner;
            MethodName = methodName;
            StartAt = Owner.Owner.TimeProvider.GetUtcNow();
            InvocationIndex = invocationIndex;
            ArgumentsExpressionProvider = argumentsExpressionProvider;
            ManagedThreadId = Environment.CurrentManagedThreadId;
            _startAtTimestamp = owner.Owner.TimeProvider.GetTimestamp();

            _checkpoints = [];
            MarkCheckpoint(StartCheckpointName, null);
        }

        public override void Dispose()
        {
            _endAtTimestamp = Owner.Owner.TimeProvider.GetTimestamp();
            MarkCheckpoint(EndCheckpointName, _endAtTimestamp, null);
            (_freezedCheckpoints, _checkpoints) = (_checkpoints, null);
            Owner.TerminateInvocation(this);
        }

        public override void MarkCheckpoint(string name, object? noteProvider = null) =>
            MarkCheckpoint(name, Owner.Owner.TimeProvider.GetTimestamp(), noteProvider);

        internal protected override InvocationMeasurementReport CreateMeasurementReport()
        {
            var path = Checkpoints.Zip(Checkpoints.Skip(1), static (start, end) =>
                new CheckpointTransitionMeasurementReport(
                    Key: new CheckpointTransitionKey(start.Name, end.Name),
                    Note: start.NoteProvider?.ToString(),
                    Duration: TimeSpan.FromTicks(end.DurationTimestamp - start.DurationTimestamp)));
            return new(
                CounterName: Owner.Name,
                InvocationId: InvocationIndex,
                StartAt: StartAt,
                ManagedThreadId: ManagedThreadId,
                ArgumentsExpression: ArgumentsExpressionProvider?.ToString(),
                Duration: TimeSpan.FromTicks(Duration),
                CodePathMeasurements: [.. path]);
        }

        private void MarkCheckpoint(string name, long timestamp, object? noteProvider)
        {
            var checkpoints = _checkpoints ?? throw new ObjectDisposedException(null);
            checkpoints.Add(new(name, noteProvider, timestamp));
        }
    }

    private readonly record struct CheckpointMeasurement(
        string Name,
        object? NoteProvider,
        long DurationTimestamp);
}