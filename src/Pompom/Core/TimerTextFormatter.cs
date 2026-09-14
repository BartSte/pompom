using System.Globalization;

namespace Pompom.Core;

internal static class TimerTextFormatter
{
    public static string Format(TimeSpan remaining)
    {
        long totalSeconds = Math.Max(0, (long)Math.Ceiling(remaining.TotalSeconds));
        long minutes = totalSeconds / 60;
        long seconds = totalSeconds % 60;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{minutes}:{seconds:00}");
    }
}
