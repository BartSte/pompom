namespace Pompom.Tests;

internal sealed class ManualTimeProvider : TimeProvider
{
    private long _timestamp;
    private DateTimeOffset _utcNow = DateTimeOffset.UnixEpoch;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public override long GetTimestamp() => _timestamp;

    public void Advance(TimeSpan duration)
    {
        _timestamp += duration.Ticks;
        _utcNow += duration;
    }
}
