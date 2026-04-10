namespace SteamUtility.Core.Models;

public sealed record SteamEnvironmentSummary(
    string RootPath,
    int LibraryCount,
    int InstalledAppCount,
    int CompatDataCount,
    int CompatibilityToolCount,
    int ExplicitCompatibilityMappings,
    int ReportEntryCount,
    int LoginUserCount,
    ulong? ActiveSteamId,
    string? ActiveAccountName,
    int UserConfigFileCount,
    int UserAppScopeCount);
