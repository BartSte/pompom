namespace Pompom.Services;

internal sealed class SingleInstanceCoordinator : IDisposable
{
    private const string MutexName = @"Local\Pompom.SingleInstance.Mutex";
    private const string ActivationEventName = @"Local\Pompom.SingleInstance.Activate";

    private readonly Mutex _mutex;
    private readonly EventWaitHandle? _activationEvent;
    private readonly CancellationTokenSource _cancellation = new();
    private Task? _listener;
    private readonly bool _ownsMutex;

    public SingleInstanceCoordinator()
    {
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        IsPrimary = createdNew;
        _ownsMutex = createdNew;

        if (!IsPrimary)
        {
            return;
        }

        _activationEvent = new EventWaitHandle(
            false,
            EventResetMode.AutoReset,
            ActivationEventName);
    }

    public event EventHandler? ActivationRequested;

    public bool IsPrimary { get; }

    public void StartListening()
    {
        if (IsPrimary && _listener is null)
        {
            _listener = Task.Run(ListenForActivation);
        }
    }

    public void SignalPrimaryInstance()
    {
        if (IsPrimary)
        {
            return;
        }

        try
        {
            using EventWaitHandle activationEvent = EventWaitHandle.OpenExisting(
                ActivationEventName);
            activationEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // The first process can close between the mutex check and this signal.
        }
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        _activationEvent?.Set();

        try
        {
            _listener?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
            // The application is already closing.
        }

        _activationEvent?.Dispose();
        _cancellation.Dispose();

        if (_ownsMutex)
        {
            _mutex.ReleaseMutex();
        }

        _mutex.Dispose();
    }

    private void ListenForActivation()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            _activationEvent!.WaitOne();
            if (!_cancellation.IsCancellationRequested)
            {
                ActivationRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
