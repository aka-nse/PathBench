namespace PathBench;

public abstract class InvocationCounter : IDisposable
{
    public abstract void Dispose();
    public abstract void MarkCheckpoint(string name, object? noteProvider = null);
}
