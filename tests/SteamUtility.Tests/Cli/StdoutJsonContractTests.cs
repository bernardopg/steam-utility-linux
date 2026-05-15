using System.Text.Json;
using SteamUtility.Cli;
using SteamUtility.Core.Models;
using SteamUtility.Tests.Fakes;

namespace SteamUtility.Tests.Cli;

public static class StdoutJsonContractTests
{
    public static void CheckOwnership_JsonMode_EmitsOnlyJsonOnStdout()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"games-{Guid.NewGuid():N}.json");
        try
        {
            var result = CommandContractTestHarness.Run(
                ["check_ownership", outputPath, "[730]", "--json"],
                new SteamUtilityCli.CliRuntimeOverrides
                {
                    ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                    GetOwnedApps = (_, _) => [new SteamOwnedApp(730, "Counter-Strike 2")]
                });

            AssertSingleJsonStdout(result);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    public static void Idle_SingleGame_EmitsOnlyJsonOnStdout()
    {
        var result = CommandContractTestHarness.Run(
            ["idle", "730", "Counter-Strike 2"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunIdle = (_, _, _) => { }
            });

        AssertSingleJsonStdout(result);
    }

    public static void GetAchievementData_EmitsOnlyJsonOnStdout()
    {
        var cacheDir = Path.Combine(Path.GetTempPath(), $"steam-achievements-{Guid.NewGuid():N}");
        try
        {
            var result = CommandContractTestHarness.Run(
                ["get_achievement_data", "730", cacheDir],
                new SteamUtilityCli.CliRuntimeOverrides
                {
                    ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                    LoadAchievementData = (_, _) => BuildSampleResult()
                });

            AssertSingleJsonStdout(result);
        }
        finally
        {
            if (Directory.Exists(cacheDir))
            {
                Directory.Delete(cacheDir, recursive: true);
            }
        }
    }

    public static void MutationCommands_EmitOnlyJsonOnStdout()
    {
        var unlock = CommandContractTestHarness.Run(
            ["unlock_achievement", "730", "ACH_WIN_ONE_GAME"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunSingleAchievementMutation = (_, _, _, _) => new SteamUtilityCli.AchievementMutationCommandResult(true)
            });
        AssertSingleJsonStdout(unlock);

        var stats = CommandContractTestHarness.Run(
            ["update_stats", "730", """[{"id":"TOTAL_WINS","value":1}]"""],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunUpdateStats = (_, _, _) => new SteamUtilityCli.StatsMutationCommandResult(true)
            });
        AssertSingleJsonStdout(stats);
    }

    private static void AssertSingleJsonStdout(CliRunResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.Stderr))
        {
            throw new Exception($"Expected stderr to be empty for SGI JSON command. stderr={result.Stderr}");
        }

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new Exception("Expected SGI command stdout to contain one JSON object.");
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
