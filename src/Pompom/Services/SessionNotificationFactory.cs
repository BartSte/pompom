using Pompom.Core;

namespace Pompom.Services;

internal static class SessionNotificationFactory
{
    public static NotificationMessage? Create(
        SessionType completedSession,
        SessionType nextSession,
        PompomSettings settings)
    {
        if (completedSession == SessionType.Work)
        {
            if (!settings.NotifyAfterWork)
            {
                return null;
            }

            string breakName = nextSession == SessionType.LongBreak
                ? "long break"
                : "short break";
            return new NotificationMessage(
                "Work session complete",
                $"It is time for a {breakName}.",
                NotificationSound.BreakStart);
        }

        if (!settings.NotifyAfterBreak)
        {
            return null;
        }

        return new NotificationMessage(
            "Break complete",
            "You are ready for the next work session.",
            NotificationSound.WorkStart);
    }
}
