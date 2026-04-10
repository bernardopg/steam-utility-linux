using SteamUtility.Core.Models;
using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamLibraryScannerTests
{
    public static void ScanInstalledApps_FindsManifestsAcrossLibraries()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var firstLibrary = Path.Combine(tempRoot, "first");
        var secondLibrary = Path.Combine(tempRoot, "second");

        CreateManifest(firstLibrary, 300, "Alpha Game", "AlphaGame", "4");
        CreateManifest(secondLibrary, 200, "Zulu Game", "ZuluGame", "2");

        try
        {
            var installation = new SteamInstallation(
                RootPath: tempRoot,
                SteamAppsPath: Path.Combine(tempRoot, "steamapps"),
                LibraryFolders:
                [
                    new SteamLibraryFolder("0", firstLibrary, true),
                    new SteamLibraryFolder("1", secondLibrary)
                ]);

            var scanner = new SteamLibraryScanner();
            var apps = scanner.ScanInstalledApps(installation);

            if (apps.Count != 2) throw new Exception($"Expected 2 apps, got {apps.Count}.");
            if (apps[0].AppId != 300) throw new Exception("Expected Alpha Game to sort first.");
            if (apps[0].Name != "Alpha Game") throw new Exception("Unexpected first app name.");
            if (apps[1].AppId != 200) throw new Exception("Expected Zulu Game to sort second by name.");
            if (apps[1].LibraryPath != secondLibrary) throw new Exception("Unexpected second library path.");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static void CreateManifest(string libraryRoot, int appId, string name, string installdir, string stateFlags)
    {
        var steamAppsPath = Path.Combine(libraryRoot, "steamapps");
        Directory.CreateDirectory(steamAppsPath);

        var manifestPath = Path.Combine(steamAppsPath, $"appmanifest_{appId}.acf");
        File.WriteAllText(
            manifestPath,
            $"\"AppState\"\n{{\n  \"appid\"\t\t\"{appId}\"\n  \"name\"\t\t\"{name}\"\n  \"installdir\"\t\t\"{installdir}\"\n  \"StateFlags\"\t\t\"{stateFlags}\"\n}}");
    }
}
