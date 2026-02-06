namespace PathBench;

public abstract partial class MethodProfileReportFormatter
{
    public abstract void Format(
        MethodProfileReport report,
        IFormatProvider? provider,
        TextWriter writer);
}
