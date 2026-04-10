using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamAppManifestParserTests
{
    public static void Parse_AppManifest_ReturnsExpectedValues()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        var manifestPath = Path.Combine(tempRoot, "appmanifest_123.acf");
        File.WriteAllText(
            manifestPath,
            "\"AppState\"\n{\n  \"appid\"\t\t\"123\"\n  \"name\"\t\t\"Test Game\"\n  \"installdir\"\t\t\"TestGame\"\n  \"StateFlags\"\t\t\"4\"\n}");

        try
        {
            var parser = new SteamAppManifestParser();
            var manifest = parser.Parse(manifestPath, "/games/library");

            if (manifest is null) throw new Exception("Expected manifest to be parsed.");
            if (manifest.AppId != 123) throw new Exception("Unexpected AppId.");
            if (manifest.Name != "Test Game") throw new Exception("Unexpected manifest name.");
            if (manifest.InstallDirectory != "TestGame") throw new Exception("Unexpected install directory.");
            if (manifest.ManifestPath != manifestPath) throw new Exception("Unexpected manifest path.");
            if (manifest.LibraryPath != "/games/library") throw new Exception("Unexpected library path.");
            if (manifest.StateFlags != "4") throw new Exception("Unexpected state flags.");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
