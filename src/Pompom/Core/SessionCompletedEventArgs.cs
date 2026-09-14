namespace Pompom.Core;

internal sealed class SessionCompletedEventArgs(
    SessionType completedSession,
    SessionType nextSession) : EventArgs
{
    public SessionType CompletedSession { get; } = completedSession;

    public SessionType NextSession { get; } = nextSession;
}
