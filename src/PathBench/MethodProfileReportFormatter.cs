namespace PathBench;

/// <summary>
/// Abstract base class to format <see cref="MethodProfileReport"/> instances.
/// </summary>
public abstract partial class MethodProfileReportFormatter
{
    /// <summary>
    /// When overridden in a derived class, formats the specified report and writes the output to the specified writer.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="writer"></param>
    public abstract void Format(MethodProfileReport report, TextWriter writer);
}
