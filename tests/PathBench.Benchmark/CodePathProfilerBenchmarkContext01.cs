using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace PathBench.Benchmark;

public class CodePathProfilerBenchmarkContext01
{
    [Benchmark]
    public void Profile()
    {
        var codePathProfiler = CodePathProfiler.Create();
        ProfileTarget(codePathProfiler);
    }

    [Benchmark]
    public void Empty()
    {
        var codePathProfiler = CodePathProfiler.Empty;
        ProfileTarget(codePathProfiler);
    }

    [Benchmark]
    public void NegativeControl()
    {
        NegativeControlTarget();
    }

    private static void ProfileTarget(CodePathProfiler codePathProfiler)
    {
        using var profiler = codePathProfiler.StartMeasurement();
        DisturbOptimize(0);
    }

    private static void NegativeControlTarget()
    {
        DisturbOptimize(0);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DisturbOptimize(int i)
    {
    }
}
