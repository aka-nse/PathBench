// See https://aka.ms/new-console-template for more information
using PathBench;

await SampleClass.InvokeTest();
Console.WriteLine("Hello, World!");


static class SampleClass
{
    public static readonly CodePathCounter _Counter = new();

    public static async Task InvokeTest()
    {
        var random = new Random(123456789);
        for(var i = 0; i < 200; ++i)
        {
            Console.Write($"\r        \r{i}");
            await SimulatedWork(random.Next(0, 20));
        }
    }

    private static async Task SimulatedWork(int seed)
    {
        using var counter = _Counter.StartMeasurement(argumentsExpressionProvider: new { seed });
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