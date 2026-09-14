namespace Pompom.Services;

internal interface INotificationService : IDisposable
{
    event EventHandler? Activated;

    bool Initialize();

    void Show(string title, string body);
}
