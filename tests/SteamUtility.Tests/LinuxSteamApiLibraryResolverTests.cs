using SteamUtility.Core.Models;
using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class LinuxSteamApiLibraryResolverTests
{
    public static void FindLibraryPath_PrefersLocalLibraryFromSteamGameFolders()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var commonPath = Path.Combine(tempRoot, "steamapps", "common", "Some Game", "bin", "linux64");
        Directory.CreateDirectory(commonPath);
        var expectedPath = Path.Combine(commonPath, "libsteam_api.so");
        WriteElfStub(expectedPath, is64Bit: true);

        try
        {
            var installation = new SteamInstallation(
                RootPath: tempRoot,
                SteamAppsPath: Path.Combine(tempRoot, "steamapps"),
                LibraryFolders: [new SteamLibraryFolder("0", tempRoot, true)]);

            var result = LinuxSteamApiLibraryResolver.FindLibraryPath(installation);

            if (result != expectedPath)
            {
                throw new Exception($"Expected '{expectedPath}', got '{result}'.");
            }
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    public static void FindLibraryPath_UsesEnvironmentOverrideWhenPresent()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        var overridePath = Path.Combine(tempRoot, "libsteam_api.so");
        WriteElfStub(overridePath, is64Bit: true);
        var previousValue = Environment.GetEnvironmentVariable("STEAMWORKS_NATIVE_LIBRARY");

        try
        {
            Environment.SetEnvironmentVariable("STEAMWORKS_NATIVE_LIBRARY", overridePath);

            var installation = new SteamInstallation(
                RootPath: tempRoot,
                SteamAppsPath: Path.Combine(tempRoot, "steamapps"),
                LibraryFolders: [new SteamLibraryFolder("0", tempRoot, true)]);

            var result = LinuxSteamApiLibraryResolver.FindLibraryPath(installation);

            if (result != overridePath)
            {
                throw new Exception($"Expected override path '{overridePath}', got '{result}'.");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("STEAMWORKS_NATIVE_LIBRARY", previousValue);
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static void WriteElfStub(string path, bool is64Bit)
    {
        var elfClass = is64Bit ? (byte)2 : (byte)1;
        File.WriteAllBytes(path, [0x7F, (byte)'E', (byte)'L', (byte)'F', elfClass, 0x01, 0x01, 0x00]);
    }
}
