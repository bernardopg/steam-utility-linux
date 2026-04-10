using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class LinuxSteamClientLibraryTests
{
    public static void FindLibraryPath_Prefers64BitClient_WhenMultipleCandidatesExist()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(tempRoot, "linux64"));
        Directory.CreateDirectory(Path.Combine(tempRoot, "linux32"));
        File.WriteAllText(Path.Combine(tempRoot, "linux64", "steamclient.so"), string.Empty);
        File.WriteAllText(Path.Combine(tempRoot, "linux32", "steamclient.so"), string.Empty);

        try
        {
            var result = LinuxSteamClientLibrary.FindLibraryPath(tempRoot);

            if (result != Path.Combine(tempRoot, "linux64", "steamclient.so"))
            {
                throw new Exception($"Expected linux64 client, got '{result}'.");
            }
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    public static void FindLibraryPath_ReturnsNull_WhenClientLibraryDoesNotExist()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var result = LinuxSteamClientLibrary.FindLibraryPath(tempRoot);

            if (result is not null)
            {
                throw new Exception($"Expected null result, got '{result}'.");
            }
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
}
