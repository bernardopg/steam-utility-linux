using SteamUtility.Core.Models;
using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamCompatDataScannerTests
{
    public static void Scan_FindsCompatDataAndPfx()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var libraryRoot = Path.Combine(tempRoot, "library");
        var compatDataRoot = Path.Combine(libraryRoot, "steamapps", "compatdata");
        var firstCompat = Path.Combine(compatDataRoot, "123");
        var secondCompat = Path.Combine(compatDataRoot, "456");
        var pfxPath = Path.Combine(firstCompat, "pfx");

        Directory.CreateDirectory(pfxPath);
        Directory.CreateDirectory(secondCompat);

        try
        {
            var installation = new SteamInstallation(
                RootPath: tempRoot,
                SteamAppsPath: Path.Combine(libraryRoot, "steamapps"),
                LibraryFolders: [new SteamLibraryFolder("0", libraryRoot, true)]);

            var scanner = new SteamCompatDataScanner();
            var entries = scanner.Scan(installation);

            if (entries.Count != 2) throw new Exception($"Expected 2 compatdata entries, got {entries.Count}.");
            if (entries[0].AppId != 123) throw new Exception("Expected AppId 123 first.");
            if (entries[0].PfxPath != pfxPath) throw new Exception("Expected pfx path for AppId 123.");
            if (entries[1].AppId != 456) throw new Exception("Expected AppId 456 second.");
            if (entries[1].PfxPath is not null) throw new Exception("Expected AppId 456 to have no pfx path.");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
