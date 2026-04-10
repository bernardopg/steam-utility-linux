namespace SteamUtility.Core.Models;

public sealed record SteamCompatDataEntry(
    int AppId,
    string CompatDataPath,
    string? PfxPath,
    string LibraryPath)
{
    public int SchemaVersion => 1;
}
