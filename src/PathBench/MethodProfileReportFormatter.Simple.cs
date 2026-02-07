

namespace PathBench;

partial class MethodProfileReportFormatter
{
    /// <summary>
    /// Gets the simple formatter.
    /// </summary>
    public static MethodProfileReportFormatter Simple { get; } = new Default_();

    private sealed class Default_ : MethodProfileReportFormatter
    {
        public override void Format(MethodProfileReport report, TextWriter writer)
        {
            writer.WriteLine($"<`{report.CounterName}` profile report>");
            writer.WriteLine($"  total invocation   : {report.TotalTimes}");
            writer.WriteLine($"  mean duration      : {report.MeanDuration.TotalMilliseconds} msec (SD = {report.StandardDeviationOfDuration?.TotalMilliseconds ?? double.NaN})");
            writer.WriteLine($"  code path summaries:");
            foreach(var pathSummary in report.CodePathSummaries)
            {
                writer.WriteLine($"    {pathSummary.Key}:");
                writer.WriteLine($"      total invocation: {pathSummary.Value.TotalTimes}");
                writer.WriteLine($"      mean duration   : {pathSummary.Value.MeanDuration.TotalMilliseconds} msec (SD = {pathSummary.Value.StandardDeviationOfDuration?.TotalMilliseconds ?? double.NaN})");
            }
        }
    }
}
