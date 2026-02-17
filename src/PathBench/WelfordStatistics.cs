namespace PathBench;

internal struct WelfordStatistics()
{
    private long _n;
    private double _mu;
    private double _ss;
    private double _max = double.NegativeInfinity;
    private double _min = double.PositiveInfinity;

    public void IncrementResult(double x)
    {
        var n = _n;
        var mu = _mu;
        var ss = _ss;
        _n = n + 1;
        _mu = mu + (x - mu) / _n;
        _ss = ss + (x - mu) * (x - _mu);
        _max = Math.Max(_max, x);
        _min = Math.Min(_min, x);
    }

    public readonly void Deconstruct(out long n, out double mean, out double sd, out double max, out double min)
    {
        n = _n;
        mean = _mu;
        sd = n > 1
            ? Math.Sqrt(_ss / (_n - 1))
            : double.NaN;
        max = _max;
        min = _min;
    }
}
