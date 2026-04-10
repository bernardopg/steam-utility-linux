namespace SteamUtility.Core.Models;

public sealed record SteamUserConfigEntry(
    ulong SteamId,
    string ConfigType,
    string ConfigPath,
    IReadOnlyList<int> AppIds)
{
    public int SchemaVersion => 1;
}
