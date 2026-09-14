using System.IO;
using System.Security;
using Microsoft.Windows.AppNotifications;

namespace Pompom.Services;

internal sealed class WindowsAppNotificationAdapter : IAppNotificationAdapter
{
    private AppNotificationManager? _manager;
    private bool _registered;

    public event EventHandler? Activated;

    public bool IsSupported => AppNotificationManager.IsSupported();

    public void Register()
    {
        if (_registered)
        {
            return;
        }

        _manager = AppNotificationManager.Default;
        _manager.NotificationInvoked += OnNotificationInvoked;
        try
        {
            string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Pompom.ico");
            var iconUri = new Uri(iconPath);
            _manager.Register("Pompom", iconUri);
            _registered = true;
        }
        catch
        {
            _manager.NotificationInvoked -= OnNotificationInvoked;
            throw;
        }
    }

    public void Show(NotificationMessage message)
    {
        if (_manager is null)
        {
            throw new InvalidOperationException("Notifications are not registered.");
        }

        string xml = $"""
            <toast launch="show">
              <visual>
                <binding template="ToastGeneric">
                  <text>{SecurityElement.Escape(message.Title)}</text>
                  <text>{SecurityElement.Escape(message.Body)}</text>
                </binding>
              </visual>
              <audio src="{GetSoundEvent(message.Sound)}" />
            </toast>
            """;

        _manager.Show(new AppNotification(xml));
    }

    internal static string GetSoundEvent(NotificationSound sound) => sound switch
    {
        NotificationSound.BreakStart => "ms-winsoundevent:Notification.IM",
        NotificationSound.WorkStart => "ms-winsoundevent:Notification.Mail",
        _ => "ms-winsoundevent:Notification.Default",
    };

    public void Dispose()
    {
        if (_manager is null)
        {
            return;
        }

        _manager.NotificationInvoked -= OnNotificationInvoked;
        if (_registered)
        {
            try
            {
                _manager.Unregister();
            }
            catch (Exception exception) when (
                exception is InvalidOperationException
                    or NotSupportedException
                    or UnauthorizedAccessException
                    or System.Runtime.InteropServices.COMException)
            {
                // Registration cleanup is best effort during process shutdown.
            }
        }

        _registered = false;
        _manager = null;
    }

    private void OnNotificationInvoked(
        AppNotificationManager sender,
        AppNotificationActivatedEventArgs args)
    {
        Activated?.Invoke(this, EventArgs.Empty);
    }
}
