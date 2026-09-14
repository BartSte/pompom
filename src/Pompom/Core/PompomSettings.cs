namespace Pompom.Core;

internal sealed record PompomSettings
{
    internal const int CurrentSchemaVersion = 1;
    internal const int MinimumMinutes = 1;
    internal const int MaximumMinutes = 180;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public int WorkMinutes { get; init; } = 25;

    public int ShortBreakMinutes { get; init; } = 5;

    public int LongBreakMinutes { get; init; } = 15;

    public bool LongBreakEnabled { get; init; } = true;

    public bool AutoStartBreaks { get; init; } = true;

    public bool AutoStartWork { get; init; }

    public bool NotifyAfterWork { get; init; } = true;

    public bool NotifyAfterBreak { get; init; } = true;

    public static PompomSettings Default { get; } = new();

    public PompomSettings Normalize()
    {
        return this with
        {
            SchemaVersion = CurrentSchemaVersion,
            WorkMinutes = NormalizeMinutes(WorkMinutes, Default.WorkMinutes),
            ShortBreakMinutes = NormalizeMinutes(ShortBreakMinutes, Default.ShortBreakMinutes),
            LongBreakMinutes = NormalizeMinutes(LongBreakMinutes, Default.LongBreakMinutes),
        };
    }

    private static int NormalizeMinutes(int value, int defaultValue)
    {
        return value is >= MinimumMinutes and <= MaximumMinutes ? value : defaultValue;
    }
}
