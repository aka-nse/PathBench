

namespace PathBench;

partial class MethodProfileReportFormatter
{
    public static MethodProfileReportFormatter Default { get; } = new Default_();

    private sealed class Default_ : MethodProfileReportFormatter
    {
        public override void Format(
            MethodProfileReport report,
            IFormatProvider? provider,
            TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
