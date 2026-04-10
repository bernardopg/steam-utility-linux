namespace SteamUtility.Core.Models;

public sealed record SteamCompatibilityReportEntry(
    int AppId,
    string Name,
    string? InstallDirectory,
    bool HasCompatData,
    string? CompatDataPath,
    string? CompatPfxPath,
    string? LibraryPath,
    string? AssignedTool,
    string? AssignedToolPriority,
    string? AssignedToolConfig,
    string? ResolvedToolPath,
    bool ResolvedToolIsCustom)
{
    public int SchemaVersion => 1;
}
