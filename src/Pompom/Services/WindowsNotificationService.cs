using System.IO;

namespace Pompom.Services;

internal sealed class WindowsNotificationService(
    IAppNotificationAdapter adapter,
    Action<string, string> showFallback) : INotificationService
{
    private readonly IAppNotificationAdapter _adapter = adapter;
    private readonly Action<string, string> _showFallback = showFallback;
    private bool _nativeReady;

    public event EventHandler? Activated;

    public bool Initialize()
    {
        try
        {
            if (!_adapter.IsSupported)
            {
                return false;
            }

            _adapter.Activated += OnActivated;
            _adapter.Register();
            _nativeReady = true;
            return true;
        }
        catch (Exception exception) when (IsExpectedNotificationFailure(exception))
        {
            _adapter.Activated -= OnActivated;
            _nativeReady = false;
            return false;
        }
    }

    public void Show(NotificationMessage message)
    {
        if (_nativeReady)
        {
            try
            {
                _adapter.Show(message);
                return;
            }
            catch (Exception exception) when (IsExpectedNotificationFailure(exception))
            {
                _nativeReady = false;
            }
        }

        _showFallback(message.Title, message.Body);
    }

    public void Dispose()
    {
        _adapter.Activated -= OnActivated;
        _adapter.Dispose();
    }

    private static bool IsExpectedNotificationFailure(Exception exception)
    {
        return exception is InvalidOperationException
            or NotSupportedException
            or UnauthorizedAccessException
            or IOException
            or DllNotFoundException
            or EntryPointNotFoundException
            or TypeInitializationException
            or TypeLoadException
            or System.Runtime.InteropServices.COMException;
    }

    private void OnActivated(object? sender, EventArgs eventArgs)
    {
        Activated?.Invoke(this, EventArgs.Empty);
    }
}
