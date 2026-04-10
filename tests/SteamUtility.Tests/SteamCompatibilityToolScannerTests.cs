using SteamUtility.Core.Models;
using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamCompatibilityToolScannerTests
{
    public static void Scan_FindsCustomAndBundledCompatibilityTools()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var customToolsRoot = Path.Combine(tempRoot, "compatibilitytools.d");
        var commonRoot = Path.Combine(tempRoot, "steamapps", "common");

        Directory.CreateDirectory(Path.Combine(customToolsRoot, "MyCustomTool"));
        Directory.CreateDirectory(Path.Combine(commonRoot, "GE-Proton9-0"));
        Directory.CreateDirectory(Path.Combine(commonRoot, "SteamLinuxRuntime"));
        Directory.CreateDirectory(Path.Combine(commonRoot, "NotATool"));

        try
        {
            var installation = new SteamInstallation(
                RootPath: tempRoot,
                SteamAppsPath: Path.Combine(tempRoot, "steamapps"),
                LibraryFolders: [new SteamLibraryFolder("0", tempRoot, true)]);

            var scanner = new SteamCompatibilityToolScanner();
            var tools = scanner.Scan(installation);

            if (tools.Count != 3) throw new Exception($"Expected 3 tools, got {tools.Count}.");

            var custom = tools.SingleOrDefault(tool => tool.Name == "MyCustomTool");
            var proton = tools.SingleOrDefault(tool => tool.Name == "GE-Proton9-0");
            var runtime = tools.SingleOrDefault(tool => tool.Name == "SteamLinuxRuntime");

            if (custom is null || !custom.IsCustom) throw new Exception("Expected custom tool to be marked as custom.");
            if (proton is null || proton.IsCustom) throw new Exception("Expected Proton tool to be discovered as bundled.");
            if (runtime is null || runtime.IsCustom) throw new Exception("Expected SteamLinuxRuntime to be discovered as bundled.");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
