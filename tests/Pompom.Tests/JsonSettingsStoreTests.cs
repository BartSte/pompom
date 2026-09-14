using System.Text.Json;
using Pompom.Core;
using Pompom.Services;

namespace Pompom.Tests;

[TestClass]
public sealed class JsonSettingsStoreTests
{
    [TestMethod]
    public void MissingFileReturnsDefaults()
    {
        using var directory = new TemporaryDirectory();
        var store = new JsonSettingsStore(Path.Combine(directory.Path, "settings.json"));

        PompomSettings settings = store.Load();

        Assert.AreEqual(PompomSettings.Default, settings);
    }

    [TestMethod]
    public void SaveAndLoadRoundTrip()
    {
        using var directory = new TemporaryDirectory();
        string path = Path.Combine(directory.Path, "settings.json");
        var store = new JsonSettingsStore(path);
        var expected = PompomSettings.Default with
        {
            WorkMinutes = 45,
            ShortBreakMinutes = 8,
            LongBreakMinutes = 22,
            LongBreakEnabled = false,
            AutoStartBreaks = false,
            AutoStartWork = true,
            NotifyAfterWork = false,
            NotifyAfterBreak = false,
        };

        store.Save(expected);
        PompomSettings actual = store.Load();

        Assert.AreEqual(expected, actual);
        StringAssert.Contains(File.ReadAllText(path), "\"workMinutes\": 45");
    }

    [TestMethod]
    public void SaveReplacesExistingFileAndRemovesTemporaryFile()
    {
        using var directory = new TemporaryDirectory();
        string path = Path.Combine(directory.Path, "settings.json");
        var store = new JsonSettingsStore(path);
        store.Save(PompomSettings.Default);

        store.Save(PompomSettings.Default with { WorkMinutes = 30 });

        Assert.AreEqual(30, store.Load().WorkMinutes);
        Assert.AreEqual(1, Directory.GetFiles(directory.Path).Length);
    }

    [TestMethod]
    public void MalformedJsonReturnsDefaults()
    {
        using var directory = new TemporaryDirectory();
        string path = Path.Combine(directory.Path, "settings.json");
        File.WriteAllText(path, "{ not json");
        var store = new JsonSettingsStore(path);

        Assert.AreEqual(PompomSettings.Default, store.Load());
    }

    [TestMethod]
    public void InvalidDurationsReturnTheirDefaults()
    {
        using var directory = new TemporaryDirectory();
        string path = Path.Combine(directory.Path, "settings.json");
        File.WriteAllText(
            path,
            JsonSerializer.Serialize(new
            {
                schemaVersion = 99,
                workMinutes = 0,
                shortBreakMinutes = 181,
                longBreakMinutes = -5,
                autoStartWork = true,
            }));
        var store = new JsonSettingsStore(path);

        PompomSettings settings = store.Load();

        Assert.AreEqual(1, settings.SchemaVersion);
        Assert.AreEqual(25, settings.WorkMinutes);
        Assert.AreEqual(5, settings.ShortBreakMinutes);
        Assert.AreEqual(15, settings.LongBreakMinutes);
        Assert.IsTrue(settings.AutoStartWork);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Pompom.Tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }
    }
}
