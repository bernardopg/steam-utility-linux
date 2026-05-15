using System.Text.Json;
using SteamUtility.Cli;
using SteamUtility.Core.Models;
using SteamUtility.Core.Services;
using SteamUtility.Tests.Fakes;

namespace SteamUtility.Tests.Cli;

public static class GetAchievementDataCliTests
{
    public static void Run_WithInvalidAppId_ReturnsLegacyJsonError()
    {
        var result = CommandContractTestHarness.Run(
            ["get_achievement_data", "not-a-number"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create()
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("error").GetString() != "Invalid app_id")
        {
            throw new Exception("Expected Invalid app_id error.");
        }
    }

    public static void Run_WithMissingInstallation_ReturnsLegacyJsonError()
    {
        var result = CommandContractTestHarness.Run(
            ["get_achievement_data", "440"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => null
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("error").GetString() != "Steam installation not found.")
        {
            throw new Exception("Expected missing-installation error.");
        }
    }

    public static void Run_WithAchievementItemId_ReturnsAchievementJson()
    {
        var result = CommandContractTestHarness.Run(
            ["get_achievement_data", "440", "/tmp/unused-cache", "ACH_WIN_ONE_GAME"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                LoadAchievementData = (_, _) => BuildSampleResult()
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        var root = payload.RootElement;
        if (root.GetProperty("type").GetString() != "achievement") throw new Exception("Expected achievement payload.");
        if (root.GetProperty("id").GetString() != "ACH_WIN_ONE_GAME") throw new Exception("Expected achievement id.");
        if (root.GetProperty("name").GetString() != "First Victory") throw new Exception("Expected achievement name.");
    }

    public static void Run_WithStatItemId_ReturnsStatJson()
    {
        var result = CommandContractTestHarness.Run(
            ["get_achievement_data", "440", "/tmp/unused-cache", "TOTAL_WINS"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                LoadAchievementData = (_, _) => BuildSampleResult()
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        var root = payload.RootElement;
        if (root.GetProperty("type").GetString() != "stat") throw new Exception("Expected stat payload.");
        if (root.GetProperty("id").GetString() != "TOTAL_WINS") throw new Exception("Expected stat id.");
        if (root.GetProperty("value").GetInt32() != 12) throw new Exception("Expected stat value 12.");
    }

    public static void Run_WithMissingItemId_ReturnsIdNotFoundError()
    {
        var result = CommandContractTestHarness.Run(
            ["get_achievement_data", "440", "/tmp/unused-cache", "MISSING_ID"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                LoadAchievementData = (_, _) => BuildSampleResult()
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("error").GetString() != "ID not found")
        {
            throw new Exception("Expected ID not found error.");
        }
    }

    public static void Run_WithoutItemId_WritesAggregateCacheFileAndReturnsSuccessPath()
    {
        var cacheDir = Path.Combine(Path.GetTempPath(), $"steam-achievements-{Guid.NewGuid():N}");
        try
        {
            var result = CommandContractTestHarness.Run(
                ["get_achievement_data", "440", cacheDir],
                new SteamUtilityCli.CliRuntimeOverrides
                {
                    ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                    LoadAchievementData = (_, _) => BuildSampleResult()
                });

            using var payload = JsonDocument.Parse(result.Stdout);
            var successPath = payload.RootElement.GetProperty("success").GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(successPath)) throw new Exception("Expected success path.");
            if (!File.Exists(successPath)) throw new Exception("Expected aggregate cache file to be written.");

            using var filePayload = JsonDocument.Parse(File.ReadAllText(successPath));
            if (filePayload.RootElement.GetProperty("achievements").GetArrayLength() != 1)
            {
                throw new Exception("Expected one achievement in aggregate payload.");
            }

            if (filePayload.RootElement.GetProperty("stats").GetArrayLength() != 1)
            {
                throw new Exception("Expected one stat in aggregate payload.");
            }
        }
        finally
        {
            if (Directory.Exists(cacheDir))
            {
                Directory.Delete(cacheDir, recursive: true);
            }
        }
    }

    public static void Run_WhenSteamworksInitFails_ReturnsFailureReason()
    {
        var result = CommandContractTestHarness.Run(
            ["get_achievement_data", "440"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                LoadAchievementData = (_, _) => throw new SteamworksInitializationException(
                    SteamworksInitializationFailure.ApiInitFailed,
                    "Failed to initialize Steam API. Make sure Steam is running and the selected app id is valid.")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("failureReason").GetString() != SteamworksInitializationFailure.ApiInitFailed.ToString())
        {
            throw new Exception("Expected ApiInitFailed failure reason.");
        }
    }

    private static SteamUtilityCli.AchievementDataCommandResult BuildSampleResult()
    {
        return new SteamUtilityCli.AchievementDataCommandResult(
            12345678901234567,
            [
                new AchievementData
                {
                    Id = "ACH_WIN_ONE_GAME",
                    Name = "First Victory",
                    Description = "Win your first match",
                    IconNormal = "icon.png",
                    IconLocked = "icon-locked.png",
                    IsHidden = false,
                    Permission = 0,
                    Achieved = true,
                    Percent = 42.5f
                }
            ],
            [
                new StatData
                {
                    Id = "TOTAL_WINS",
                    Name = "Total Wins",
                    Type = "integer",
                    Permission = 0,
                    MinValue = 0,
                    MaxValue = 1000,
                    DefaultValue = 0,
                    Value = 12,
                    IncrementOnly = false
                }
            ]);
    }
}
