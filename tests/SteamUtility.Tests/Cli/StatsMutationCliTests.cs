using System.Text.Json;
using SteamUtility.Cli;
using SteamUtility.Core.Services;
using SteamUtility.Tests.Fakes;

namespace SteamUtility.Tests.Cli;

public static class StatsMutationCliTests
{
    public static void Run_UpdateStats_WithMissingJsonArray_ReturnsRequiredError()
    {
        var result = CommandContractTestHarness.Run(
            ["update_stats", "440"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create()
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("error").GetString() != "stats JSON array is required")
        {
            throw new Exception("Expected missing stats JSON error.");
        }
    }

    public static void Run_UpdateStats_WithInvalidJson_ReturnsFormatError()
    {
        var result = CommandContractTestHarness.Run(
            ["update_stats", "440", "not-json"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create()
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        var error = payload.RootElement.GetProperty("error").GetString() ?? string.Empty;
        if (!error.Contains("Invalid stats format", StringComparison.Ordinal))
        {
            throw new Exception("Expected invalid stats format error.");
        }
    }

    public static void Run_UpdateStats_Success_ReturnsSuccessMessage()
    {
        var result = CommandContractTestHarness.Run(
            ["update_stats", "440", "[{\"name\":\"TOTAL_WINS\",\"value\":12}]"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunUpdateStats = (_, _, _) => new SteamUtilityCli.StatsMutationCommandResult(
                    true,
                    SuccessMessage: "Successfully updated all stats")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("success").GetString() != "Successfully updated all stats")
        {
            throw new Exception("Expected update_stats success message.");
        }
    }

    public static void Run_UpdateStats_PartialFailure_ReturnsError()
    {
        var result = CommandContractTestHarness.Run(
            ["update_stats", "440", "[{\"name\":\"TOTAL_WINS\",\"value\":12}]"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunUpdateStats = (_, _, _) => new SteamUtilityCli.StatsMutationCommandResult(
                    false,
                    Error: "One or more stats failed to update")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("error").GetString() != "One or more stats failed to update")
        {
            throw new Exception("Expected partial failure error.");
        }
    }

    public static void Run_UpdateStats_StoreFailure_ReturnsError()
    {
        var result = CommandContractTestHarness.Run(
            ["update_stats", "440", "[{\"name\":\"TOTAL_WINS\",\"value\":12}]"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunUpdateStats = (_, _, _) => new SteamUtilityCli.StatsMutationCommandResult(
                    false,
                    Error: "Failed to store updated stats")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("error").GetString() != "Failed to store updated stats")
        {
            throw new Exception("Expected store failure error.");
        }
    }

    public static void Run_UpdateStats_ValidationFailure_ReturnsError()
    {
        var result = CommandContractTestHarness.Run(
            ["update_stats", "440", "[{\"name\":\"TOTAL_WINS\",\"value\":12}]"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunUpdateStats = (_, _, _) => new SteamUtilityCli.StatsMutationCommandResult(
                    false,
                    Error: "Stat validation failed after storing changes")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("error").GetString() != "Stat validation failed after storing changes")
        {
            throw new Exception("Expected stat validation failure error.");
        }
    }

    public static void Run_UpdateStats_WhenSteamworksInitFails_ReturnsFailureReason()
    {
        var result = CommandContractTestHarness.Run(
            ["update_stats", "440", "[{\"name\":\"TOTAL_WINS\",\"value\":12}]"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunUpdateStats = (_, _, _) => throw new SteamworksInitializationException(
                    SteamworksInitializationFailure.ApiInitFailed,
                    "Failed to initialize Steam API. Make sure Steam is running and the selected app id is valid.")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("failureReason").GetString() != SteamworksInitializationFailure.ApiInitFailed.ToString())
        {
            throw new Exception("Expected ApiInitFailed failure reason.");
        }
    }

    public static void Run_ResetAllStats_Success_ReturnsSuccessMessage()
    {
        var result = CommandContractTestHarness.Run(
            ["reset_all_stats", "440"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunResetAllStats = (_, _) => new SteamUtilityCli.StatsMutationCommandResult(
                    true,
                    SuccessMessage: "Successfully reset all stats")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("success").GetString() != "Successfully reset all stats")
        {
            throw new Exception("Expected reset_all_stats success message.");
        }
    }

    public static void Run_ResetAllStats_ResetFailure_ReturnsError()
    {
        var result = CommandContractTestHarness.Run(
            ["reset_all_stats", "440"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunResetAllStats = (_, _) => new SteamUtilityCli.StatsMutationCommandResult(
                    false,
                    Error: "Failed to reset all stats")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("error").GetString() != "Failed to reset all stats")
        {
            throw new Exception("Expected reset failure error.");
        }
    }

    public static void Run_ResetAllStats_ValidationFailure_ReturnsError()
    {
        var result = CommandContractTestHarness.Run(
            ["reset_all_stats", "440"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunResetAllStats = (_, _) => new SteamUtilityCli.StatsMutationCommandResult(
                    false,
                    Error: "Stat reset validation failed after storing changes")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("error").GetString() != "Stat reset validation failed after storing changes")
        {
            throw new Exception("Expected stat reset validation failure error.");
        }
    }

    public static void Run_ResetAllStats_WhenSteamworksInitFails_ReturnsFailureReason()
    {
        var result = CommandContractTestHarness.Run(
            ["reset_all_stats", "440"],
            new SteamUtilityCli.CliRuntimeOverrides
            {
                ResolveInstallation = () => FakeSteamInstallationFactory.Create(),
                RunResetAllStats = (_, _) => throw new SteamworksInitializationException(
                    SteamworksInitializationFailure.ApiInitFailed,
                    "Failed to initialize Steam API. Make sure Steam is running and the selected app id is valid.")
            });

        using var payload = JsonDocument.Parse(result.Stdout);
        if (payload.RootElement.GetProperty("failureReason").GetString() != SteamworksInitializationFailure.ApiInitFailed.ToString())
        {
            throw new Exception("Expected ApiInitFailed failure reason.");
        }
    }
}
