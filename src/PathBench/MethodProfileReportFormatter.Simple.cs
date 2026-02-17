

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
            var timeSpans = report.CodePathSummaries.Values.Select(static x => x.MeanDuration);
            var adjustedTimeScale = TimeScale.GetBestTimeScaleFor(timeSpans);

            writer.WriteLine($"<`{report.CounterName}` profile report>");
            writer.WriteLine($"  total invocation   : {report.TotalTimes}");
            writer.WriteLine($"  mean duration      : {getDurationText(adjustedTimeScale, report.MeanDuration, report.StandardDeviationOfDuration)}");
            writer.WriteLine($"  code path summaries:");
            foreach(var pathSummary in report.CodePathSummaries)
            {
                writer.WriteLine($"    {pathSummary.Key}:");
                writer.WriteLine($"      total invocation: {pathSummary.Value.TotalTimes}");
                writer.WriteLine($"      mean duration   : {getDurationText(adjustedTimeScale, pathSummary.Value.MeanDuration, pathSummary.Value.StandardDeviationOfDuration)}");
            }

            string getDurationText(TimeScale scale, PreciseDuration meanDuration, PreciseDuration? standardDeviationOfDuration)
            {
                var mean = meanDuration.ToString(scale);
                var sd = standardDeviationOfDuration.HasValue ? standardDeviationOfDuration.Value.ToString(scale) : "N/A";
                return $"{mean} (SD = {sd})";
            }
        }
    }
}
