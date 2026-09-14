namespace Pompom.Core;

internal sealed class TimerEngine
{
    private readonly TimeProvider _timeProvider;
    private PompomSettings _settings;
    private SessionType _session = SessionType.Work;
    private TimerStatus _status = TimerStatus.Idle;
    private TimeSpan _duration;
    private TimeSpan _remaining;
    private TimeSpan _remainingAtStart;
    private long _startedAt;
    private int _completedWorkSessions;

    public TimerEngine(PompomSettings settings, TimeProvider? timeProvider = null)
    {
        _settings = settings.Normalize();
        _timeProvider = timeProvider ?? TimeProvider.System;
        _duration = GetDuration(_session);
        _remaining = _duration;
    }

    public event EventHandler<SessionCompletedEventArgs>? SessionCompleted;

    public TimerSnapshot Snapshot => new(
        _session,
        _status,
        _remaining,
        _duration,
        _completedWorkSessions);

    public void Start()
    {
        if (_status == TimerStatus.Running)
        {
            return;
        }

        if (_remaining <= TimeSpan.Zero)
        {
            _remaining = _duration;
        }

        StartCurrentSession();
    }

    public void Pause()
    {
        if (_status != TimerStatus.Running)
        {
            return;
        }

        Update();
        if (_status != TimerStatus.Running)
        {
            return;
        }

        _remaining = GetCurrentRemaining();
        _status = TimerStatus.Paused;
    }

    public void Reset()
    {
        _duration = GetDuration(_session);
        _remaining = _duration;
        _status = TimerStatus.Idle;
    }

    public void Skip()
    {
        bool continueRunning = _status == TimerStatus.Running;
        AdvanceToNextSession();

        if (continueRunning)
        {
            StartCurrentSession();
        }
        else
        {
            _status = TimerStatus.Idle;
        }
    }

    public void Update()
    {
        if (_status != TimerStatus.Running)
        {
            return;
        }

        TimeSpan currentRemaining = GetCurrentRemaining();
        if (currentRemaining > TimeSpan.Zero)
        {
            _remaining = currentRemaining;
            return;
        }

        SessionType completedSession = _session;
        AdvanceToNextSession();

        bool autoStart = _session == SessionType.Work
            ? _settings.AutoStartWork
            : _settings.AutoStartBreaks;

        if (autoStart)
        {
            StartCurrentSession();
        }
        else
        {
            _status = TimerStatus.Idle;
        }

        SessionCompleted?.Invoke(
            this,
            new SessionCompletedEventArgs(completedSession, _session));
    }

    public void UpdateSettings(PompomSettings settings)
    {
        _settings = settings.Normalize();
        if (_status != TimerStatus.Idle)
        {
            return;
        }

        _duration = GetDuration(_session);
        _remaining = _duration;
    }

    private void StartCurrentSession()
    {
        _remainingAtStart = _remaining;
        _startedAt = _timeProvider.GetTimestamp();
        _status = TimerStatus.Running;
    }

    private TimeSpan GetCurrentRemaining()
    {
        TimeSpan elapsed = _timeProvider.GetElapsedTime(_startedAt);
        TimeSpan remaining = _remainingAtStart - elapsed;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    private void AdvanceToNextSession()
    {
        _session = _session switch
        {
            SessionType.Work => GetNextBreak(),
            SessionType.ShortBreak or SessionType.LongBreak => SessionType.Work,
            _ => throw new InvalidOperationException("Unknown session type."),
        };

        _duration = GetDuration(_session);
        _remaining = _duration;
    }

    private SessionType GetNextBreak()
    {
        _completedWorkSessions++;
        if (_completedWorkSessions < 4)
        {
            return SessionType.ShortBreak;
        }

        _completedWorkSessions = 0;
        return _settings.LongBreakEnabled
            ? SessionType.LongBreak
            : SessionType.ShortBreak;
    }

    private TimeSpan GetDuration(SessionType session)
    {
        int minutes = session switch
        {
            SessionType.Work => _settings.WorkMinutes,
            SessionType.ShortBreak => _settings.ShortBreakMinutes,
            SessionType.LongBreak => _settings.LongBreakMinutes,
            _ => throw new InvalidOperationException("Unknown session type."),
        };

        return TimeSpan.FromMinutes(minutes);
    }
}
