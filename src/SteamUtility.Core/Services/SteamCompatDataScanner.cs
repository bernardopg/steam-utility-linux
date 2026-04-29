using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamCompatDataScanner
{
    public IReadOnlyList<SteamCompatDataEntry> Scan(SteamInstallation installation)
    {
        var results = new List<SteamCompatDataEntry>();

        foreach (var library in installation.LibraryFolders)
        {
            var steamAppsPath = ResolveSteamAppsPath(library.Path);
            var compatDataRoot = Path.Combine(steamAppsPath, "compatdata");
            if (!Directory.Exists(compatDataRoot))
            {
                continue;
            }

            foreach (var appDirectory in Directory.EnumerateDirectories(compatDataRoot))
            {
                var directoryName = Path.GetFileName(appDirectory);
                if (!int.TryParse(directoryName, out var appId))
                {
                    continue;
                }

                var pfxPath = Path.Combine(appDirectory, "pfx");
                results.Add(new SteamCompatDataEntry(
                    AppId: appId,
                    CompatDataPath: appDirectory,
                    PfxPath: Directory.Exists(pfxPath) ? pfxPath : null,
                    LibraryPath: library.Path));
            }
        }

        return results
            .OrderBy(static entry => entry.AppId)
            .ToArray();
    }

    private static string ResolveSteamAppsPath(string libraryPath) =>
        SteamPaths.ResolveSteamAppsPath(libraryPath);
}
