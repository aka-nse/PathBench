namespace PathBench.Test;

public class FakeTimeProvider : TimeProvider
{
    private readonly ThreadLocal<long> _timestampMicroseconds = new(() => 0);

    public long TimestampMicroseconds
    {
        get => _timestampMicroseconds.Value;
        set => _timestampMicroseconds.Value = value;
    }

    public override long TimestampFrequency => 1_000_000;
    public override long GetTimestamp() => TimestampMicroseconds;
}
