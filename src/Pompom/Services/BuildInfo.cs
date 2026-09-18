using System.Reflection;

namespace Pompom.Services;

internal static class BuildInfo
{
    private static readonly Assembly ApplicationAssembly = typeof(BuildInfo).Assembly;
    private static readonly string InformationalVersion = ApplicationAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
        ?? string.Empty;

    public static string Version { get; } = GetVersion();

    public static string GitCommit { get; } = GetGitCommit();

    private static string GetVersion()
    {
        if (string.IsNullOrWhiteSpace(InformationalVersion))
        {
            return ApplicationAssembly.GetName().Version?.ToString(3) ?? "unknown";
        }

        int metadataSeparator = InformationalVersion.IndexOf('+', StringComparison.Ordinal);
        return metadataSeparator < 0
            ? InformationalVersion
            : InformationalVersion[..metadataSeparator];
    }

    private static string GetGitCommit()
    {
        int metadataSeparator = InformationalVersion.IndexOf('+', StringComparison.Ordinal);
        if (metadataSeparator < 0)
        {
            return "unknown";
        }

        string commit = InformationalVersion[(metadataSeparator + 1)..];
        return commit.Length >= 12 && commit.All(Uri.IsHexDigit)
            ? commit[..12]
            : "unknown";
    }
}
