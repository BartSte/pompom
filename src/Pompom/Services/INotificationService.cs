namespace Pompom.Services;

internal interface INotificationService : IDisposable
{
    event EventHandler? Activated;

    bool Initialize();

    void Show(NotificationMessage message);
}
