// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;
using PathBench;


SampleClass.InvokeTest();


static class SampleClass
{
    public static readonly CodePathProfiler _Profiler = CodePathProfiler.Create();

    public static void InvokeTest()
    {
        for (var i = 0; i < 1000; ++i)
        {
            Console.Write($"\r        \r{i}");
            SimulatedWork(i);
        }
        Console.WriteLine();

        var reports = _Profiler.CreateProfileReports();
        Console.WriteLine(reports[nameof(SimulatedWork)]);
        Console.WriteLine();
        var sw = new StringWriter();
        MethodProfileReportFormatter.DefaultGraphvizStyle.Format(
            reports[nameof(SimulatedWork)],
            writer: sw);
        Console.WriteLine(sw.ToString());
    }

    private static void SimulatedWork(int seed)
    {
        using var counter = _Profiler.StartMeasurement(argumentsExpressionProvider: $"seed={seed}");
        if (seed % 2 == 0)
        {
            counter.MarkCheckpoint("EvenSeed");
            DisturbOptimize(2);
        }
        else
        {
            counter.MarkCheckpoint("OddSeed");
            DisturbOptimize(0);
        }
        for (var i = 0; i < seed; ++i)
        {
            counter.MarkCheckpoint("LoopBegin", i);
            if (seed % 3 == 0)
            {
                counter.MarkCheckpoint("DivisibleBy3");
                DisturbOptimize(3);
            }
            if (seed % 5 == 0)
            {
                counter.MarkCheckpoint("DivisibleBy5");
                DisturbOptimize(5);
            }
            if (seed % 7 == 0)
            {
                counter.MarkCheckpoint("DivisibleBy7");
                DisturbOptimize(7);
            }
            counter.MarkCheckpoint("LoopEnd");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DisturbOptimize(int value)
    {
        Thread.SpinWait(value * 100);
    }
}