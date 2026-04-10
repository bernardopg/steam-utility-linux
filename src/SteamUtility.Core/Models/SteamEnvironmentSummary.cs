namespace SteamUtility.Core.Models;

public sealed record SteamEnvironmentSummary(
    string RootPath,
    int LibraryCount,
    int InstalledAppCount,
    int CompatDataCount,
    int CompatibilityToolCount,
    int ExplicitCompatibilityMappings,
    int ReportEntryCount);
