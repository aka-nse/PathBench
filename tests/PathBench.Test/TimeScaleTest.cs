namespace PathBench.Test;

public class TimeScaleTest
{
    public static TheoryData<PreciseDuration[], TimeScale> GetBestTimeScaleForTestCase()
    {
        static PreciseDuration f(double sec) => PreciseDuration.FromSeconds(sec);

        return new()
        {
            { new PreciseDuration[] { f(1), f(2) }, TimeScale.Seconds },
            { new PreciseDuration[] { f(0.1), f(0.2) }, TimeScale.Seconds },
            { new PreciseDuration[] { f(0.1e-3), f(0.2e-3) }, TimeScale.Milliseconds },
            { new PreciseDuration[] { f(0.1e-6), f(0.2e-6) }, TimeScale.Microseconds },
            { new PreciseDuration[] { f(0.1e-9), f(0.2e-9) }, TimeScale.Nanoseconds },
        };
    }

    [Theory, MemberData(nameof(GetBestTimeScaleForTestCase))]
    public void GetBestTimeScaleFor(PreciseDuration[] timeSpans, TimeScale expectedTimeScale)
    {
        var actualTimeScale = TimeScaleExtensions.GetBestTimeScaleFor(timeSpans);
        Assert.Equal(expectedTimeScale, actualTimeScale);
    }
}
