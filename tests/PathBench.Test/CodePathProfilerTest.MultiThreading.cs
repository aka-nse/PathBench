using System.Runtime.CompilerServices;

namespace PathBench.Test;

public partial class CodePathProfilerTest
{
    [Theory, MemberData(nameof(Profile01Data))]
    public void Profile01MultiThreading(Profile01TimeSet[] timeSet)
    {
        if(timeSet.Length == 0)
        {
            return;
        }

        var timeProvider = new FakeTimeProvider();
        var codePathProfiler = CodePathProfiler.Create("SampleClassName", new() { TimeProvider = timeProvider, });
        var threads = new Thread[timeSet.Length];
        for (int i = 0; i < threads.Length; i++)
        {
            var time = timeSet[i];
            threads[i] = new Thread(() =>
            {
                timeProvider.TimestampMicroseconds = time.X0_us;
                var enumerator = TestTarget.ProfileTarget(codePathProfiler).GetEnumerator();
                enumerator.MoveNext();
                timeProvider.TimestampMicroseconds = time.X1_us;
                enumerator.MoveNext();
                timeProvider.TimestampMicroseconds = time.X2_us;
                enumerator.MoveNext();
                timeProvider.TimestampMicroseconds = time.X3_us;
                enumerator.MoveNext();
                timeProvider.TimestampMicroseconds = time.X4_us;
                enumerator.MoveNext();
            });
        }
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i].Start();
        }
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i].Join();
        }
        var statWhole = TestHelpers.CalculateMeanAndSD(timeSet.Select(static x => (x.X4_us - x.X0_us) / 1_000_000.0));
        var statSection_s_1 = TestHelpers.CalculateMeanAndSD(timeSet.Select(static x => (x.X1_us - x.X0_us) / 1_000_000.0));
        var statSection_1_2 = TestHelpers.CalculateMeanAndSD(timeSet.Select(static x => (x.X2_us - x.X1_us) / 1_000_000.0));
        var statSection_2_3 = TestHelpers.CalculateMeanAndSD(timeSet.Select(static x => (x.X3_us - x.X2_us) / 1_000_000.0));
        var statSection_3_e = TestHelpers.CalculateMeanAndSD(timeSet.Select(static x => (x.X4_us - x.X3_us) / 1_000_000.0));
        var reports = codePathProfiler.CreateProfileReports();
        Assert.True(reports.TryGetValue(nameof(TestTarget.ProfileTarget), out var report));
        Assert.Equal($"SampleClassName.{nameof(TestTarget.ProfileTarget)}", report.CounterName);
        Assert.Equal(statWhole.time, report.TotalTimes);
        Assert.Equal(statWhole.mean, report.MeanDuration.TotalSeconds, _meanTolerance);
        Assert.Equal(statWhole.sd, report.StandardDeviationOfDuration.TotalSeconds, _sdTolerance);
        Assert.True(report.CodePathSummaries.TryGetValue(new(CodePathProfiler.StartCheckpointName, "checkpoint1"), out var trn1));
        Assert.True(report.CodePathSummaries.TryGetValue(new("checkpoint1", "checkpoint2"), out var trn2));
        Assert.True(report.CodePathSummaries.TryGetValue(new("checkpoint2", "checkpoint3"), out var trn3));
        Assert.True(report.CodePathSummaries.TryGetValue(new("checkpoint3", CodePathProfiler.EndCheckpointName), out var trn4));
        Assert.Equal(new(CodePathProfiler.StartCheckpointName, "checkpoint1"), trn1.Key);
        Assert.Equal(new("checkpoint1", "checkpoint2"), trn2.Key);
        Assert.Equal(new("checkpoint2", "checkpoint3"), trn3.Key);
        Assert.Equal(new("checkpoint3", CodePathProfiler.EndCheckpointName), trn4.Key);
        Assert.Equal(statSection_s_1.time, trn1.TotalTimes);
        Assert.Equal(statSection_1_2.time, trn2.TotalTimes);
        Assert.Equal(statSection_2_3.time, trn3.TotalTimes);
        Assert.Equal(statSection_3_e.time, trn4.TotalTimes);
        Assert.Equal(statSection_s_1.mean, trn1.MeanDuration.TotalSeconds, _meanTolerance);
        Assert.Equal(statSection_1_2.mean, trn2.MeanDuration.TotalSeconds, _meanTolerance);
        Assert.Equal(statSection_2_3.mean, trn3.MeanDuration.TotalSeconds, _meanTolerance);
        Assert.Equal(statSection_3_e.mean, trn4.MeanDuration.TotalSeconds, _meanTolerance);
        Assert.Equal(statSection_s_1.sd, trn1.StandardDeviationOfDuration.TotalSeconds, _sdTolerance);
        Assert.Equal(statSection_1_2.sd, trn2.StandardDeviationOfDuration.TotalSeconds, _sdTolerance);
        Assert.Equal(statSection_2_3.sd, trn3.StandardDeviationOfDuration.TotalSeconds, _sdTolerance);
        Assert.Equal(statSection_3_e.sd, trn4.StandardDeviationOfDuration.TotalSeconds, _sdTolerance);
    }
}

file static class TestTarget
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static IEnumerable<ValueTuple> ProfileTarget(CodePathProfiler codePathProfiler)
    {
        using var profiler = codePathProfiler.StartMeasurement();
        yield return default;
        profiler.MarkCheckpoint("checkpoint1");
        yield return default;
        profiler.MarkCheckpoint("checkpoint2");
        yield return default;
        profiler.MarkCheckpoint("checkpoint3");
        yield return default;
    }
}
