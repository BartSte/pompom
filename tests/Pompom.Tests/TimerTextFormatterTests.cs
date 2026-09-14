using Pompom.Core;

namespace Pompom.Tests;

[TestClass]
public sealed class TimerTextFormatterTests
{
    [DataRow(0, "0:00")]
    [DataRow(1, "0:01")]
    [DataRow(65, "1:05")]
    [DataRow(10800, "180:00")]
    [TestMethod]
    public void FormatUsesTotalMinutes(int seconds, string expected)
    {
        Assert.AreEqual(expected, TimerTextFormatter.Format(TimeSpan.FromSeconds(seconds)));
    }

    [TestMethod]
    public void FormatRoundsPartialSecondsUp()
    {
        Assert.AreEqual("0:01", TimerTextFormatter.Format(TimeSpan.FromMilliseconds(1)));
    }
}
