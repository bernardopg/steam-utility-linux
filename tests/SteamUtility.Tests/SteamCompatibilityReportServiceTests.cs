using SteamUtility.Core.Models;
using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamCompatibilityReportServiceTests
{
    public static void Build_WithMissingFiles_ReturnsEmptyReport()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(tempRoot, "steamapps"));
        Directory.CreateDirectory(Path.Combine(tempRoot, "config"));

        try
        {
            var installation = new SteamInstallation(
                RootPath: tempRoot,
                SteamAppsPath: Path.Combine(tempRoot, "steamapps"),
                LibraryFolders: [new SteamLibraryFolder("0", tempRoot, true)]);

            var service = new SteamCompatibilityReportService();
            var result = service.Build(installation);

            if (result.Count != 0) throw new Exception($"Expected empty report, got {result.Count} entries.");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    public static void Build_MergesAppCompatDataAndMappings()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var steamAppsPath = Path.Combine(tempRoot, "steamapps");
        var compatDataEntryPath = Path.Combine(steamAppsPath, "compatdata", "570");
        var compatDataPfxPath = Path.Combine(compatDataEntryPath, "pfx");
        var manifestPath = Path.Combine(steamAppsPath, "appmanifest_570.acf");
        var configPath = Path.Combine(tempRoot, "config");
        var customToolsPath = Path.Combine(tempRoot, "compatibilitytools.d", "proton_experimental");
        var bundledToolsPath = Path.Combine(steamAppsPath, "common", "Proton Experimental");

        Directory.CreateDirectory(compatDataPfxPath);
        Directory.CreateDirectory(configPath);
        Directory.CreateDirectory(customToolsPath);
        Directory.CreateDirectory(bundledToolsPath);
        File.WriteAllText(
            manifestPath,
            "\"AppState\"\n{\n  \"appid\"\t\t\"570\"\n  \"name\"\t\t\"Dota 2\"\n  \"installdir\"\t\t\"dota 2 beta\"\n  \"StateFlags\"\t\t\"4\"\n}");
        File.WriteAllText(
            Path.Combine(configPath, "config.vdf"),
            "\"InstallConfigStore\"\n{\n  \"Software\"\n  {\n    \"Valve\"\n    {\n      \"Steam\"\n      {\n        \"CompatToolMapping\"\n        {\n          \"570\"\n          {\n            \"name\"\t\t\"proton_experimental\"\n            \"Priority\"\t\t\"250\"\n          }\n        }\n      }\n    }\n  }\n}");

        try
        {
            var installation = new SteamInstallation(
                RootPath: tempRoot,
                SteamAppsPath: steamAppsPath,
                LibraryFolders: [new SteamLibraryFolder("0", tempRoot, true)]);

            var service = new SteamCompatibilityReportService();
            var result = service.Build(installation);

            if (result.Count != 1) throw new Exception($"Expected 1 report entry, got {result.Count}.");

            var entry = result[0];
            if (entry.AppId != 570) throw new Exception("Unexpected AppId.");
            if (entry.Name != "Dota 2") throw new Exception("Unexpected app name.");
            if (entry.InstallDirectory != "dota 2 beta") throw new Exception("Unexpected install directory.");
            if (!entry.HasCompatData) throw new Exception("Expected compatdata to be detected.");
            if (entry.AssignedTool != "proton_experimental") throw new Exception("Unexpected assigned tool.");
            if (entry.AssignedToolPriority != "250") throw new Exception("Unexpected tool priority.");
            if (entry.CompatDataPath != compatDataEntryPath) throw new Exception("Unexpected compatdata path.");
            if (entry.CompatPfxPath != compatDataPfxPath) throw new Exception("Unexpected pfx path.");
            if (entry.LibraryPath != tempRoot) throw new Exception("Unexpected library path.");
            if (entry.ResolvedToolPath != customToolsPath) throw new Exception("Expected custom tool to win precedence.");
            if (!entry.ResolvedToolIsCustom) throw new Exception("Expected resolved tool to be custom.");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
