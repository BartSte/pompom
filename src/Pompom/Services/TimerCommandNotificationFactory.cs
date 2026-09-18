using Pompom.Core;

namespace Pompom.Services;

internal static class TimerCommandNotificationFactory
{
    public static NotificationMessage? Create(
        HotkeyAction action,
        TimerSnapshot before,
        TimerSnapshot after)
    {
        return action switch
        {
            HotkeyAction.Start when before.Status != TimerStatus.Running => new NotificationMessage(
                before.Status == TimerStatus.Paused ? "Timer resumed" : "Timer started",
                $"The {GetSessionName(after.Session)} timer is running.",
                NotificationSound.Default),
            HotkeyAction.Stop when before.Status == TimerStatus.Running => new NotificationMessage(
                "Timer stopped",
                $"The {GetSessionName(after.Session)} timer is paused.",
                NotificationSound.Default),
            HotkeyAction.Skip => new NotificationMessage(
                "Session skipped",
                $"The {GetSessionName(after.Session)} timer is ready.",
                NotificationSound.Default),
            HotkeyAction.Reset => new NotificationMessage(
                "Timer reset",
                $"The {GetSessionName(after.Session)} timer is ready.",
                NotificationSound.Default),
            _ => null,
        };
    }

    private static string GetSessionName(SessionType session) => session switch
    {
        SessionType.Work => "focus",
        SessionType.ShortBreak => "short break",
        SessionType.LongBreak => "long break",
        _ => throw new InvalidOperationException("Unknown session type."),
    };
}
