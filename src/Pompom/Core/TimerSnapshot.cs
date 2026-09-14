namespace Pompom.Core;

internal sealed record TimerSnapshot(
    SessionType Session,
    TimerStatus Status,
    TimeSpan Remaining,
    TimeSpan Duration,
    int CompletedWorkSessions);
