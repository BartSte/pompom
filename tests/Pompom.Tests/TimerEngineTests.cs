using Pompom.Core;

namespace Pompom.Tests;

[TestClass]
public sealed class TimerEngineTests
{
    [TestMethod]
    public void NewTimerUsesDefaultWorkSession()
    {
        var engine = new TimerEngine(PompomSettings.Default);

        TimerSnapshot snapshot = engine.Snapshot;

        Assert.AreEqual(SessionType.Work, snapshot.Session);
        Assert.AreEqual(TimerStatus.Idle, snapshot.Status);
        Assert.AreEqual(TimeSpan.FromMinutes(25), snapshot.Remaining);
        Assert.AreEqual(0, snapshot.CompletedWorkSessions);
    }

    [TestMethod]
    public void StartPauseAndResumeKeepRemainingTime()
    {
        var clock = new ManualTimeProvider();
        var engine = new TimerEngine(
            PompomSettings.Default with { WorkMinutes = 1 },
            clock);

        engine.Start();
        clock.Advance(TimeSpan.FromSeconds(10));
        engine.Pause();
        clock.Advance(TimeSpan.FromSeconds(20));

        Assert.AreEqual(TimerStatus.Paused, engine.Snapshot.Status);
        Assert.AreEqual(TimeSpan.FromSeconds(50), engine.Snapshot.Remaining);

        engine.Start();
        clock.Advance(TimeSpan.FromSeconds(5));
        engine.Update();

        Assert.AreEqual(TimerStatus.Running, engine.Snapshot.Status);
        Assert.AreEqual(TimeSpan.FromSeconds(45), engine.Snapshot.Remaining);
    }

    [TestMethod]
    public void ResetRestoresCurrentSessionWithoutChangingCycle()
    {
        var clock = new ManualTimeProvider();
        var engine = new TimerEngine(
            PompomSettings.Default with { WorkMinutes = 1 },
            clock);
        engine.Start();
        clock.Advance(TimeSpan.FromSeconds(20));

        engine.Reset();

        Assert.AreEqual(TimerStatus.Idle, engine.Snapshot.Status);
        Assert.AreEqual(TimeSpan.FromMinutes(1), engine.Snapshot.Remaining);
        Assert.AreEqual(0, engine.Snapshot.CompletedWorkSessions);
    }

    [TestMethod]
    public void NaturalCompletionUsesSeparateAutoStartSettings()
    {
        var clock = new ManualTimeProvider();
        var engine = new TimerEngine(
            PompomSettings.Default with
            {
                WorkMinutes = 1,
                ShortBreakMinutes = 1,
                AutoStartBreaks = true,
                AutoStartWork = false,
            },
            clock);

        engine.Start();
        clock.Advance(TimeSpan.FromMinutes(1));
        engine.Update();

        Assert.AreEqual(SessionType.ShortBreak, engine.Snapshot.Session);
        Assert.AreEqual(TimerStatus.Running, engine.Snapshot.Status);

        clock.Advance(TimeSpan.FromMinutes(1));
        engine.Update();

        Assert.AreEqual(SessionType.Work, engine.Snapshot.Session);
        Assert.AreEqual(TimerStatus.Idle, engine.Snapshot.Status);
    }

    [TestMethod]
    public void FourthWorkSessionUsesLongBreak()
    {
        var engine = new TimerEngine(PompomSettings.Default);

        for (int session = 1; session <= 4; session++)
        {
            engine.Skip();
            if (session < 4)
            {
                Assert.AreEqual(SessionType.ShortBreak, engine.Snapshot.Session);
                engine.Skip();
            }
        }

        Assert.AreEqual(SessionType.LongBreak, engine.Snapshot.Session);
        Assert.AreEqual(0, engine.Snapshot.CompletedWorkSessions);
    }

    [TestMethod]
    public void DisabledLongBreakUsesShortBreakAndStartsNewSet()
    {
        var engine = new TimerEngine(
            PompomSettings.Default with { LongBreakEnabled = false });

        for (int session = 1; session <= 4; session++)
        {
            engine.Skip();
            if (session < 4)
            {
                engine.Skip();
            }
        }

        Assert.AreEqual(SessionType.ShortBreak, engine.Snapshot.Session);
        Assert.AreEqual(0, engine.Snapshot.CompletedWorkSessions);
    }

    [TestMethod]
    public void SkipCountsWorkAndPreservesOnlyRunningState()
    {
        var engine = new TimerEngine(PompomSettings.Default);

        engine.Start();
        engine.Skip();

        Assert.AreEqual(SessionType.ShortBreak, engine.Snapshot.Session);
        Assert.AreEqual(TimerStatus.Running, engine.Snapshot.Status);
        Assert.AreEqual(1, engine.Snapshot.CompletedWorkSessions);

        engine.Pause();
        engine.Skip();

        Assert.AreEqual(SessionType.Work, engine.Snapshot.Session);
        Assert.AreEqual(TimerStatus.Idle, engine.Snapshot.Status);
    }

    [TestMethod]
    public void SkipDoesNotRaiseCompletionEvent()
    {
        var engine = new TimerEngine(PompomSettings.Default);
        int completionCount = 0;
        engine.SessionCompleted += (_, _) => completionCount++;

        engine.Skip();

        Assert.AreEqual(0, completionCount);
    }

    [TestMethod]
    public void LateUpdateAdvancesOnlyOneSession()
    {
        var clock = new ManualTimeProvider();
        var engine = new TimerEngine(
            PompomSettings.Default with
            {
                WorkMinutes = 1,
                ShortBreakMinutes = 5,
                AutoStartBreaks = true,
            },
            clock);

        engine.Start();
        clock.Advance(TimeSpan.FromMinutes(20));
        engine.Update();

        Assert.AreEqual(SessionType.ShortBreak, engine.Snapshot.Session);
        Assert.AreEqual(TimerStatus.Running, engine.Snapshot.Status);
        Assert.AreEqual(TimeSpan.FromMinutes(5), engine.Snapshot.Remaining);
    }

    [TestMethod]
    public void SettingsUpdateChangesIdleButNotPausedSession()
    {
        var engine = new TimerEngine(PompomSettings.Default, new ManualTimeProvider());
        engine.UpdateSettings(PompomSettings.Default with { WorkMinutes = 40 });

        Assert.AreEqual(TimeSpan.FromMinutes(40), engine.Snapshot.Remaining);

        engine.Start();
        engine.Pause();
        engine.UpdateSettings(PompomSettings.Default with { WorkMinutes = 60 });

        Assert.AreEqual(TimeSpan.FromMinutes(40), engine.Snapshot.Remaining);

        engine.Reset();

        Assert.AreEqual(TimeSpan.FromMinutes(60), engine.Snapshot.Remaining);
    }
}
