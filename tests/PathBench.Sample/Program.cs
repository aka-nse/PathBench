// See https://aka.ms/new-console-template for more information
using PathBench;

await SampleClass.InvokeTest();
Console.WriteLine("Hello, World!");


static class SampleClass
{
    public static readonly CodePathProfiler _Profiler = new();

    public static async Task InvokeTest()
    {
        var random = new Random(123456789);
        for(var i = 0; i < 200; ++i)
        {
            Console.Write($"\r        \r{i}");
            await SimulatedWork(random.Next(0, 20));
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
            await Task.Delay(100);
        }
        for(var i = 0; i < seed; ++i)
        {
            counter.MarkCheckpoint("LoopIteration", new { i });
            await Task.Delay(1);
        }
    }
}