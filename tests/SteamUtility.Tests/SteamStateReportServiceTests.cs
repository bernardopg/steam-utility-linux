using SteamUtility.Core.Models;
using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamStateReportServiceTests
{
    public static void Build_SummarizesSteamStateFiles()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var steamAppsPath = Path.Combine(tempRoot, "steamapps");
        var libraryRoot = tempRoot;
        var compatDataPath = Path.Combine(steamAppsPath, "compatdata", "570", "pfx");
        var manifestPath = Path.Combine(steamAppsPath, "appmanifest_570.acf");
        var configPath = Path.Combine(tempRoot, "config");
        var userdataConfigPath = Path.Combine(tempRoot, "userdata", "12345678901234567", "config");

        Directory.CreateDirectory(compatDataPath);
        Directory.CreateDirectory(configPath);
        Directory.CreateDirectory(userdataConfigPath);

        File.WriteAllText(
            manifestPath,
            "\"AppState\"\n{\n  \"appid\"\t\t\"570\"\n  \"name\"\t\t\"Dota 2\"\n  \"installdir\"\t\t\"dota 2 beta\"\n  \"StateFlags\"\t\t\"4\"\n}");
        File.WriteAllText(
            Path.Combine(tempRoot, "config", "libraryfolders.vdf"),
            "\"libraryfolders\"\n{\n  \"0\"\n  {\n    \"path\"\t\t\"" + tempRoot + "\"\n  }\n}");
        File.WriteAllText(
            Path.Combine(tempRoot, "config", "loginusers.vdf"),
            "\"users\"\n{\n  \"12345678901234567\"\n  {\n    \"AccountName\"\t\t\"primary\"\n    \"PersonaName\"\t\t\"Primary User\"\n    \"MostRecent\"\t\t\"1\"\n    \"Timestamp\"\t\t\"200\"\n  }\n  \"76543210987654321\"\n  {\n    \"AccountName\"\t\t\"secondary\"\n    \"PersonaName\"\t\t\"Secondary User\"\n    \"MostRecent\"\t\t\"0\"\n    \"Timestamp\"\t\t\"100\"\n  }\n}");
        File.WriteAllText(
            Path.Combine(configPath, "config.vdf"),
            "\"InstallConfigStore\"\n{\n  \"Software\"\n  {\n    \"Valve\"\n    {\n      \"Steam\"\n      {\n        \"CompatToolMapping\"\n        {\n          \"570\"\n          {\n            \"name\"\t\t\"proton_experimental\"\n            \"Priority\"\t\t\"250\"\n          }\n        }\n      }\n    }\n  }\n}");
        File.WriteAllText(
            Path.Combine(userdataConfigPath, "localconfig.vdf"),
            "\"UserLocalConfigStore\"\n{\n  \"Software\"\n  {\n    \"Valve\"\n    {\n      \"Steam\"\n      {\n        \"apps\"\n        {\n          \"570\"\n          {\n            \"LastPlayed\"\t\t\"100\"\n          }\n        }\n      }\n    }\n  }\n}");

        try
        {
            var installation = new SteamInstallation(
                RootPath: tempRoot,
                SteamAppsPath: steamAppsPath,
                LibraryFolders: [new SteamLibraryFolder("0", libraryRoot, true)]);

            var service = new SteamStateReportService();
            var summary = service.Build(installation);

            if (summary.LibraryCount != 1) throw new Exception("Unexpected library count.");
            if (summary.InstalledAppCount != 1) throw new Exception("Unexpected app count.");
            if (summary.CompatDataCount != 1) throw new Exception("Unexpected compatdata count.");
            if (summary.CompatibilityToolCount != 0) throw new Exception("Unexpected compatibility tool count.");
            if (summary.ExplicitCompatibilityMappings != 1) throw new Exception("Unexpected mapping count.");
            if (summary.ReportEntryCount != 1) throw new Exception("Unexpected report count.");
            if (summary.LoginUserCount != 2) throw new Exception("Unexpected login user count.");
            if (summary.ActiveSteamId != 12345678901234567UL) throw new Exception("Unexpected active Steam ID.");
            if (summary.ActiveAccountName != "primary") throw new Exception("Unexpected active account.");
            if (summary.UserConfigFileCount != 1) throw new Exception("Unexpected user config file count.");
            if (summary.UserAppScopeCount != 1) throw new Exception("Unexpected user app scope count.");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
