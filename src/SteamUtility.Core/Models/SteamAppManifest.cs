namespace SteamUtility.Core.Models;

public sealed record SteamAppManifest(
    int AppId,
    string Name,
    string InstallDirectory,
    string ManifestPath,
    string LibraryPath,
    string StateFlags)
{
    public int SchemaVersion => 1;
}
