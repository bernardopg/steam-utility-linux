using SteamUtility.Core.Abstractions;

namespace SteamUtility.Core.Services;

public sealed class LinuxSteamLocator : ISteamLocator
{
    private static readonly string[] CandidatePaths =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "Steam")
    ];

    public string? TryGetSteamRoot()
    {
        return CandidatePaths.FirstOrDefault(Directory.Exists);
    }
}
