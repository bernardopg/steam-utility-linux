using SteamUtility.Core.Models;
using SteamUtility.Core.Services;
using SteamUtility.Tests.Fakes;

namespace SteamUtility.Tests.Native;

public static class SteamApiNativeTests
{
    public static void EnsureLoaded_ThrowsWhenLibraryMissing()
    {
        SteamApiNativeTestHost.Reset();

        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var installation = FakeSteamInstallationFactory.Create(tempRoot, Path.Combine(tempRoot, "steamapps"));
        var resolver = FakeSteamApiLibrary.CreateResolver(Path.Combine(tempRoot, "missing", "libsteam_api.so"));

        try
        {
            try
            {
                SteamApiNative.EnsureLoaded(installation, resolver);
                throw new Exception("Expected Steamworks initialization to fail.");
            }
            catch (SteamworksInitializationException ex)
            {
                if (ex.FailureReason != SteamworksInitializationFailure.LibraryNotFound)
                {
                    throw new Exception($"Expected LibraryNotFound, got '{ex.FailureReason}'.");
                }
            }
        }
        finally
        {
            SteamApiNativeTestHost.Reset();
        }
    }

    public static void EnsureLoaded_ThrowsWhenExportMissing()
    {
        SteamApiNativeTestHost.Reset();

        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var installation = FakeSteamInstallationFactory.Create(tempRoot, Path.Combine(tempRoot, "steamapps"));
        var resolver = FakeSteamApiLibrary.CreateResolver(FakeSteamApiLibrary.MissingExportLibraryPath);

        try
        {
            try
            {
                SteamApiNative.EnsureLoaded(installation, resolver);
                throw new Exception("Expected Steamworks initialization to fail.");
            }
            catch (SteamworksInitializationException ex)
            {
                if (ex.FailureReason != SteamworksInitializationFailure.ExportNotFound)
                {
                    throw new Exception($"Expected ExportNotFound, got '{ex.FailureReason}'.");
                }
            }
        }
        finally
        {
            SteamApiNativeTestHost.Reset();
        }
    }

    public static void EnsureLoaded_AndNativeCallsWork()
    {
        SteamApiNativeTestHost.Reset();

        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var installation = FakeSteamInstallationFactory.Create(tempRoot, Path.Combine(tempRoot, "steamapps"));
        var resolver = FakeSteamApiLibrary.CreateResolver(FakeSteamApiLibrary.FullLibraryPath);
        var initialized = false;

        try
        {
            SteamApiNative.EnsureLoaded(installation, resolver);
            initialized = SteamApiNative.Init();
            if (!initialized)
            {
                throw new Exception("Expected SteamAPI_Init to succeed.");
            }

            var steamUser = SteamApiNative.GetSteamUser();
            var steamUserStats = SteamApiNative.GetSteamUserStats();
            if (steamUser == IntPtr.Zero) throw new Exception("Expected Steam user interface.");
            if (steamUserStats == IntPtr.Zero) throw new Exception("Expected Steam user stats interface.");

            if (SteamApiNative.GetSteamId(steamUser) != 12345678901234567UL)
            {
                throw new Exception("Unexpected Steam ID.");
            }

            if (!SteamApiNative.RequestCurrentStats(steamUserStats))
            {
                throw new Exception("Expected current stats request to succeed.");
            }

            if (SteamApiNative.RequestGlobalAchievementPercentages(steamUserStats) == 0)
            {
                throw new Exception("Expected global achievement percentage request to succeed.");
            }

            if (SteamApiNative.GetNumAchievements(steamUserStats) != 1)
            {
                throw new Exception("Expected one achievement in the fake library.");
            }

            if (SteamApiNative.GetAchievementName(steamUserStats, 0) != "ACH_TUTORIAL_COMPLETED")
            {
                throw new Exception("Unexpected achievement name.");
            }

            if (SteamApiNative.GetAchievementDisplayAttribute(steamUserStats, "ACH_TUTORIAL_COMPLETED", "name") != "n00b")
            {
                throw new Exception("Unexpected achievement display name.");
            }

            if (SteamApiNative.GetAchievementDisplayAttribute(steamUserStats, "ACH_TUTORIAL_COMPLETED", "desc") != "Tutorial level completed")
            {
                throw new Exception("Unexpected achievement display description.");
            }

            if (!SteamApiNative.GetStat(steamUserStats, "hedGamesPlayed", out int gamesPlayed) || gamesPlayed != 7)
            {
                throw new Exception("Unexpected integer stat value.");
            }

            if (!SteamApiNative.GetStat(steamUserStats, "hedAccuracy", out float accuracy) || !Approximately(accuracy, 19.5f))
            {
                throw new Exception("Unexpected float stat value.");
            }

            if (!SteamApiNative.GetStat(steamUserStats, "hedAverageRate", out float averageRate) || !Approximately(averageRate, 2.25f))
            {
                throw new Exception("Unexpected average rate stat value.");
            }

            if (!SteamApiNative.SetStat(steamUserStats, "hedGamesPlayed", 12))
            {
                throw new Exception("Expected integer stat mutation to succeed.");
            }

            if (!SteamApiNative.SetStat(steamUserStats, "hedAccuracy", 42.5f))
            {
                throw new Exception("Expected float stat mutation to succeed.");
            }

            if (!SteamApiNative.SetStat(steamUserStats, "hedAverageRate", 3.5f))
            {
                throw new Exception("Expected average rate stat mutation to succeed.");
            }

            if (!SteamApiNative.GetStat(steamUserStats, "hedGamesPlayed", out gamesPlayed) || gamesPlayed != 12)
            {
                throw new Exception("Expected integer stat mutation to persist.");
            }

            if (!SteamApiNative.GetStat(steamUserStats, "hedAccuracy", out accuracy) || !Approximately(accuracy, 42.5f))
            {
                throw new Exception("Expected float stat mutation to persist.");
            }

            if (!SteamApiNative.GetStat(steamUserStats, "hedAverageRate", out averageRate) || !Approximately(averageRate, 3.5f))
            {
                throw new Exception("Expected average rate mutation to persist.");
            }

            if (!SteamApiNative.GetAchievement(steamUserStats, "ACH_TUTORIAL_COMPLETED", out var achieved) || achieved)
            {
                throw new Exception("Expected achievement to start locked.");
            }

            if (!SteamApiNative.SetAchievement(steamUserStats, "ACH_TUTORIAL_COMPLETED"))
            {
                throw new Exception("Expected achievement unlock to succeed.");
            }

            if (!SteamApiNative.GetAchievement(steamUserStats, "ACH_TUTORIAL_COMPLETED", out achieved) || !achieved)
            {
                throw new Exception("Expected achievement unlock to persist.");
            }

            if (!SteamApiNative.ClearAchievement(steamUserStats, "ACH_TUTORIAL_COMPLETED"))
            {
                throw new Exception("Expected achievement lock to succeed.");
            }

            if (!SteamApiNative.GetAchievement(steamUserStats, "ACH_TUTORIAL_COMPLETED", out achieved) || achieved)
            {
                throw new Exception("Expected achievement lock to persist.");
            }

            if (!SteamApiNative.ResetAllStats(steamUserStats, true))
            {
                throw new Exception("Expected reset all stats to succeed.");
            }

            if (!SteamApiNative.GetStat(steamUserStats, "hedGamesPlayed", out gamesPlayed) || gamesPlayed != 7)
            {
                throw new Exception("Expected integer stat reset to default.");
            }

            if (!SteamApiNative.GetStat(steamUserStats, "hedAccuracy", out accuracy) || !Approximately(accuracy, 19.5f))
            {
                throw new Exception("Expected float stat reset to default.");
            }

            if (!SteamApiNative.GetStat(steamUserStats, "hedAverageRate", out averageRate) || !Approximately(averageRate, 2.25f))
            {
                throw new Exception("Expected average rate stat reset to default.");
            }

            if (!SteamApiNative.GetAchievement(steamUserStats, "ACH_TUTORIAL_COMPLETED", out achieved) || achieved)
            {
                throw new Exception("Expected achievement reset to locked.");
            }

            if (!SteamApiNative.GetAchievementAchievedPercent(steamUserStats, "ACH_TUTORIAL_COMPLETED", out var percent) || !Approximately(percent, 28.2f))
            {
                throw new Exception("Unexpected achievement percentage.");
            }

            SteamApiNative.Shutdown();
            initialized = false;
        }
        finally
        {
            if (initialized)
            {
                SteamApiNative.Shutdown();
            }

            SteamApiNativeTestHost.Reset();
        }
    }

    private static bool Approximately(float actual, float expected, float epsilon = 0.01f)
        => Math.Abs(actual - expected) <= epsilon;
}
