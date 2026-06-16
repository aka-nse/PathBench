#if NET7_0_OR_GREATER
using System.Numerics;
#endif
using System.Text.RegularExpressions;

namespace PathBench;

/// <summary>
/// Provides pico-order precise duration representation.
/// </summary>
public readonly partial struct PreciseDuration(long ticks)
    : IEquatable<PreciseDuration>
    , IComparable<PreciseDuration>
#if NET7_0_OR_GREATER
    , IComparisonOperators<PreciseDuration, PreciseDuration, bool>
    , IAdditionOperators<PreciseDuration, PreciseDuration, PreciseDuration>
    , ISubtractionOperators<PreciseDuration, PreciseDuration, PreciseDuration>
#endif
    , IFormattable
{
    /// <summary>
    /// Represents a Not-a-Number (NaN) value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long NaNTicks = long.MinValue;

    /// <summary>
    /// Represents positive infinity value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long PositiveInfinityTicks = long.MaxValue;

    /// <summary>
    /// Represents negative infinity value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long NegativeInfinityTicks = long.MinValue + 1;

    /// <summary>
    /// Represents the minimum finite value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long MaxTicks = long.MaxValue - 1;

    /// <summary>
    /// Represents the maximum finite value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long MinTicks = long.MinValue + 2;

    /// <summary>
    /// Ticks per second (1e+12).
    /// </summary>
    public const long TicksPerSecond = 1_000_000_000_000;

    /// <summary>
    /// Ticks per millisecond (1e+9).
    /// </summary>
    public const long TicksPerMillisecond = TicksPerSecond / 1000;

    /// <summary>
    /// Ticks per microsecond (1e+6).
    /// </summary>
    public const long TicksPerMicrosecond = TicksPerSecond / 1_000_000;

    /// <summary>
    /// Ticks per nanosecond (1e+3).
    /// </summary>
    public const long TicksPerNanosecond = TicksPerSecond / 1_000_000_000;

    /// <summary>
    /// Represents a zero duration.
    /// </summary>
    public static readonly PreciseDuration Zero = new(0);

    /// <summary>
    /// Represents a Not-a-Number (NaN) value of <see cref="PreciseDuration"/>.
    /// </summary>
    public static readonly PreciseDuration NaN = new(NaNTicks);

    /// <summary>
    /// Represents positive infinity value of <see cref="PreciseDuration"/>.
    /// </summary>
    public static readonly PreciseDuration PositiveInfinity = new(PositiveInfinityTicks);

    /// <summary>
    /// Represents negative infinity value of <see cref="PreciseDuration"/>.
    /// </summary>
    public static readonly PreciseDuration NegativeInfinity = new(NegativeInfinityTicks);

    /// <summary>
    /// Represents the maximum finite value of <see cref="PreciseDuration"/>.
    /// </summary>
    public static readonly PreciseDuration MaxValue = new(MaxTicks);

    /// <summary>
    /// Represents the minimum finite value of <see cref="PreciseDuration"/>.
    /// </summary>
    public static readonly PreciseDuration MinValue = new(MinTicks);

    /// <summary>
    /// Gets the number of ticks that represent the value of the current TimeSpan structure.
    /// </summary>
    public long Ticks { get; } = ticks;

    /// <summary>
    /// Gets a value indicating whether this instance is not a number (NaN).
    /// </summary>
    public bool IsNaN => Ticks == NaNTicks;

    /// <summary>
    /// Gets a value indicating whether this instance represents positive infinity.
    /// </summary>
    public bool IsPositiveInfinity => Ticks == PositiveInfinityTicks;

    /// <summary>
    /// Gets a value indicating whether this instance represents negative infinity.
    /// </summary>
    public bool IsNegativeInfinity => Ticks == NegativeInfinityTicks;

    /// <summary>
    /// Gets a value indicating whether this instance represents infinity (positive or negative).
    /// </summary>
    public bool IsInfinity => IsPositiveInfinity || IsNegativeInfinity;

    private double DoubleTicks =>
        Ticks switch
        {
            NaNTicks => double.NaN,
            PositiveInfinityTicks => double.PositiveInfinity,
            NegativeInfinityTicks => double.NegativeInfinity,
            _ => Ticks,
        };

    /// <summary>
    /// Gets the total number of seconds represented by this instance.
    /// </summary>
    public double TotalSeconds => DoubleTicks / TicksPerSecond;

    /// <summary>
    /// Gets the total number of milliseconds represented by this instance.
    /// </summary>
    public double TotalMilliseconds => DoubleTicks / TicksPerMillisecond;

    /// <summary>
    /// Gets the total number of microseconds represented by this instance.
    /// </summary>
    public double TotalMicroseconds => DoubleTicks / TicksPerMicrosecond;

    /// <summary>
    /// Gets the total number of nanoseconds represented by this instance.
    /// </summary>
    public double TotalNanoseconds => DoubleTicks / TicksPerNanosecond;

    private static long FromScaledValue(double value, long scale)
    {
        switch (value)
        {
        case double.NaN:
            return NaNTicks;
        case double.PositiveInfinity:
            return PositiveInfinityTicks;
        case double.NegativeInfinity:
            return NegativeInfinityTicks;
        default:
            break;
        }
        value *= scale;
        if(value > MaxTicks)
        {
            return PositiveInfinityTicks;
        }
        if(value < MinTicks)
        {
            return NegativeInfinityTicks;
        }
        return (long)value;
    }

    /// <summary>
    /// Creates a new <see cref="PreciseDuration"/> representing the specified number of seconds.
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static PreciseDuration FromSeconds(double seconds) =>
        new(FromScaledValue(seconds, TicksPerSecond));

    /// <summary>
    /// Creates a new <see cref="PreciseDuration"/> representing the specified number of milliseconds.
    /// </summary>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static PreciseDuration FromMilliseconds(double milliseconds) =>
        new(FromScaledValue(milliseconds, TicksPerMillisecond));

    /// <summary>
    /// Creates a new <see cref="PreciseDuration"/> representing the specified number of microseconds.
    /// </summary>
    /// <param name="microseconds"></param>
    /// <returns></returns>
    public static PreciseDuration FromMicroseconds(double microseconds) =>
        new(FromScaledValue(microseconds, TicksPerMicrosecond));

    /// <summary>
    /// Creates a new <see cref="PreciseDuration"/> representing the specified number of nanoseconds.
    /// </summary>
    /// <param name="nanoseconds"></param>
    /// <returns></returns>
    public static PreciseDuration FromNanoseconds(double nanoseconds) =>
        new(FromScaledValue(nanoseconds, TicksPerNanosecond));

    #region ToString overloads

    //lang=regex
    private const string _formatMatchingPattern = @"^(?<real>.+?)(?<unit>nsec|usec|msec|sec|)$";
#if NET7_0_OR_GREATER
    [GeneratedRegex(_formatMatchingPattern)]
    private static partial Regex GetFormatMatcher();
#else
    private static Regex GetFormatMatcher() => new (_formatMatchingPattern, RegexOptions.Compiled);
#endif
    private static readonly Regex _FormatMatcher = GetFormatMatcher();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => ToString(TimeScale.Auto);

    /// <inheritdoc cref="ToString(string?, IFormatProvider?)" />
    [ExcludeFromCodeCoverage]
    public string ToString(string? format) =>
        ToString(format, System.Globalization.CultureInfo.CurrentCulture);

    /// <inheritdoc cref="ToString()" />
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <para>The format string.</para>
    /// <para>This contains real number format style and unit suffix.</para>
    /// <para>Available units are <c>sec</c>, <c>msec</c>, <c>usec</c>, <c>nsec</c>, or empty. </para>
    /// <para>for 1.234567 seconds, e.g.</para>
    /// <list type="bullet">
    /// <item><c>G2sec</c>: <c>1.23 sec</c></item>
    /// <item><c>G2msec</c>: <c>1234.57 msec</c></item>
    /// <item><c>e1sec</c>: <c>1.2e+0 sec</c></item>
    /// <item><c>e1msec</c>: <c>1.2e+3 msec</c></item>
    /// <item><c>0.0sec</c>: <c>1.2 sec</c></item>
    /// <item><c>0.0msec</c>: <c>1234.5 msec</c></item>
    /// </list>
    /// <exception cref="FormatException" />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if(format is null)
        {
            return ToString("", TimeScale.Auto, formatProvider);
        }
        var match = _FormatMatcher.Match(format);
        var realFormat = match.Groups["real"].Value;
        var unit = match.Groups["unit"].Value switch
        {
            "sec" => TimeScale.Seconds,
            "msec" => TimeScale.Milliseconds,
            "usec" => TimeScale.Microseconds,
            "nsec" => TimeScale.Nanoseconds,
            _ => TimeScale.Auto,
        };
        return ToString(realFormat, unit, formatProvider);
    }

    /// <inheritdoc cref="ToString(string?, TimeScale, IFormatProvider?)" />
    [ExcludeFromCodeCoverage]
    public string ToString(TimeScale timeScale) =>
        ToString("", timeScale);

    /// <inheritdoc cref="ToString(string?, TimeScale, IFormatProvider?)" />
    [ExcludeFromCodeCoverage]
    public string ToString(string? realPartFormat, TimeScale timeScale) =>
        ToString(realPartFormat, timeScale, System.Globalization.CultureInfo.CurrentCulture);

    /// <summary>
    /// Gets the string representation of the given time span in the specified time scale.
    /// </summary>
    /// <param name="realPartFormat"></param>
    /// <param name="timeScale"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public string ToString(string? realPartFormat, TimeScale timeScale, IFormatProvider? formatProvider) =>
        timeScale switch
        {
            TimeScale.Nanoseconds => string.Format(formatProvider, $"{{0:{realPartFormat}}} nsec", TotalNanoseconds),
            TimeScale.Microseconds => string.Format(formatProvider, $"{{0:{realPartFormat}}} usec", TotalMicroseconds),
            TimeScale.Milliseconds => string.Format(formatProvider, $"{{0:{realPartFormat}}} msec", TotalMilliseconds),
            TimeScale.Seconds => string.Format(formatProvider, $"{{0:{realPartFormat}}} sec", TotalSeconds),
            TimeScale.Auto => ToString(realPartFormat, TimeScale.GetBestTimeScaleFor(this), formatProvider),
            _ => throw new FormatException("Invalid time scale."),
        };

#endregion ToString overloads

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override int GetHashCode() =>
        Ticks.GetHashCode();

    /// <inheritdoc />
    public override bool Equals(object? other) =>
        other switch
        {
            PreciseDuration pd => this == pd,
            TimeSpan ts => Ticks / TicksPerMicrosecond == ts.Ticks / TimeSpan.TicksPerMicrosecond,
            _ => false,
        };

    /// <inheritdoc />
    public bool Equals(PreciseDuration other) => this == other;

    /// <inheritdoc />
    public int CompareTo(PreciseDuration other) => Ticks.CompareTo(other.Ticks);

    /// <inheritdoc />
    public static PreciseDuration operator +(PreciseDuration x, PreciseDuration y)
    {
        if (x.Ticks == NaNTicks || y.Ticks == NaNTicks)
        {
            return new(NaNTicks);
        }
        if (x.Ticks == PositiveInfinityTicks && y.Ticks == NegativeInfinityTicks)
        {
            return new(NaNTicks);
        }
        if (x.Ticks == NegativeInfinityTicks && y.Ticks == PositiveInfinityTicks)
        {
            return new(NaNTicks);
        }
        if (x.Ticks == PositiveInfinityTicks || y.Ticks == PositiveInfinityTicks)
        {
            return new(PositiveInfinityTicks);
        }
        if (x.Ticks == NegativeInfinityTicks || y.Ticks == NegativeInfinityTicks)
        {
            return new(NegativeInfinityTicks);
        }
        if (x.Ticks > 0 && y.Ticks > MaxTicks - x.Ticks)
        {
            return new(PositiveInfinityTicks);
        }
        if (x.Ticks < 0 && y.Ticks < MinTicks - x.Ticks)
        {
            return new(NegativeInfinityTicks);
        }
        return new(x.Ticks + y.Ticks);
    }

    /// <inheritdoc />
    public static PreciseDuration operator -(PreciseDuration x, PreciseDuration y)
    {
        if (x.Ticks == NaNTicks || y.Ticks == NaNTicks)
        {
            return new(NaNTicks);
        }
        if (x.Ticks == PositiveInfinityTicks && y.Ticks == PositiveInfinityTicks)
        {
            return new(NaNTicks);
        }
        if (x.Ticks == NegativeInfinityTicks && y.Ticks == NegativeInfinityTicks)
        {
            return new(NaNTicks);
        }
        if (x.Ticks == PositiveInfinityTicks || y.Ticks == NegativeInfinityTicks)
        {
            return new(PositiveInfinityTicks);
        }
        if (x.Ticks == NegativeInfinityTicks || y.Ticks == PositiveInfinityTicks)
        {
            return new(NegativeInfinityTicks);
        }
        if (y.Ticks > 0 && x.Ticks < MinTicks + y.Ticks)
        {
            return new(NegativeInfinityTicks);
        }
        if (y.Ticks < 0 && x.Ticks > MaxTicks + y.Ticks)
        {
            return new(PositiveInfinityTicks);
        }
        return new(x.Ticks - y.Ticks);
    }

    /// <inheritdoc />
    public static bool operator >(PreciseDuration x, PreciseDuration y)
    {
        if (x.Ticks == NaNTicks || y.Ticks == NaNTicks)
        {
            return false;
        }
        return x.Ticks > y.Ticks;
    }

    /// <inheritdoc />
    public static bool operator >=(PreciseDuration x, PreciseDuration y)
    {
        if (x.Ticks == NaNTicks || y.Ticks == NaNTicks)
        {
            return false;
        }
        return x.Ticks >= y.Ticks;
    }

    /// <inheritdoc />
    public static bool operator <(PreciseDuration x, PreciseDuration y)
    {
        if (x.Ticks == NaNTicks || y.Ticks == NaNTicks)
        {
            return false;
        }
        return x.Ticks < y.Ticks;
    }

    /// <inheritdoc />
    public static bool operator <=(PreciseDuration x, PreciseDuration y)
    {
        if (x.Ticks == NaNTicks || y.Ticks == NaNTicks)
        {
            return false;
        }
        return x.Ticks <= y.Ticks;
    }

    /// <inheritdoc />
    public static bool operator ==(PreciseDuration x, PreciseDuration y)
    {
        if (x.Ticks == NaNTicks || y.Ticks == NaNTicks)
        {
            return false;
        }
        return x.Ticks == y.Ticks;
    }

    /// <inheritdoc />
    public static bool operator !=(PreciseDuration x, PreciseDuration y)
    {
        if (x.Ticks == NaNTicks || y.Ticks == NaNTicks)
        {
            return false;
        }
        return x.Ticks != y.Ticks;
    }

    /// <summary>
    /// Defines an explicit conversion of a <see cref="PreciseDuration"/> to a <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="duration"></param>
    /// <exception cref="InvalidCastException" />
    public static explicit operator TimeSpan(PreciseDuration duration)
    {
        if (duration.IsNaN || duration.IsInfinity)
        {
            throw new InvalidCastException("Cannot convert NaN or Infinity to TimeSpan.");
        }
        return TimeSpan.FromTicks(duration.Ticks / (TicksPerMicrosecond / TimeSpan.TicksPerMicrosecond));
    }

    /// <summary>
    /// Defines an explicit conversion of a <see cref="TimeSpan"/> to a <see cref="PreciseDuration"/>.
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <exception cref="OverflowException" />
    public static explicit operator PreciseDuration(TimeSpan timeSpan)
    {
        if (timeSpan.Ticks > MaxTicks / (TicksPerMicrosecond / TimeSpan.TicksPerMicrosecond) ||
           timeSpan.Ticks < MinTicks / (TicksPerMicrosecond / TimeSpan.TicksPerMicrosecond))
        {
            throw new OverflowException("TimeSpan value is too large or too small to convert to PreciseDuration.");
        }
        return new(timeSpan.Ticks * (TicksPerMicrosecond / TimeSpan.TicksPerMicrosecond));
    }
}
