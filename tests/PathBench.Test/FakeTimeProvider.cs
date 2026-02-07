namespace PathBench.Test;

public class FakeTimeProvider : TimeProvider
{
    public long TimestampMicroseconds { get; set; } = 0;

    public override long TimestampFrequency => 1_000_000;
    public override long GetTimestamp() => TimestampMicroseconds;
}
