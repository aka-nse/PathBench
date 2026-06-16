using System.Text;

namespace PathBench;

partial class MethodProfileReportFormatter
{
    /// <summary>
    /// Gets the graphviz visualization style formatter with default settings.
    /// </summary>
    public static MethodProfileReportFormatter DefaultGraphvizStyle { get; } =
        new GraphvizStyle_(GraphvizStyleFormatterOptions.Default);

    /// <summary>
    /// Creates a graphviz visualization style formatter with the specified options.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static MethodProfileReportFormatter CreateGraphvizStyle(
        GraphvizStyleFormatterOptions? options = null) =>
        new GraphvizStyle_(options ?? GraphvizStyleFormatterOptions.Default);

    private sealed class GraphvizStyle_(IGraphvizStyleFormatterOptions options)
        : MethodProfileReportFormatter
    {
        private readonly string _fontName = options.FontName;
        private readonly TimeScale _timeScale = options.TimeScale switch
        {
            TimeScale.Auto or
            TimeScale.Nanoseconds or
            TimeScale.Microseconds or
            TimeScale.Milliseconds => options.TimeScale,
            _ => throw new ArgumentOutOfRangeException(nameof(options.TimeScale)),
        };

        public override void Format(MethodProfileReport report, TextWriter writer)
        {
            var adjustedTimeScale =
                _timeScale == TimeScale.Auto
                ? TimeScale.GetBestTimeScaleFor(report.CodePathSummaries.Values.Select(static x => x.MeanDuration))
                : _timeScale;

            writer.WriteLine($$"""
                digraph {{SanitizeIdentifier(report.CounterName)}} {
                    graph [
                        fontname = "{{_fontName}}",
                        label = "{{SanitizeLabel(report.CounterName)}}",
                    ];
                    node [
                        fontname = "{{_fontName}}",
                        shape = "box",
                    ];
                    edge [
                        fontname = "{{_fontName}}",
                    ];
                """);

            var checkpointIdentifiers = report.FoundCheckpoints.Values
                .ToDictionary(static c => c.Name, static c => SanitizeIdentifier(c.Name));
            foreach(var checkpoint in report.FoundCheckpoints.Values.OrderBy(static c => c.SortKey))
            {
                var identifier = checkpointIdentifiers[checkpoint.Name];
                var label = SanitizeLabel(checkpoint.Name);
                writer.WriteLine($$"""
                        {{identifier}} [label = "{{label}}"]
                    """);
            }
            foreach (var (key, transition) in report.CodePathSummaries)
            {
                var startIdentifier = checkpointIdentifiers[key.StartCheckpoint];
                var endIdentifier = checkpointIdentifiers[key.EndCheckpoint];
                writer.WriteLine("""
                        {0} -> {1} [label = "{2} times\n{3}"]
                    """,
                    startIdentifier,
                    endIdentifier,
                    transition.TotalTimes,
                    transition.MeanDuration.ToString(adjustedTimeScale));
            }
            writer.WriteLine("""
                }
                """);
        }


        public static string SanitizeIdentifier(string name)
        {
            var sb = new StringBuilder();
            sb.Append("__");
            foreach (var c in name)
            {
                switch (c)
                {
                case >= (char)0x30 and < (char)0x3A:
                case >= (char)0x41 and < (char)0x5B:
                case >= (char)0x61 and < (char)0x7B:
                    sb.Append(c);
                    continue;
                default:
                    sb.Append($"_u{(uint)c:X04}_");
                    continue;
                }
            }
            return sb.ToString();
        }

        public static string SanitizeLabel(string name)
        {
            var sb = new StringBuilder(name.Length * 2);
            foreach (var c in name)
            {
                switch (c)
                {
                case '\n':
                    sb.Append(@"\n");
                    break;
                case '\t':
                    sb.Append(@"\t");
                    break;
                case '\\':
                    sb.Append(@"\\");
                    break;
                case '"':
                    sb.Append("\\\"");
                    break;
                default:
                    sb.Append(c);
                    break;
                }
            }
            return sb.ToString();
        }
    }
}


/// <summary>
/// Provides configuration options for formatting styles in Graphviz output.
/// </summary>
public interface IGraphvizStyleFormatterOptions
{
    /// <summary>
    /// Specifies the font name to use in the graphviz output.
    /// </summary>
    public string FontName { get; }

    /// <summary>
    /// Specifies the time scale to display measurement results.
    /// </summary>
    public TimeScale TimeScale { get; }
}

/// <summary>
/// Provides configuration options for formatting styles in Graphviz output.
/// </summary>
public class GraphvizStyleFormatterOptions : IGraphvizStyleFormatterOptions
{
    private sealed class Default_ : IGraphvizStyleFormatterOptions
    {
        public string FontName => "Monospace";
        public TimeScale TimeScale => TimeScale.Auto;
    }
    internal static IGraphvizStyleFormatterOptions Default { get; } = new Default_();

    /// <inheritdoc />
    public string FontName { get; set; } = Default.FontName;

    /// <inheritdoc />
    public TimeScale TimeScale { get; set; } = Default.TimeScale;
}