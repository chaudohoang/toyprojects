namespace FtpUpload;

/// <summary>
/// Test seam for the day-rollover. Normally this is just the real clock. When
/// SimulateFastDaySeconds &gt; 0, the watch loop advances <see cref="Offset"/> by whole days so a
/// midnight rollover can be exercised in seconds instead of waiting for real midnight.
///
/// IMPORTANT: only DATE-dependent code (day-file paths, day-change detection) reads this. Elapsed
/// second timers (per-file timeout, panel timeout, NG cooldown) and log time-of-day stamps keep
/// using the real <see cref="System.DateTime"/> clock, so those behaviours are unaffected by the
/// simulation.
/// </summary>
public static class Clock
{
    /// <summary>Added to the real clock. Zero in production; whole days in fast-day test mode.</summary>
    public static System.TimeSpan Offset = System.TimeSpan.Zero;

    public static System.DateTime Now => System.DateTime.Now + Offset;
    public static System.DateTime Today => Now.Date;
}
