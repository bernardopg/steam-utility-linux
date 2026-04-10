using SteamUtility.Core.Abstractions;
using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamInstallationService(ISteamLocator steamLocator)
{
    private readonly SteamLibraryFoldersParser _libraryFoldersParser = new();

    public SteamInstallation? TryResolve()
    {
        var root = steamLocator.TryGetSteamRoot();
        if (string.IsNullOrWhiteSpace(root))
        {
            return null;
        }

        var steamAppsPath = Path.Combine(root, "steamapps");
        var libraryFoldersPath = Path.Combine(steamAppsPath, "libraryfolders.vdf");

        var libraryFolders = File.Exists(libraryFoldersPath)
            ? _libraryFoldersParser.Parse(File.ReadAllText(libraryFoldersPath))
            : [new SteamLibraryFolder("0", steamAppsPath, true)];

        return new SteamInstallation(
            RootPath: root,
            SteamAppsPath: steamAppsPath,
            LibraryFolders: libraryFolders);
    }
}
