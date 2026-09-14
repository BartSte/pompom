using Pompom.Core;
using Pompom.Services;

namespace Pompom.Tests;

[TestClass]
public sealed class NotificationTests
{
    [TestMethod]
    public void WorkCompletionNamesLongBreak()
    {
        NotificationMessage? message = SessionNotificationFactory.Create(
            SessionType.Work,
            SessionType.LongBreak,
            PompomSettings.Default);

        Assert.IsNotNull(message);
        Assert.AreEqual("Work session complete", message.Title);
        StringAssert.Contains(message.Body, "long break");
    }

    [TestMethod]
    public void NotificationSettingsSuppressMatchingSession()
    {
        NotificationMessage? workMessage = SessionNotificationFactory.Create(
            SessionType.Work,
            SessionType.ShortBreak,
            PompomSettings.Default with { NotifyAfterWork = false });
        NotificationMessage? breakMessage = SessionNotificationFactory.Create(
            SessionType.ShortBreak,
            SessionType.Work,
            PompomSettings.Default with { NotifyAfterBreak = false });

        Assert.IsNull(workMessage);
        Assert.IsNull(breakMessage);
    }

    [TestMethod]
    public void NativeNotificationIsUsedWhenAvailable()
    {
        var adapter = new FakeNotificationAdapter { IsSupported = true };
        int fallbackCount = 0;
        using var service = new WindowsNotificationService(
            adapter,
            (_, _) => fallbackCount++);

        bool initialized = service.Initialize();
        service.Show("Title", "Body");

        Assert.IsTrue(initialized);
        Assert.IsTrue(adapter.RegisterCalled);
        Assert.AreEqual(1, adapter.ShowCount);
        Assert.AreEqual(0, fallbackCount);
    }

    [TestMethod]
    public void UnsupportedNativeNotificationUsesTrayFallback()
    {
        var adapter = new FakeNotificationAdapter { IsSupported = false };
        NotificationMessage? fallbackMessage = null;
        using var service = new WindowsNotificationService(
            adapter,
            (title, body) => fallbackMessage = new NotificationMessage(title, body));

        bool initialized = service.Initialize();
        service.Show("Title", "Body");

        Assert.IsFalse(initialized);
        Assert.IsFalse(adapter.RegisterCalled);
        Assert.AreEqual(new NotificationMessage("Title", "Body"), fallbackMessage);
    }

    [TestMethod]
    public void NativeShowFailureUsesTrayFallback()
    {
        var adapter = new FakeNotificationAdapter
        {
            IsSupported = true,
            ShowException = new InvalidOperationException(),
        };
        int fallbackCount = 0;
        using var service = new WindowsNotificationService(
            adapter,
            (_, _) => fallbackCount++);
        service.Initialize();

        service.Show("Title", "Body");

        Assert.AreEqual(1, fallbackCount);
    }

    [TestMethod]
    public void NotificationActivationIsForwarded()
    {
        var adapter = new FakeNotificationAdapter { IsSupported = true };
        using var service = new WindowsNotificationService(adapter, (_, _) => { });
        int activationCount = 0;
        service.Activated += (_, _) => activationCount++;
        service.Initialize();

        adapter.RaiseActivated();

        Assert.AreEqual(1, activationCount);
    }

    private sealed class FakeNotificationAdapter : IAppNotificationAdapter
    {
        public event EventHandler? Activated;

        public bool IsSupported { get; init; }

        public bool RegisterCalled { get; private set; }

        public int ShowCount { get; private set; }

        public Exception? ShowException { get; init; }

        public void Register()
        {
            RegisterCalled = true;
        }

        public void Show(string title, string body)
        {
            ShowCount++;
            if (ShowException is not null)
            {
                throw ShowException;
            }
        }

        public void RaiseActivated()
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
        }
    }
}
