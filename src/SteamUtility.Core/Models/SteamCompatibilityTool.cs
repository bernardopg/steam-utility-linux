namespace SteamUtility.Core.Models;

public sealed record SteamCompatibilityTool(
    string Name,
    string RootPath,
    bool IsCustom)
{
    public int SchemaVersion => 1;
}
