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
}


internal static class  TimeScaleExtensions
{
    extension(TimeScale timeScale)
    {
        public static TimeScale SelectAuto(IEnumerable<TimeSpan> timeSpans)
        {
            var minTime = timeSpans.Where(static x => x > TimeSpan.Zero).Min();
            if (minTime.TotalMilliseconds >= 0.1)
            {
                return TimeScale.Milliseconds;
            }
            else if (minTime.TotalMicroseconds >= 0.1)
            {
                return TimeScale.Microseconds;
            }
            else
            {
                return TimeScale.Nanoseconds;
            }
        }

        public string GetString(TimeSpan time) =>
            timeScale switch
            {
                TimeScale.Nanoseconds => $"{time.TotalNanoseconds} nsec",
                TimeScale.Microseconds => $"{time.TotalMicroseconds} usec",
                TimeScale.Milliseconds => $"{time.TotalMilliseconds} msec",
                _ => string.Empty,
            };
    }
}