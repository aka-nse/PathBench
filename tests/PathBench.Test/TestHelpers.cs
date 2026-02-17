namespace PathBench.Test;

internal static class TestHelpers
{
    public static (long time, double mean, double sd) CalculateMeanAndSD(IEnumerable<double> values)
    {
        var time = values.Count();
        var mean = values.Average();
        var sd = time > 1
            ? Math.Sqrt(values.Select(x => (x - mean) * (x - mean)).Sum() / (time - 1.0))
            : double.NaN;
        return (time, mean, sd);
    }
}
