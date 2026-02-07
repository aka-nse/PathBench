namespace PathBench;

/// <summary>
/// Provides an abstract base for profiling and recording invocation checkpoints and measurements within an operation.
/// </summary>
/// <remarks>
/// Use this class to track performance or timing information by marking checkpoints and creating measurements during the execution of an operation.
/// Implementations may vary in how profiling data is collected and reported.
/// </remarks>
public abstract class InvocationProfiler : IDisposable
{
    /// <inheritdoc />
    public abstract void Dispose();

    /// <summary>
    /// Marks a checkpoint in the invocation.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="noteProvider"></param>
    public abstract void MarkCheckpoint(string name, object? noteProvider = null);

    /// <summary>
    /// Creates an invocation measurement summary.
    /// </summary>
    /// <returns></returns>
    internal protected abstract InvocationMeasurementReport CreateMeasurementReport();
}
