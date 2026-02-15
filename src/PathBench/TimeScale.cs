namespace PathBench;

/// <summary>
/// Specifies the time scale to display measurement results.
/// </summary>
public enum TimeScale
{
    /// <summary> Automatically selects the most appropriate time scale. </summary>
    Auto,

    /// <summary> Displays measurement results in nanoseconds. </summary>
    Nanoseconds,

    /// <summary> Gets or sets the duration in microseconds. </summary>
    Microseconds,

    /// <summary> Gets or sets the duration in milliseconds. </summary>
    Milliseconds,

    /// <summary> Gets or sets the duration in seconds. </summary>
    Seconds,
}


/// <summary>
/// Provides extension methods for <see cref="TimeScale"/>.
/// </summary>
public static class TimeScaleExtensions
{
    extension(TimeScale timeScale)
    {
        /// <summary>
        /// Gets the best time scale for the given time spans.
        /// </summary>
        /// <param name="timeSpans"></param>
        /// <returns></returns>
        /// <remarks>
        /// The best time scale is determined by the smallest time span in the collection.
        /// <list type="bullet">
        /// <item>If minimum time span is greater than sub-msec(>= 0.1msec), <see cref="TimeScale.Milliseconds"/> will be selected. </item>
        /// <item>If minimum time span is greater than sub-usec(>= 0.1usec), <see cref="TimeScale.Microseconds"/> will be selected. </item>
        /// <item>Otherwise, <see cref="TimeScale.Nanoseconds"/> will be selected. </item>
        /// </list>
        /// </remarks>
        public static TimeScale GetBestTimeScaleFor(IEnumerable<PreciseDuration> timeSpans) =>
            GetBestTimeScaleFor(timeSpans.Min());

        /// <summary>
        /// Gets the best time scale for the given duration.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        /// <remarks>
        /// The best time scale is determined by the smallest time span in the collection.
        /// <list type="bullet">
        /// <item>If duration is greater than sub-msec(>= 0.1msec), <see cref="TimeScale.Milliseconds"/> will be selected. </item>
        /// <item>If duration is greater than sub-usec(>= 0.1usec), <see cref="TimeScale.Microseconds"/> will be selected. </item>
        /// <item>Otherwise, <see cref="TimeScale.Nanoseconds"/> will be selected. </item>
        /// </list>
        /// </remarks>
        public static TimeScale GetBestTimeScaleFor(PreciseDuration duration)
        {
            if (duration.TotalSeconds >= 0.1)
            {
                return TimeScale.Seconds;
            }
            if (duration.TotalMilliseconds >= 0.1)
            {
                return TimeScale.Milliseconds;
            }
            else if (duration.TotalMicroseconds >= 0.1)
            {
                return TimeScale.Microseconds;
            }
            else
            {
                return TimeScale.Nanoseconds;
            }
        }
    }
}