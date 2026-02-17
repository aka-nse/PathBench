using PathBench;

invokeTest();

static void invokeTest()
{
    for (var i = 0; i < 1000; ++i)
    {
        SampleClass.SimulatedWork(i);
    }

    var reports = SampleClass.Profiler.CreateProfileReports();
    var sw = new StringWriter();
    MethodProfileReportFormatter.DefaultGraphvizStyle.Format(
        reports[nameof(SampleClass.SimulatedWork)],
        writer: sw);
    Console.WriteLine(sw.ToString());
}

static class SampleClass
{
    public static readonly CodePathProfiler Profiler = CodePathProfiler.Create();

    public static void SimulatedWork(int seed)
    {
        using var counter = Profiler.StartMeasurement(argumentsExpressionProvider: $"seed={seed}");
        if (seed % 2 == 0)
        {
            counter.MarkCheckpoint("EvenSeed");
            Wait(2);
        }
        else
        {
            counter.MarkCheckpoint("OddSeed");
            Wait(0);
        }
        for (var i = 0; i < seed; ++i)
        {
            counter.MarkCheckpoint("LoopBegin", i);
            if (seed % 3 == 0)
            {
                counter.MarkCheckpoint("DivisibleBy3");
                Wait(3);
            }
            if (seed % 5 == 0)
            {
                counter.MarkCheckpoint("DivisibleBy5");
                Wait(5);
            }
            if (seed % 7 == 0)
            {
                counter.MarkCheckpoint("DivisibleBy7");
                Wait(7);
            }
            counter.MarkCheckpoint("LoopEnd");
        }
    }

    private static void Wait(int value)
    {
        Thread.SpinWait(value * 100);
    }
}