using System.IO;
using System.Windows;
using System.Windows.Threading;
using Pompom.Core;
using Pompom.Services;

namespace Pompom;

public partial class App : System.Windows.Application
{
    private SingleInstanceCoordinator? _singleInstance;
    private ISettingsStore? _settingsStore;
    private PompomSettings _settings = PompomSettings.Default;
    private TimerEngine? _timerEngine;
    private MainWindow? _mainWindow;
    private TrayIconService? _trayIcon;
    private INotificationService? _notificationService;
    private IGlobalHotkeyService? _hotkeyService;
    private DispatcherTimer? _displayTimer;
    private bool _exiting;

    protected override void OnStartup(StartupEventArgs eventArgs)
    {
        base.OnStartup(eventArgs);

        _singleInstance = new SingleInstanceCoordinator();
        if (!_singleInstance.IsPrimary)
        {
            _singleInstance.SignalPrimaryInstance();
            _singleInstance.Dispose();
            _singleInstance = null;
            Shutdown();
            return;
        }

        _singleInstance.ActivationRequested += (_, _) =>
            Dispatcher.BeginInvoke(ShowMainWindow);
        _singleInstance.StartListening();

        _settingsStore = new JsonSettingsStore();
        _settings = _settingsStore.Load();
        AppTheme.Apply(_settings.DarkTheme);
        _timerEngine = new TimerEngine(_settings);
        _timerEngine.SessionCompleted += OnSessionCompleted;

        _mainWindow = new MainWindow(_timerEngine);
        MainWindow = _mainWindow;
        _mainWindow.SettingsRequested += (_, _) => OpenSettings();
        _mainWindow.TimerCommandRequested += OnHotkeyPressed;

        _trayIcon = new TrayIconService();
        _trayIcon.ShowRequested += (_, _) => ShowMainWindow();
        _trayIcon.StartRequested += (_, _) => RunTimerCommand(HotkeyAction.Start, _timerEngine.Start);
        _trayIcon.StopRequested += (_, _) => RunTimerCommand(HotkeyAction.Stop, _timerEngine.Pause);
        _trayIcon.SkipRequested += (_, _) => RunTimerCommand(HotkeyAction.Skip, _timerEngine.Skip);
        _trayIcon.SettingsRequested += (_, _) => OpenSettings();
        _trayIcon.ExitRequested += (_, _) => ExitApplication();

        _notificationService = new WindowsNotificationService(
            new WindowsAppNotificationAdapter(),
            (title, body) => _trayIcon.ShowBalloon(title, body));
        _notificationService.Activated += (_, _) =>
            Dispatcher.BeginInvoke(ShowMainWindow);
        _notificationService.Initialize();

        _hotkeyService = new GlobalHotkeyService();
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;

        _displayTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(200),
        };
        _displayTimer.Tick += (_, _) =>
        {
            _timerEngine.Update();
            RefreshViews();
        };
        _displayTimer.Start();

        _mainWindow.Show();
        IReadOnlyList<HotkeyBinding> failedHotkeys = _hotkeyService.Register(_mainWindow);
        RefreshViews();

        if (failedHotkeys.Count > 0)
        {
            string shortcuts = string.Join(
                Environment.NewLine,
                failedHotkeys.Select(binding => binding.Shortcut));
            System.Windows.MessageBox.Show(
                _mainWindow,
                $"Pompom could not register these shortcuts:{Environment.NewLine}{shortcuts}"
                    + $"{Environment.NewLine}{Environment.NewLine}Another application may use them.",
                "Keyboard shortcuts unavailable",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    protected override void OnExit(ExitEventArgs eventArgs)
    {
        _displayTimer?.Stop();
        _hotkeyService?.Dispose();
        _notificationService?.Dispose();
        _trayIcon?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(eventArgs);
    }

    private void OnSessionCompleted(object? sender, SessionCompletedEventArgs eventArgs)
    {
        NotificationMessage? message = SessionNotificationFactory.Create(
            eventArgs.CompletedSession,
            eventArgs.NextSession,
            _settings);
        if (message is not null)
        {
            _notificationService?.Show(message);
        }
    }

    private void OnHotkeyPressed(object? sender, HotkeyPressedEventArgs eventArgs)
    {
        if (_timerEngine is null)
        {
            return;
        }

        switch (eventArgs.Action)
        {
            case HotkeyAction.Start:
                RunTimerCommand(HotkeyAction.Start, _timerEngine.Start);
                break;
            case HotkeyAction.Stop:
                RunTimerCommand(HotkeyAction.Stop, _timerEngine.Pause);
                break;
            case HotkeyAction.Skip:
                RunTimerCommand(HotkeyAction.Skip, _timerEngine.Skip);
                break;
            case HotkeyAction.Reset:
                RunTimerCommand(HotkeyAction.Reset, _timerEngine.Reset);
                break;
            case HotkeyAction.ShowOrHide:
                ToggleMainWindow();
                break;
            default:
                throw new InvalidOperationException("Unknown hotkey action.");
        }
    }

    private void RunTimerCommand(HotkeyAction action, Action command)
    {
        bool isInTray = _mainWindow?.IsVisible != true;
        TimerSnapshot before = _timerEngine!.Snapshot;
        command();
        RefreshViews();

        if (!isInTray)
        {
            return;
        }

        NotificationMessage? message = TimerCommandNotificationFactory.Create(
            action,
            before,
            _timerEngine.Snapshot);
        if (message is not null)
        {
            _notificationService?.Show(message);
        }
    }

    private void RefreshViews()
    {
        if (_timerEngine is null || _mainWindow is null || _trayIcon is null)
        {
            return;
        }

        TimerSnapshot snapshot = _timerEngine.Snapshot;
        _mainWindow.RefreshTimer();
        _trayIcon.Update(
            Pompom.MainWindow.GetSessionName(snapshot.Session),
            TimerTextFormatter.Format(snapshot.Remaining),
            snapshot.Status == TimerStatus.Running);
    }

    private void OpenSettings()
    {
        if (_mainWindow is null || _settingsStore is null || _timerEngine is null)
        {
            return;
        }

        ShowMainWindow();
        var settingsWindow = new SettingsWindow(
            _settings,
            () => _notificationService?.Show(new NotificationMessage(
                "Pompom alarm test",
                "The alarm must continue until you select Dismiss.",
                NotificationSound.BreakStart,
                IsAlarm: true)))
        {
            Owner = _mainWindow,
        };

        if (settingsWindow.ShowDialog() != true
            || settingsWindow.SavedSettings is not { } newSettings)
        {
            return;
        }

        try
        {
            _settingsStore.Save(newSettings);
            _settings = newSettings.Normalize();
            AppTheme.Apply(_settings.DarkTheme);
            _timerEngine.UpdateSettings(_settings);
            RefreshViews();
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            System.Windows.MessageBox.Show(
                _mainWindow,
                "Pompom could not save the settings. Check access to the local app-data folder.",
                "Settings were not saved",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ToggleMainWindow()
    {
        if (_mainWindow?.IsVisible == true)
        {
            _mainWindow.Hide();
        }
        else
        {
            ShowMainWindow();
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.Show();
        if (_mainWindow.WindowState == WindowState.Minimized)
        {
            _mainWindow.WindowState = WindowState.Normal;
        }

        _mainWindow.Activate();
        _mainWindow.Topmost = true;
        _mainWindow.Topmost = false;
        _mainWindow.Focus();
    }

    private void ExitApplication()
    {
        if (_exiting)
        {
            return;
        }

        _exiting = true;
        _displayTimer?.Stop();
        _hotkeyService?.Dispose();
        _hotkeyService = null;
        if (_mainWindow is not null)
        {
            _mainWindow.AllowClose = true;
            _mainWindow.Close();
        }

        Shutdown();
    }
}
