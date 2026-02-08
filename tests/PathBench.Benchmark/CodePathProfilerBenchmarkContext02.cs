using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace PathBench.Benchmark;

public class CodePathProfilerBenchmarkContext02
{
    private const int _iterationCount = 1000;

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
        for (var i = 0; i < _iterationCount; ++i)
        {
            profiler.MarkCheckpoint("LoopBegin", i);
            if (i % 2 == 0)
            {
                profiler.MarkCheckpoint("EvenBranch");
                DisturbOptimize(0);
            }
            else
            {
                profiler.MarkCheckpoint("OddBranch");
                DisturbOptimize(1);
            }
        }
    }

    private static void NegativeControlTarget()
    {
        for (var i = 0; i < _iterationCount; ++i)
        {
            if (i % 2 == 0)
            {
                DisturbOptimize(0);
            }
            else
            {
                DisturbOptimize(1);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DisturbOptimize(int i)
    {
    }
}
