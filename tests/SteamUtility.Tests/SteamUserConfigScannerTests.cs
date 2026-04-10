using SteamUtility.Core.Models;
using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamUserConfigScannerTests
{
    public static void Scan_FindsLocalAndSharedConfigs()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var userdataRoot = Path.Combine(tempRoot, "userdata", "12345678901234567", "config");
        Directory.CreateDirectory(userdataRoot);

        File.WriteAllText(
            Path.Combine(userdataRoot, "localconfig.vdf"),
            "\"UserLocalConfigStore\"\n{\n  \"Software\"\n  {\n    \"Valve\"\n    {\n      \"Steam\"\n      {\n        \"apps\"\n        {\n          \"570\"\n          {\n            \"LastPlayed\"\t\t\"100\"\n          }\n        }\n      }\n    }\n  }\n}");

        File.WriteAllText(
            Path.Combine(userdataRoot, "sharedconfig.vdf"),
            "\"UserLocalConfigStore\"\n{\n  \"Software\"\n  {\n    \"Valve\"\n    {\n      \"Steam\"\n      {\n        \"apps\"\n        {\n          \"730\"\n          {\n            \"LastPlayed\"\t\t\"200\"\n          }\n        }\n      }\n    }\n  }\n}");

        try
        {
            var installation = new SteamInstallation(
                RootPath: tempRoot,
                SteamAppsPath: Path.Combine(tempRoot, "steamapps"),
                LibraryFolders: [new SteamLibraryFolder("0", tempRoot, true)]);

            var scanner = new SteamUserConfigScanner();
            var entries = scanner.Scan(installation);

            if (entries.Count != 2) throw new Exception($"Expected 2 config files, got {entries.Count}.");

            var local = entries.SingleOrDefault(entry => entry.ConfigType == "local");
            var shared = entries.SingleOrDefault(entry => entry.ConfigType == "shared");

            if (local is null || local.SteamId != 12345678901234567UL) throw new Exception("Expected local config entry.");
            if (!local.AppIds.Contains(570)) throw new Exception("Expected local config to include AppId 570.");
            if (shared is null || shared.SteamId != 12345678901234567UL) throw new Exception("Expected shared config entry.");
            if (!shared.AppIds.Contains(730)) throw new Exception("Expected shared config to include AppId 730.");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
