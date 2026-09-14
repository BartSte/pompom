using System.IO;
using System.Text.Json;
using Pompom.Core;

namespace Pompom.Services;

internal sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    private readonly string _settingsPath;

    public JsonSettingsStore(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? GetDefaultSettingsPath();
    }

    public PompomSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return PompomSettings.Default;
            }

            string json = File.ReadAllText(_settingsPath);
            PompomSettings? settings = JsonSerializer.Deserialize<PompomSettings>(
                json,
                SerializerOptions);
            return settings?.Normalize() ?? PompomSettings.Default;
        }
        catch (JsonException)
        {
            return PompomSettings.Default;
        }
        catch (IOException)
        {
            return PompomSettings.Default;
        }
        catch (UnauthorizedAccessException)
        {
            return PompomSettings.Default;
        }
    }

    public void Save(PompomSettings settings)
    {
        string? directory = Path.GetDirectoryName(_settingsPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("The settings path has no directory.");
        }

        Directory.CreateDirectory(directory);
        string temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(_settingsPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            string json = JsonSerializer.Serialize(settings.Normalize(), SerializerOptions);
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(_settingsPath))
            {
                File.Replace(temporaryPath, _settingsPath, null);
            }
            else
            {
                File.Move(temporaryPath, _settingsPath);
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static string GetDefaultSettingsPath()
    {
        string localAppData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "Pompom", "settings.json");
    }
}
