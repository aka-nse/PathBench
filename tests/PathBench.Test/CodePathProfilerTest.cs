using System.Runtime.CompilerServices;

namespace PathBench.Test;

public class CodePathProfilerTest
{
    public record Profile01TimeSet(long X0_us, long X1_us, long X2_us, long X3_us, long X4_us);

    public static TheoryData<Profile01TimeSet[]> Profile01Data() =>
        [
            [],
            [
                new(0, 0, 0, 0, 0),
            ],
            [
                new(0, 1000, 2000, 3000, 4000),
                new(0, 1000, 2000, 3000, 4000),
                new(0, 1000, 2000, 3000, 4000),
            ],
            [
                new(0, 1000 +  0, 2000 +  0, 3000 +  0, 4000 +  0),
                new(0, 1000 + 10, 2000 + 20, 3000 + 30, 4000 + 40),
                new(0, 1000 - 10, 2000 - 20, 3000 - 30, 4000 - 40),
                new(0, 1000 + 20, 2000 + 40, 3000 + 60, 4000 + 80),
                new(0, 1000 - 20, 2000 - 40, 3000 - 60, 4000 - 80),
                new(0, 1000 + 10, 2000 + 20, 3000 + 30, 4000 + 40),
                new(0, 1000 - 10, 2000 - 20, 3000 - 30, 4000 - 40),
                new(0, 1000 + 20, 2000 + 40, 3000 + 60, 4000 + 80),
                new(0, 1000 - 20, 2000 - 40, 3000 - 60, 4000 - 80),
            ],
        ];

    [Theory, MemberData(nameof(Profile01Data))]
    public void Profile01(Profile01TimeSet[] timeSet)
    {
        const double meanTolerance = 1e-6;
        const double sdTolerance = 1e-4;

        var timeProvider = new FakeTimeProvider();
        var codePathProfiler = CodePathProfiler.Create("SampleClassName", new() { TimeProvider = timeProvider, });
        for(var k = 0; k < timeSet.Length; ++k)
        {
            var time = timeSet[k];
            timeProvider.TimestampMicroseconds = time.X0_us;
            var enumerator = ProfileTarget(codePathProfiler).GetEnumerator();
            enumerator.MoveNext();
            timeProvider.TimestampMicroseconds = time.X1_us;
            enumerator.MoveNext();
            timeProvider.TimestampMicroseconds = time.X2_us;
            enumerator.MoveNext();
            timeProvider.TimestampMicroseconds = time.X3_us;
            enumerator.MoveNext();
            timeProvider.TimestampMicroseconds = time.X4_us;
            enumerator.MoveNext();

            var statWhole = TestHelpers.CalculateMeanAndSD(timeSet.Take(k + 1).Select(static x => (x.X4_us - x.X0_us) / 1_000_000.0));
            var statSection_s_1 = TestHelpers.CalculateMeanAndSD(timeSet.Take(k + 1).Select(static x => (x.X1_us - x.X0_us) / 1_000_000.0));
            var statSection_1_2 = TestHelpers.CalculateMeanAndSD(timeSet.Take(k + 1).Select(static x => (x.X2_us - x.X1_us) / 1_000_000.0));
            var statSection_2_3 = TestHelpers.CalculateMeanAndSD(timeSet.Take(k + 1).Select(static x => (x.X3_us - x.X2_us) / 1_000_000.0));
            var statSection_3_e = TestHelpers.CalculateMeanAndSD(timeSet.Take(k + 1).Select(static x => (x.X4_us - x.X3_us) / 1_000_000.0));
            var reports = codePathProfiler.CreateProfileReports();
            Assert.True(reports.TryGetValue(nameof(ProfileTarget), out var report));
            Assert.Equal($"SampleClassName.{nameof(ProfileTarget)}", report.CounterName);
            Assert.Equal(statWhole.time, report.TotalTimes);
            Assert.Equal(statWhole.mean, report.MeanDuration.TotalSeconds, meanTolerance);
            Assert.Equal(statWhole.sd, report.StandardDeviationOfDuration?.TotalSeconds ?? double.NaN, sdTolerance);
            Assert.True(report.CodePathSummaries.TryGetValue(new (CodePathProfiler.StartCheckpointName, "checkpoint1"), out var trn1));
            Assert.True(report.CodePathSummaries.TryGetValue(new ("checkpoint1", "checkpoint2"), out var trn2));
            Assert.True(report.CodePathSummaries.TryGetValue(new ("checkpoint2", "checkpoint3"), out var trn3));
            Assert.True(report.CodePathSummaries.TryGetValue(new ("checkpoint3", CodePathProfiler.EndCheckpointName), out var trn4));
            Assert.Equal(new(CodePathProfiler.StartCheckpointName, "checkpoint1"), trn1.Key);
            Assert.Equal(new("checkpoint1", "checkpoint2"), trn2.Key);
            Assert.Equal(new("checkpoint2", "checkpoint3"), trn3.Key);
            Assert.Equal(new("checkpoint3", CodePathProfiler.EndCheckpointName), trn4.Key);
            Assert.Equal(statSection_s_1.time, trn1.TotalTimes);
            Assert.Equal(statSection_1_2.time, trn2.TotalTimes);
            Assert.Equal(statSection_2_3.time, trn3.TotalTimes);
            Assert.Equal(statSection_3_e.time, trn4.TotalTimes);
            Assert.Equal(statSection_s_1.mean, trn1.MeanDuration.TotalSeconds, meanTolerance);
            Assert.Equal(statSection_1_2.mean, trn2.MeanDuration.TotalSeconds, meanTolerance);
            Assert.Equal(statSection_2_3.mean, trn3.MeanDuration.TotalSeconds, meanTolerance);
            Assert.Equal(statSection_3_e.mean, trn4.MeanDuration.TotalSeconds, meanTolerance);
            Assert.Equal(statSection_s_1.sd, trn1.StandardDeviationOfDuration?.TotalSeconds ?? double.NaN, sdTolerance);
            Assert.Equal(statSection_1_2.sd, trn2.StandardDeviationOfDuration?.TotalSeconds ?? double.NaN, sdTolerance);
            Assert.Equal(statSection_2_3.sd, trn3.StandardDeviationOfDuration?.TotalSeconds ?? double.NaN, sdTolerance);
            Assert.Equal(statSection_3_e.sd, trn4.StandardDeviationOfDuration?.TotalSeconds ?? double.NaN, sdTolerance);
        }
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static IEnumerable<ValueTuple> ProfileTarget(CodePathProfiler codePathProfiler)
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
