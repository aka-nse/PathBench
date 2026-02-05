namespace PathBench;

public abstract class InvocationProfiler : IDisposable
{
    public abstract void Dispose();
    public abstract void MarkCheckpoint(string name, object? noteProvider = null);
    public abstract InvocationMeasurement CreateMeasurement();
}
