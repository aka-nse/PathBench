namespace PathBench.Test;

public class PreciseDurationTest
{
#pragma warning disable IDE1006

    private const long NaN = PreciseDuration.NaNValue;
    private const long PInf = PreciseDuration.PositiveInfinityValue;
    private const long NInf = PreciseDuration.NegativeInfinityValue;
    private const long Max = PreciseDuration.MaxValue;
    private const long Min = PreciseDuration.MinValue;


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
}
