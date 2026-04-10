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
}
