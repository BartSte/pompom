namespace Pompom.Services;

internal interface IAppNotificationAdapter : IDisposable
{
    event EventHandler? Activated;

    bool IsSupported { get; }

    void Register();

    void Show(string title, string body);
}
