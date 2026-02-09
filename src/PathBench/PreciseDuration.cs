using System.Numerics;

namespace PathBench;

/// <summary>
/// Provides pico-order precise duration representation.
/// </summary>
public readonly struct PreciseDuration(long ticks)
    : IEquatable<PreciseDuration>
    , IComparable<PreciseDuration>
    , IComparisonOperators<PreciseDuration, PreciseDuration, bool>
    , IAdditionOperators<PreciseDuration, PreciseDuration, PreciseDuration>
    , ISubtractionOperators<PreciseDuration, PreciseDuration, PreciseDuration>
{
    /// <summary>
    /// Represents a Not-a-Number (NaN) value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long NaNValue = long.MinValue;

    /// <summary>
    /// Represents positive infinity value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long PositiveInfinityValue = long.MaxValue;

    /// <summary>
    /// Represents negative infinity value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long NegativeInfinityValue = long.MinValue + 1;

    /// <summary>
    /// Represents the minimum finite value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long MaxValue = long.MaxValue - 1;

    /// <summary>
    /// Represents the maximum finite value of ticks in <see cref="PreciseDuration"/>.
    /// </summary>
    public const long MinValue = long.MinValue + 2;

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
    /// Gets the number of ticks that represent the value of the current TimeSpan structure.
    /// </summary>
    public long Ticks { get; } = ticks;

    private double DoubleTicks =>
        Ticks switch
        {
            NaNValue => double.NaN,
            PositiveInfinityValue => double.PositiveInfinity,
            NegativeInfinityValue => double.NegativeInfinity,
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

    private static long FromScaledValue(double value, long scale) =>
        value switch
        {
            double.NaN => NaNValue,
            double.PositiveInfinity => PositiveInfinityValue,
            double.NegativeInfinity => NegativeInfinityValue,
            _ => (long)(value * scale),
        };

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

    /// <inheritdoc />
    public override string ToString() => ToString(TimeScale.Auto);

    /// <summary>
    /// Gets the string representation of the given time span in the specified time scale.
    /// </summary>
    /// <param name="timeScale"></param>
    /// <returns></returns>
    public string ToString(TimeScale timeScale) =>
            timeScale switch
            {
                TimeScale.Nanoseconds => $"{TotalNanoseconds} nsec",
                TimeScale.Microseconds => $"{TotalMicroseconds} usec",
                TimeScale.Milliseconds => $"{TotalMilliseconds} msec",
                TimeScale.Auto => ToString(TimeScale.GetBestTimeScaleFor(this)),
                _ => string.Empty,
            };

    /// <inheritdoc />
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
    public static PreciseDuration operator +(PreciseDuration left, PreciseDuration right) =>
        new(left.Ticks + right.Ticks);

    /// <inheritdoc />
    public static PreciseDuration operator -(PreciseDuration left, PreciseDuration right) =>
        new(left.Ticks - right.Ticks);

    /// <inheritdoc />
    public static bool operator >(PreciseDuration left, PreciseDuration right) =>
        left.Ticks > right.Ticks;

    /// <inheritdoc />
    public static bool operator >=(PreciseDuration left, PreciseDuration right) =>
        left.Ticks >= right.Ticks;

    /// <inheritdoc />
    public static bool operator <(PreciseDuration left, PreciseDuration right) =>
        left.Ticks < right.Ticks;

    /// <inheritdoc />
    public static bool operator <=(PreciseDuration left, PreciseDuration right) =>
        left.Ticks <= right.Ticks;

    /// <inheritdoc />
    public static bool operator ==(PreciseDuration left, PreciseDuration right) =>
        left.Ticks == right.Ticks;

    /// <inheritdoc />
    public static bool operator !=(PreciseDuration left, PreciseDuration right) =>
        left.Ticks != right.Ticks;
}
