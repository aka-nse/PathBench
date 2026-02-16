using System.Runtime.CompilerServices;

namespace PathBench.Test;

public class PreciseDurationTest
{
    private static class PrivateAccess
    {
        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DoubleTicks")]
        public extern static double DoubleTicks(in PreciseDuration duration);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "FromScaledValue")]
        public extern static long FromScaledValue(PreciseDuration _, double value, long scale);
    }

#pragma warning disable IDE1006
    private const long NaN = PreciseDuration.NaNTicks;
    private const long PInf = PreciseDuration.PositiveInfinityTicks;
    private const long NInf = PreciseDuration.NegativeInfinityTicks;
    private const long Max = PreciseDuration.MaxTicks;
    private const long Min = PreciseDuration.MinTicks;
#pragma warning restore IDE1006

    [Fact]
    public void Scale()
    {
#pragma warning disable format

        Assert.Equal(1e+0, PreciseDuration.FromSeconds     (1e+0).TotalSeconds, precision: 12);
        Assert.Equal(1e+0, PreciseDuration.FromMilliseconds(1e+3).TotalSeconds, precision: 12);
        Assert.Equal(1e+0, PreciseDuration.FromMicroseconds(1e+6).TotalSeconds, precision: 12);
        Assert.Equal(1e+0, PreciseDuration.FromNanoseconds (1e+9).TotalSeconds, precision: 12);

        Assert.Equal(1e+3, PreciseDuration.FromSeconds     (1e+0).TotalMilliseconds, precision: 12);
        Assert.Equal(1e+3, PreciseDuration.FromMilliseconds(1e+3).TotalMilliseconds, precision: 12);
        Assert.Equal(1e+3, PreciseDuration.FromMicroseconds(1e+6).TotalMilliseconds, precision: 12);
        Assert.Equal(1e+3, PreciseDuration.FromNanoseconds (1e+9).TotalMilliseconds, precision: 12);

        Assert.Equal(1e+6, PreciseDuration.FromSeconds     (1e+0).TotalMicroseconds, precision: 12);
        Assert.Equal(1e+6, PreciseDuration.FromMilliseconds(1e+3).TotalMicroseconds, precision: 12);
        Assert.Equal(1e+6, PreciseDuration.FromMicroseconds(1e+6).TotalMicroseconds, precision: 12);
        Assert.Equal(1e+6, PreciseDuration.FromNanoseconds (1e+9).TotalMicroseconds, precision: 12);

        Assert.Equal(1e+9, PreciseDuration.FromSeconds     (1e+0).TotalNanoseconds, precision: 12);
        Assert.Equal(1e+9, PreciseDuration.FromMilliseconds(1e+3).TotalNanoseconds, precision: 12);
        Assert.Equal(1e+9, PreciseDuration.FromMicroseconds(1e+6).TotalNanoseconds, precision: 12);
        Assert.Equal(1e+9, PreciseDuration.FromNanoseconds (1e+9).TotalNanoseconds, precision: 12);

#pragma warning restore format
    }

    [Fact]
    public void DoubleTicks()
    {
        Assert.Equal(1.0, PrivateAccess.DoubleTicks(new PreciseDuration(1L)));
        Assert.Equal(100.0, PrivateAccess.DoubleTicks(new PreciseDuration(100L)));
        Assert.Equal(-100.0, PrivateAccess.DoubleTicks(new PreciseDuration(-100L)));
        Assert.True(double.IsNaN(PrivateAccess.DoubleTicks(new PreciseDuration(NaN))));
        Assert.True(double.IsPositiveInfinity(PrivateAccess.DoubleTicks(new PreciseDuration(PInf))));
        Assert.True(double.IsNegativeInfinity(PrivateAccess.DoubleTicks(new PreciseDuration(NInf))));
    }

    public static TheoryData<string, string> ToStringTestCase() =>
        new()
        {
            { "0.00", "1.23 sec" },
            { "0.00sec", "1.23 sec" },
            { "0.00msec", "1234.00 msec" },
            { "0.00usec", "1234000.00 usec" },
            { "0.00nsec", "1234000000.00 nsec" },
        };

    [Theory, MemberData(nameof(ToStringTestCase))]
    public void ToStringByFormat(string format, string expected)
    {
        Assert.Equal(expected, PreciseDuration.FromSeconds(1.234).ToString(format, System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ToStringException() =>
        Assert.Throws<FormatException>(() => PreciseDuration.FromSeconds(1.234).ToString((TimeScale)(-1)));

    public static TheoryData<double, long, long> FromScaledValueTestCase() =>
        new()
        {
            { 0.0, 1L, 0L },
            { 1.0, PreciseDuration.TicksPerSecond, PreciseDuration.TicksPerSecond },
            { 10.0, PreciseDuration.TicksPerSecond, 10 * PreciseDuration.TicksPerSecond },
            { (double)Max, 10L, PreciseDuration.PositiveInfinityTicks },
            { (double)Min, 10L, PreciseDuration.NegativeInfinityTicks },
            { double.PositiveInfinity, 1L, PreciseDuration.PositiveInfinityTicks },
            { double.NegativeInfinity, 1L, PreciseDuration.NegativeInfinityTicks },
            { double.NaN, 1L, PreciseDuration.NaNTicks },
        };

    [Theory, MemberData(nameof(FromScaledValueTestCase))]
    public void FromScaledValue(double value, long scale, long expected)
    {
        Assert.Equal(expected, PrivateAccess.FromScaledValue(default, value, scale));
    }


    public static TheoryData<long, long, long> AddTestCase() =>
        new()
        {
#pragma warning disable format
            { +0  , +0  , +0   },
            { +1  , +2  , +3   },
            { -1  , -2  , -3   },
            { -1  , +1  , +0   },
            { PInf, +1  , PInf },
            { NInf, -1  , NInf },
            { PInf, NInf, NaN  },
            { NInf, PInf, NaN  },
            { Max , +1  , PInf },
            { Min , -1  , NInf },
            { NaN , +1  , NaN  },
            { +1  , NaN , NaN  },
#pragma warning restore format
        };

    [Theory, MemberData(nameof(AddTestCase))]
    public void Add(long ticks1, long ticks2, long expectedTicks)
    {
        var duration1 = new PreciseDuration(ticks1);
        var duration2 = new PreciseDuration(ticks2);
        var expectedDuration = new PreciseDuration(expectedTicks);
        Assert.Equal(expectedDuration.Ticks, (duration1 + duration2).Ticks);
    }


    public static TheoryData<long, long, long> SubtractTestCase() =>
        new()
        {
#pragma warning disable format
            { +0  , +0  , +0   },
            { +3  , +2  , +1   },
            { -3  , -2  , -1   },
            { +1  , +1  , +0   },
            { PInf, +1  , PInf },
            { NInf, -1  , NInf },
            { PInf, PInf, NaN  },
            { NInf, NInf, NaN  },
            { Max , -1  , PInf },
            { Min , +1  , NInf },
            { NaN , +1  , NaN  },
            { +1  , NaN , NaN  },
#pragma warning restore format
        };

    [Theory, MemberData(nameof(SubtractTestCase))]
    public void Subtract(long ticks1, long ticks2, long expectedTicks)
    {
        var duration1 = new PreciseDuration(ticks1);
        var duration2 = new PreciseDuration(ticks2);
        var expectedDuration = new PreciseDuration(expectedTicks);
        Assert.Equal(expectedDuration.Ticks, (duration1 - duration2).Ticks);
    }


    public static TheoryData<long, long, bool, bool> EqualityTestCase() =>
        new()
        { //              ==     !=
            { +0  , +0  , true , false },
            { +1  , +1  , true , false },
            { -1  , -1  , true , false },
            { +1  , -1  , false, true  },
            { PInf, PInf, true , false },
            { NInf, NInf, true , false },
            { PInf, NInf, false, true  },
            { NInf, PInf, false, true  },
            { Max , Max , true , false },
            { Min , Min , true , false },
            { NaN , NaN , false, false },
            { +1  , NaN , false, false },
        };

    [Theory, MemberData(nameof(EqualityTestCase))]
    public static void Equal(long ticks1, long ticks2, bool expectedEqual, bool expectedNotEqual)
    {
        var duration1 = new PreciseDuration(ticks1);
        var duration2 = new PreciseDuration(ticks2);
        Assert.Equal(expectedEqual, duration1 == duration2);
        Assert.Equal(expectedNotEqual, duration1 != duration2);
    }


    public static TheoryData<long, long, bool, bool, bool, bool> CompareTestCase() =>
        new()
        { //              <      <=     >      >=
            { +0  , +0  , false,  true,  false,  true  },
            { +1  , +1  , false,  true,  false,  true  },
            { -1  , -1  , false,  true,  false,  true  },
            { +1  , -1  , false,  false, true,   true  },
            { -1  , +1  , true,   true,  false,  false },
            { PInf, PInf, false,  true,  false,  true  },
            { NInf, NInf, false,  true,  false,  true  },
            { PInf, NInf, false,  false, true,   true  },
            { NInf, PInf, true,   true,  false,  false },
            { Max , Max , false,  true,  false,  true  },
            { Min , Min , false,  true,  false,  true  },
            { NaN , NaN , false,  false, false,  false },
            { +1  , NaN , false,  false, false,  false },
        };

    [Theory, MemberData(nameof(CompareTestCase))]
    public static void Compare(long ticks1, long ticks2, bool expectedLess, bool expectedLessEqual, bool expectedGreater, bool expectedGreaterEqual)
    {
        var duration1 = new PreciseDuration(ticks1);
        var duration2 = new PreciseDuration(ticks2);
        Assert.Equal(expectedLess, duration1 < duration2);
        Assert.Equal(expectedLessEqual, duration1 <= duration2);
        Assert.Equal(expectedGreater, duration1 > duration2);
        Assert.Equal(expectedGreaterEqual, duration1 >= duration2);
    }

    [Fact]
    public void ConvertTimeSpan()
    {
        Assert.Equal(TimeSpan.FromSeconds(1), (TimeSpan)PreciseDuration.FromSeconds(1));
        Assert.Equal(PreciseDuration.FromSeconds(1), (PreciseDuration)TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ConvertTimeSpanException()
    {
        Assert.Throws<InvalidCastException>(() => (TimeSpan)PreciseDuration.NaN);
        Assert.Throws<InvalidCastException>(() => (TimeSpan)PreciseDuration.PositiveInfinity);
        Assert.Throws<InvalidCastException>(() => (TimeSpan)PreciseDuration.NegativeInfinity);
        Assert.Throws<OverflowException>(() => (PreciseDuration)TimeSpan.MaxValue);
        Assert.Throws<OverflowException>(() => (PreciseDuration)TimeSpan.MinValue);
    }
}
