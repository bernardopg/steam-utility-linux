using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamLibraryScanner
{
    private readonly SteamAppManifestParser _manifestParser = new();

    public IReadOnlyList<SteamAppManifest> ScanInstalledApps(SteamInstallation installation)
    {
        var results = new List<SteamAppManifest>();

        foreach (var library in installation.LibraryFolders)
        {
            var steamAppsPath = ResolveSteamAppsPath(library.Path);
            if (!Directory.Exists(steamAppsPath))
            {
                continue;
            }

            foreach (var manifestPath in Directory.EnumerateFiles(steamAppsPath, "appmanifest_*.acf", SearchOption.TopDirectoryOnly))
            {
                var manifest = _manifestParser.Parse(manifestPath, library.Path);
                if (manifest is not null)
                {
                    results.Add(manifest);
                }
            }
        }

        return results
            .OrderBy(static app => app.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static app => app.AppId)
            .ToArray();
    }

    private static string ResolveSteamAppsPath(string libraryPath) =>
        SteamPaths.ResolveSteamAppsPath(libraryPath);
}
