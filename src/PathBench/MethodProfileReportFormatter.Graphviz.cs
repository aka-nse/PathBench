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

        public override void Format(MethodProfileReport report, TextWriter writer)
        {
            writer.WriteLine($$"""
                digraph {{Sanitize(report.CounterName)}} {
                    graph [
                        fontname = "{{_fontName}}",
                        label = "{{report.CounterName}}",
                    ];
                    node [
                        fontname = "{{_fontName}}",
                        shape = box,
                    ];
                    edge [
                        fontname = "{{_fontName}}",
                    ];
                """);

            var checkpoints = new Dictionary<string, string>();
            foreach(var (key, transition) in report.CodePathSummaries)
            {
                if(!checkpoints.TryGetValue(key.StartCheckpoint, out var sanitizedStart))
                {
                    checkpoints.Add(key.StartCheckpoint, sanitizedStart = Sanitize(key.StartCheckpoint));
                }
                if(!checkpoints.TryGetValue(key.EndCheckpoint, out var sanitizedEnd))
                {
                    checkpoints.Add(key.EndCheckpoint, sanitizedEnd = Sanitize(key.EndCheckpoint));
                }
                writer.WriteLine("""
                        {0} -> {1} [label = "{2} times\n{3} msec"]
                    """,
                    sanitizedStart,
                    sanitizedEnd,
                    transition.TotalTimes,
                    transition.MeanDuration.TotalMilliseconds);
            }

            foreach(var (raw, sanitized) in checkpoints)
            {
                writer.WriteLine($$"""
                        {{sanitized}} [label = "{{raw}}"]
                    """);
            }

            writer.WriteLine("""
                }
                """);
        }


        private static string Sanitize(string name)
        {
            var sb = new StringBuilder();
            sb.Append("__");
            foreach(var c in name.EnumerateRunes())
            {
                switch(c.Value)
                {
                case >= 0x30 and < 0x3A:
                case >= 0x41 and < 0x5B:
                case >= 0x61 and < 0x7B:
                    sb.Append(c);
                    continue;
                default:
                    sb.Append($"_u{(uint)c.Value:X04}_");
                    continue;
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
}

/// <summary>
/// Provides configuration options for formatting styles in Graphviz output.
/// </summary>
public class GraphvizStyleFormatterOptions : IGraphvizStyleFormatterOptions
{
    private sealed class Default_ : IGraphvizStyleFormatterOptions
    {
        public string FontName => "Monospace";
    }
    internal static IGraphvizStyleFormatterOptions Default { get; } = new Default_();

    /// <inheritdoc />
    public string FontName { get; set; } = Default.FontName;
}