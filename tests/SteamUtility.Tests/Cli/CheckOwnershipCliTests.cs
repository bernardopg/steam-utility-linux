using System.Text.Json;
using SteamUtility.Cli;
using SteamUtility.Core.Models;
using SteamUtility.Core.Services;
using SteamUtility.Tests.Fakes;

namespace SteamUtility.Tests.Cli;

public static class CheckOwnershipCliTests
{
    public static void Run_WithoutOutputPath_PrintsUsage()
    {
        var result = CommandContractTestHarness.Run("check_ownership");

        if (!result.Stdout.Contains("check_ownership <output_path>", StringComparison.Ordinal))
        {
            throw new Exception("Expected check_ownership usage output when output path is missing.");
        }
    }

    public static void Run_WithMissingInstallation_ReturnsFailureJson()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"games-{Guid.NewGuid():N}.json");
        var result = CommandContractTestHarness.Run(
            ["check_ownership", outputPath, "[730]", "--json"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => null
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        var root = payload.RootElement;

        if (root.GetProperty("success").GetBoolean()) throw new Exception("Expected failure payload.");
        if (root.GetProperty("failureReason").GetString() != SteamClientInitializeFailure.InstallPathNotFound.ToString())
        {
            throw new Exception("Expected install-path failure reason.");
        }

        if (File.Exists(outputPath))
        {
            throw new Exception("Missing installation should not create an output file.");
        }
    }

    public static void Run_WithInvalidAppIdPayload_ReturnsFailureJson()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"games-{Guid.NewGuid():N}.json");
        var result = CommandContractTestHarness.Run(
            ["check_ownership", outputPath, "not-json", "--json"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create()
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        var error = payload.RootElement.GetProperty("error").GetString() ?? string.Empty;

        if (!error.Contains("Failed to load app ids", StringComparison.Ordinal))
        {
            throw new Exception("Expected invalid app id payload error.");
        }

        if (File.Exists(outputPath))
        {
            throw new Exception("Invalid app id payload should not create an output file.");
        }
    }

    public static void Run_WithFakeOwnedApps_WritesUpstreamCompatibleGamesJson()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"games-{Guid.NewGuid():N}.json");
        try
        {
            var result = CommandContractTestHarness.Run(
                ["check_ownership", outputPath, "[730,570]", "--json"],
                new SteamUtilityCli.CliRuntimeOverrides
                {
                    ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                    GetOwnedApps = (_, _) =>
                    [
                        new SteamOwnedApp(730, "Counter-Strike 2"),
                        new SteamOwnedApp(570, "Dota 2")
                    ]
                });

            using (var payload = JsonDocument.Parse(result.Stdout))
            {
                var root = payload.RootElement;
                if (!root.GetProperty("success").GetBoolean()) throw new Exception("Expected success payload.");
                if (root.GetProperty("ownedCount").GetInt32() != 2) throw new Exception("Expected ownedCount=2.");
                if (root.GetProperty("totalChecked").GetInt32() != 2) throw new Exception("Expected totalChecked=2.");
            }

            using var gamesPayload = JsonDocument.Parse(File.ReadAllText(outputPath));
            var games = gamesPayload.RootElement.GetProperty("games");
            if (games.GetArrayLength() != 2) throw new Exception("Expected two owned games in games.json.");
            if (games[0].GetProperty("appid").GetUInt32() != 730) throw new Exception("Expected first appid to be 730.");
            if (games[0].GetProperty("name").GetString() != "Counter-Strike 2") throw new Exception("Expected first app name to match.");
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    public static void Run_WithoutAppIdsPayload_UsesDefaultRemoteSource()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"games-{Guid.NewGuid():N}.json");
        try
        {
            var result = CommandContractTestHarness.Run(
                ["check_ownership", outputPath, "--json"],
                new SteamUtilityCli.CliRuntimeOverrides
                {
                    ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                    FetchDefaultOwnershipAppIdsContent = () => "[730,570]",
                    GetOwnedApps = (_, appIds) =>
                    {
                        if (appIds.Count != 2 || appIds[0] != 730 || appIds[1] != 570)
                        {
                            throw new Exception("Expected default remote source app ids to be used.");
                        }

                        return
                        [
                            new SteamOwnedApp(730, "Counter-Strike 2")
                        ];
                    }
                });

            using var payload = JsonDocument.Parse(result.Stdout);
            var root = payload.RootElement;
            if (!root.GetProperty("success").GetBoolean())
            {
                throw new Exception("Expected success payload when default remote app id source succeeds.");
            }

            if (root.GetProperty("totalChecked").GetInt32() != 2)
            {
                throw new Exception("Expected totalChecked=2 when default remote source provides two app ids.");
            }
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    public static void Run_WithoutAppIdsPayload_WhenDefaultRemoteFetchFails_ReturnsFailureJson()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"games-{Guid.NewGuid():N}.json");
        var result = CommandContractTestHarness.Run(
            ["check_ownership", outputPath, "--json"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                FetchDefaultOwnershipAppIdsContent = () => throw new InvalidOperationException("network down")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        var error = payload.RootElement.GetProperty("error").GetString() ?? string.Empty;
        if (!error.Contains("Failed to load app ids", StringComparison.Ordinal) ||
            !error.Contains("network down", StringComparison.Ordinal))
        {
            throw new Exception("Expected default remote fetch failure to be surfaced in the JSON error payload.");
        }

        if (File.Exists(outputPath))
        {
            throw new Exception("Fetch failure should not create an output file.");
        }
    }

    public static void Run_WithoutAppIdsPayload_WhenDefaultRemotePayloadIsInvalid_ReturnsFailureJson()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"games-{Guid.NewGuid():N}.json");
        var result = CommandContractTestHarness.Run(
            ["check_ownership", outputPath, "--json"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                FetchDefaultOwnershipAppIdsContent = () => "not-json"
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        var error = payload.RootElement.GetProperty("error").GetString() ?? string.Empty;
        if (!error.Contains("Failed to load app ids", StringComparison.Ordinal))
        {
            throw new Exception("Expected invalid default remote payload to be surfaced as a load error.");
        }

        if (File.Exists(outputPath))
        {
            throw new Exception("Invalid default remote payload should not create an output file.");
        }
    }

    public static void Run_WhenOwnershipServiceFails_ReturnsFailureReason()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"games-{Guid.NewGuid():N}.json");
        var result = CommandContractTestHarness.Run(
            ["check_ownership", outputPath, "[730]", "--json"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                GetOwnedApps = (_, _) => throw new SteamClientInitializeException(
                    SteamClientInitializeFailure.UserConnectionFailed,
                    "Could not connect to Steam user")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        var root = payload.RootElement;
        if (root.GetProperty("failureReason").GetString() != SteamClientInitializeFailure.UserConnectionFailed.ToString())
        {
            throw new Exception("Expected UserConnectionFailed failure reason.");
        }
    }
}
