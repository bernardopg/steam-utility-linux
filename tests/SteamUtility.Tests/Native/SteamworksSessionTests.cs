using SteamUtility.Core.Models;
using SteamUtility.Core.Services;
using SteamUtility.Tests.Fakes;

namespace SteamUtility.Tests.Native;

public static class SteamworksSessionTests
{
    public static void Constructor_WhenInitFails_RestoresProcessState()
    {
        SteamApiNativeTestHost.Reset();

        var originalDirectory = Directory.GetCurrentDirectory();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var currentDirectory = Path.Combine(tempRoot, "cwd");
        var installationRoot = Path.Combine(tempRoot, "steam");
        var currentSteamAppIdPath = Path.Combine(currentDirectory, "steam_appid.txt");
        var baseSteamAppIdPath = Path.Combine(AppContext.BaseDirectory, "steam_appid.txt");
        var baseSnapshot = CaptureFileState(baseSteamAppIdPath);
        var originalSteamAppId = Environment.GetEnvironmentVariable("SteamAppId");
        var originalSteamGameId = Environment.GetEnvironmentVariable("SteamGameId");

        Directory.CreateDirectory(currentDirectory);
        File.WriteAllText(currentSteamAppIdPath, "original-current");

        var installation = FakeSteamInstallationFactory.Create(installationRoot, Path.Combine(installationRoot, "steamapps"));
        var resolver = FakeSteamApiLibrary.CreateResolver(FakeSteamApiLibrary.InitFailureLibraryPath);

        try
        {
            Directory.SetCurrentDirectory(currentDirectory);

            try
            {
                new SteamworksSession(installation, 70120, resolver);
                throw new Exception("Expected SteamAPI_Init failure.");
            }
            catch (SteamworksInitializationException ex)
            {
                if (ex.FailureReason != SteamworksInitializationFailure.ApiInitFailed)
                {
                    throw new Exception($"Expected ApiInitFailed, got '{ex.FailureReason}'.");
                }
            }

            if (Environment.GetEnvironmentVariable("SteamAppId") != originalSteamAppId)
            {
                throw new Exception("Expected SteamAppId to be restored after constructor failure.");
            }

            if (Environment.GetEnvironmentVariable("SteamGameId") != originalSteamGameId)
            {
                throw new Exception("Expected SteamGameId to be restored after constructor failure.");
            }

            AssertFileState(currentSteamAppIdPath, new FileSnapshot(true, "original-current"));
            AssertFileState(baseSteamAppIdPath, baseSnapshot);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            SteamApiNativeTestHost.Reset();

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    public static void Constructor_InitializesSessionAndRestoresStateOnDispose()
    {
        SteamApiNativeTestHost.Reset();

        var originalDirectory = Directory.GetCurrentDirectory();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var currentDirectory = Path.Combine(tempRoot, "cwd");
        var installationRoot = Path.Combine(tempRoot, "steam");
        var currentSteamAppIdPath = Path.Combine(currentDirectory, "steam_appid.txt");
        var baseSteamAppIdPath = Path.Combine(AppContext.BaseDirectory, "steam_appid.txt");
        var currentSnapshot = new FileSnapshot(true, "current-placeholder");
        var baseSnapshot = CaptureFileState(baseSteamAppIdPath);
        var originalSteamAppId = Environment.GetEnvironmentVariable("SteamAppId");
        var originalSteamGameId = Environment.GetEnvironmentVariable("SteamGameId");

        Directory.CreateDirectory(currentDirectory);
        File.WriteAllText(currentSteamAppIdPath, "current-placeholder");

        var installation = FakeSteamInstallationFactory.Create(installationRoot, Path.Combine(installationRoot, "steamapps"));
        var resolver = FakeSteamApiLibrary.CreateResolver(FakeSteamApiLibrary.FullLibraryPath);

        try
        {
            Directory.SetCurrentDirectory(currentDirectory);

            using (var session = new SteamworksSession(installation, 70120, resolver))
            {
                if (Environment.GetEnvironmentVariable("SteamAppId") != "70120")
                {
                    throw new Exception("Expected SteamAppId to be set while session is active.");
                }

                if (Environment.GetEnvironmentVariable("SteamGameId") != "70120")
                {
                    throw new Exception("Expected SteamGameId to be set while session is active.");
                }

                AssertFileState(currentSteamAppIdPath, new FileSnapshot(true, "70120"));

                session.EnsureCurrentUserStatsLoaded();
                session.RequestGlobalAchievementPercentages();

                if (!session.GetStat("hedGamesPlayed", out int gamesPlayed) || gamesPlayed != 7)
                {
                    throw new Exception("Expected integer stat default value.");
                }

                if (!session.GetStat("hedAccuracy", out float accuracy) || !Approximately(accuracy, 19.5f))
                {
                    throw new Exception("Expected float stat default value.");
                }

                if (!session.GetStat("hedAverageRate", out float averageRate) || !Approximately(averageRate, 2.25f))
                {
                    throw new Exception("Expected average-rate stat default value.");
                }

                if (!session.SetStat("hedGamesPlayed", 15))
                {
                    throw new Exception("Expected integer stat mutation to succeed.");
                }

                if (!session.SetStat("hedAccuracy", 44.25f))
                {
                    throw new Exception("Expected float stat mutation to succeed.");
                }

                if (!session.SetStat("hedAverageRate", 3.5f))
                {
                    throw new Exception("Expected average-rate stat mutation to succeed.");
                }

                if (!session.GetStat("hedGamesPlayed", out gamesPlayed) || gamesPlayed != 15)
                {
                    throw new Exception("Expected integer stat mutation to persist.");
                }

                if (!session.GetStat("hedAccuracy", out accuracy) || !Approximately(accuracy, 44.25f))
                {
                    throw new Exception("Expected float stat mutation to persist.");
                }

                if (!session.GetStat("hedAverageRate", out averageRate) || !Approximately(averageRate, 3.5f))
                {
                    throw new Exception("Expected average-rate stat mutation to persist.");
                }

                if (!session.GetAchievement("ACH_TUTORIAL_COMPLETED", out var achieved) || achieved)
                {
                    throw new Exception("Expected achievement to start locked.");
                }

                if (!session.SetAchievement("ACH_TUTORIAL_COMPLETED"))
                {
                    throw new Exception("Expected achievement unlock to succeed.");
                }

                if (!session.GetAchievement("ACH_TUTORIAL_COMPLETED", out achieved) || !achieved)
                {
                    throw new Exception("Expected achievement unlock to persist.");
                }

                if (!session.ClearAchievement("ACH_TUTORIAL_COMPLETED"))
                {
                    throw new Exception("Expected achievement lock to succeed.");
                }

                if (!session.GetAchievement("ACH_TUTORIAL_COMPLETED", out achieved) || achieved)
                {
                    throw new Exception("Expected achievement lock to persist.");
                }

                if (session.GetAchievementDisplayAttribute("ACH_TUTORIAL_COMPLETED", "name") != "n00b")
                {
                    throw new Exception("Unexpected achievement name attribute.");
                }

                if (session.GetAchievementDisplayAttribute("ACH_TUTORIAL_COMPLETED", "desc") != "Tutorial level completed")
                {
                    throw new Exception("Unexpected achievement description attribute.");
                }

                if (session.GetNumAchievements() != 1)
                {
                    throw new Exception("Expected one achievement in the fake library.");
                }

                if (session.GetAchievementName(0) != "ACH_TUTORIAL_COMPLETED")
                {
                    throw new Exception("Unexpected achievement name.");
                }

                if (!session.GetAchievementAchievedPercent("ACH_TUTORIAL_COMPLETED", out var percent) || !Approximately(percent, 28.2f))
                {
                    throw new Exception("Unexpected achievement percentage.");
                }

                if (!session.StoreStats())
                {
                    throw new Exception("Expected StoreStats to succeed.");
                }

                if (!session.ResetAllStats(true))
                {
                    throw new Exception("Expected ResetAllStats to succeed.");
                }

                if (!session.GetStat("hedGamesPlayed", out gamesPlayed) || gamesPlayed != 7)
                {
                    throw new Exception("Expected integer stat reset to default.");
                }

                if (!session.GetStat("hedAccuracy", out accuracy) || !Approximately(accuracy, 19.5f))
                {
                    throw new Exception("Expected float stat reset to default.");
                }

                if (!session.GetStat("hedAverageRate", out averageRate) || !Approximately(averageRate, 2.25f))
                {
                    throw new Exception("Expected average-rate stat reset to default.");
                }

                if (!session.GetAchievement("ACH_TUTORIAL_COMPLETED", out achieved) || achieved)
                {
                    throw new Exception("Expected achievement reset to locked.");
                }
            }

            if (Environment.GetEnvironmentVariable("SteamAppId") != originalSteamAppId)
            {
                throw new Exception("Expected SteamAppId to be restored after dispose.");
            }

            if (Environment.GetEnvironmentVariable("SteamGameId") != originalSteamGameId)
            {
                throw new Exception("Expected SteamGameId to be restored after dispose.");
            }

            AssertFileState(currentSteamAppIdPath, currentSnapshot);
            AssertFileState(baseSteamAppIdPath, baseSnapshot);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            SteamApiNativeTestHost.Reset();

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static FileSnapshot CaptureFileState(string path)
    {
        return new FileSnapshot(File.Exists(path), File.Exists(path) ? File.ReadAllText(path) : null);
    }

    private static void AssertFileState(string path, FileSnapshot snapshot)
    {
        if (snapshot.Exists)
        {
            if (!File.Exists(path))
            {
                throw new Exception($"Expected '{path}' to exist.");
            }

            if (File.ReadAllText(path) != snapshot.Contents)
            {
                throw new Exception($"Expected '{path}' to be restored.");
            }

            return;
        }

        if (File.Exists(path))
        {
            throw new Exception($"Expected '{path}' to be removed.");
        }
    }

    private static bool Approximately(float actual, float expected, float epsilon = 0.01f)
        => Math.Abs(actual - expected) <= epsilon;

    private sealed record FileSnapshot(bool Exists, string? Contents);
}
