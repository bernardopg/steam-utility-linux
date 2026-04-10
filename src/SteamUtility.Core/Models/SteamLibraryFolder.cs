namespace SteamUtility.Core.Models;

public sealed record SteamLibraryFolder(
    string Key,
    string Path,
    bool IsDefault = false,
    IReadOnlyList<int>? AppIds = null)
{
    public int SchemaVersion => 1;
}
