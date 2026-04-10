namespace SteamUtility.Core.Models;

public sealed record SteamInstallation(
    string RootPath,
    string SteamAppsPath,
    IReadOnlyList<SteamLibraryFolder> LibraryFolders);
