using System.ComponentModel;
using System.Windows;
using Pompom.Core;

namespace Pompom;

public partial class MainWindow : Window
{
    private readonly TimerEngine _timerEngine;

    internal MainWindow(TimerEngine timerEngine)
    {
        _timerEngine = timerEngine;
        InitializeComponent();
        Closing += OnClosing;
        StateChanged += OnStateChanged;
        RefreshTimer();
    }

    public event EventHandler? SettingsRequested;

    public bool AllowClose { get; set; }

    public void RefreshTimer()
    {
        TimerSnapshot snapshot = _timerEngine.Snapshot;
        SessionText.Text = GetSessionName(snapshot.Session);
        CycleText.Text = GetCycleText(snapshot);
        CountdownText.Text = TimerTextFormatter.Format(snapshot.Remaining);
        StatusText.Text = snapshot.Status switch
        {
            TimerStatus.Running => "Running",
            TimerStatus.Paused => "Paused",
            TimerStatus.Idle => "Ready",
            _ => string.Empty,
        };

        double progress = snapshot.Duration <= TimeSpan.Zero
            ? 0
            : 1 - (snapshot.Remaining.TotalMilliseconds / snapshot.Duration.TotalMilliseconds);
        SessionProgress.Value = Math.Clamp(progress, 0, 1);
        StartButton.Content = snapshot.Status == TimerStatus.Paused ? "Resume" : "Start";
        StartButton.IsEnabled = snapshot.Status != TimerStatus.Running;
        StopButton.IsEnabled = snapshot.Status == TimerStatus.Running;
    }

    internal static string GetSessionName(SessionType session)
    {
        return session switch
        {
            SessionType.Work => "Focus",
            SessionType.ShortBreak => "Short break",
            SessionType.LongBreak => "Long break",
            _ => string.Empty,
        };
    }

    private static string GetCycleText(TimerSnapshot snapshot)
    {
        return snapshot.Session switch
        {
            SessionType.Work => $"Pomodoro {snapshot.CompletedWorkSessions + 1} of 4",
            SessionType.LongBreak => "Four pomodoros complete",
            _ => $"{snapshot.CompletedWorkSessions} of 4 pomodoros complete",
        };
    }

    private void StartButton_Click(object sender, RoutedEventArgs eventArgs)
    {
        _timerEngine.Start();
        RefreshTimer();
    }

    private void StopButton_Click(object sender, RoutedEventArgs eventArgs)
    {
        _timerEngine.Pause();
        RefreshTimer();
    }

    private void SkipButton_Click(object sender, RoutedEventArgs eventArgs)
    {
        _timerEngine.Skip();
        RefreshTimer();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs eventArgs)
    {
        _timerEngine.Reset();
        RefreshTimer();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs eventArgs)
    {
        SettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnClosing(object? sender, CancelEventArgs eventArgs)
    {
        if (AllowClose)
        {
            return;
        }

        eventArgs.Cancel = true;
        Hide();
    }

    private void OnStateChanged(object? sender, EventArgs eventArgs)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }
}
