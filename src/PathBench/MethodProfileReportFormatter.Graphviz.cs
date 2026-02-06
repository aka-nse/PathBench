using System.Text;

namespace PathBench;

partial class MethodProfileReportFormatter
{
#warning implement font customization
    public static MethodProfileReportFormatter GraphvizStyle { get; } = new GraphvizStyle_();

    private sealed class GraphvizStyle_ : MethodProfileReportFormatter
    {
        private readonly string _fontName = "Consolas";

        public override void Format(
            MethodProfileReport report,
            IFormatProvider? provider,
            TextWriter writer)
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
                writer.WriteLine($$"""
                        {{sanitizedStart}} -> {{sanitizedEnd}} [label = "{{transition.TotalTimes}} times\n{{transition.MeanDuration.TotalMilliseconds}} msec"]
                    """);
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
                    sb.Append($"_U{(uint)c.Value:X02}_");
                    continue;
                }
            }
            return sb.ToString();
        }
    }
}
