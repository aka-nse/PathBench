namespace PathBench;

internal struct WelfordStatistics
{
    private long _n;
    private double _mu;
    private double _ss;

    public void IncrementResult(double x)
    {
        var n = _n;
        var mu = _mu;
        var ss = _ss;
        _n = n + 1;
        _mu = mu + (x - mu) / _n;
        _ss = ss + (x - mu) * (x - _mu);
    }

    public readonly void Deconstruct(out long n, out double mean, out double sd)
    {
        n = _n;
        mean = _mu;
        sd = n > 1
            ? Math.Sqrt(_ss / (_n - 1))
            : double.NaN;
    }
}
