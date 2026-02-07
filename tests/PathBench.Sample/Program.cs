// See https://aka.ms/new-console-template for more information
using PathBench;

await SampleClass.InvokeTest();
Console.WriteLine("Hello, World!");


static class SampleClass
{
    public static readonly CodePathProfiler _Profiler = new();

    public static async Task InvokeTest()
    {
        for (var i = 0; i < 200; ++i)
        {
            Console.Write($"\r        \r{i}");
            await SimulatedWork(i);
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

    private static async Task SimulatedWork(int seed)
    {
        using var counter = _Profiler.StartMeasurement(argumentsExpressionProvider: $"seed={seed}");
        if (seed % 2 == 0)
        {
            counter.MarkCheckpoint("EvenSeed");
            await Task.Delay(10);
        }
        else
        {
            counter.MarkCheckpoint("OddSeed");
            await Task.Delay(20);
        }
        for (var i = 0; i < seed; ++i)
        {
            counter.MarkCheckpoint("LoopBegin", i);
            if (seed % 3 == 0)
            {
                counter.MarkCheckpoint("DivisibleBy3");
            }
            if (seed % 5 == 0)
            {
                counter.MarkCheckpoint("DivisibleBy5");
            }
            if (seed % 7 == 0)
            {
                counter.MarkCheckpoint("DivisibleBy7");
            }
            counter.MarkCheckpoint("LoopEnd");
        }
    }
}