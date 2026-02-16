using System.Diagnostics.CodeAnalysis;

namespace PathBench.Test;

public class CheckpointTransitionProfileReportComparer
    : IEqualityComparer<CheckpointTransitionProfileReport>
{
    public static CheckpointTransitionProfileReportComparer Instance { get; } = new();

    public bool Equals(CheckpointTransitionProfileReport? x, CheckpointTransitionProfileReport? y)
    {
        switch ((x, y))
        {
        case (null, null):
            return true;
        case (null, _):
        case (_, null):
            return false;
        default:
            break;
        }
        if (x.Key != y.Key)
        {
            return false;
        }
        if (x.TotalTimes != y.TotalTimes)
        {
            return false;
        }
        if(!RealComparer.Equals(x.MeanDuration.TotalSeconds, y.MeanDuration.TotalSeconds, 1e-12))
        {
            return false;
        }
        if (!RealComparer.Equals(x.StandardDeviationOfDuration.TotalSeconds, y.StandardDeviationOfDuration.TotalSeconds, 1e-12))
        {
            return false;
        }
        return true;
    }

    public int GetHashCode([DisallowNull] CheckpointTransitionProfileReport obj)
    {
        var hash = new HashCode();
        hash.Add(obj.Key);
        hash.Add(obj.TotalTimes);
        return hash.ToHashCode();
    }
}


internal static class RealComparer
{
    public static bool Equals(double x, double y, double epsilon = 1e-10)
    {
        if (x == y)
        {
            return true;
        }
        if(double.IsNaN(x) && double.IsNaN(y))
        {
            return true;
        }
        if(double.IsPositiveInfinity(x) && double.IsPositiveInfinity(y))
        {
            return true;
        }
        if(double.IsNegativeInfinity(x) && double.IsNegativeInfinity(y))
        {
            return true;
        }
        if (Math.Abs(x - y) < epsilon)
        {
            return true;
        }
        if(Math.Abs(x - y) / (Math.Abs(x) + Math.Abs(y)) < epsilon)
        {
            return true;
        }
        return false;
    }
}