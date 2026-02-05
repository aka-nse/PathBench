namespace PathBench;

partial class CodePathCounter
{
    private sealed class InvocationCounter_ : InvocationCounter
    {
        private MethodCounter Owner { get; }
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

        public InvocationCounter_(
            MethodCounter owner,
            string? methodName,
            long invocationIndex,
            object? argumentsExpressionProvider)
        {
            Owner = owner;
            MethodName = methodName;
            ArgumentsExpressionProvider = argumentsExpressionProvider;
            StartAt = DateTimeOffset.UtcNow;
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