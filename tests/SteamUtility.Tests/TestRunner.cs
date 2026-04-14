namespace SteamUtility.Tests;

public static class TestRunner
{
    public static int Main()
    {
        var tests = new Action[]
        {
            Cli.CliHelpAndDispatchTests.ProgramEntrypoint_ForwardsToSteamUtilityCliRun,
            Cli.CliHelpAndDispatchTests.Run_WithoutArgs_PrintsUsage,
            Cli.CliHelpAndDispatchTests.Run_WithHelpFlag_PrintsUsage,
            Cli.CliHelpAndDispatchTests.Run_WithShortHelpFlag_PrintsUsage,
            Cli.CliHelpAndDispatchTests.Run_WithUnknownCommand_PrintsUsage,
            Cli.CliHelpAndDispatchTests.Run_WithCheckOwnershipAlias_PrintsSameUsage,
            Cli.CheckOwnershipCliTests.Run_WithoutOutputPath_PrintsUsage,
            Cli.CheckOwnershipCliTests.Run_WithMissingInstallation_ReturnsFailureJson,
            Cli.CheckOwnershipCliTests.Run_WithInvalidAppIdPayload_ReturnsFailureJson,
            Cli.CheckOwnershipCliTests.Run_WithFakeOwnedApps_WritesUpstreamCompatibleGamesJson,
            Cli.CheckOwnershipCliTests.Run_WhenOwnershipServiceFails_ReturnsFailureReason,
            Cli.IdleCliTests.Run_WithInvalidAppId_ReturnsLegacyJsonError,
            Cli.IdleCliTests.Run_WithMissingInstallation_ReturnsLegacyJsonError,
            Cli.IdleCliTests.Run_WithIdleOverride_ReturnsSuccessPayload,
            Cli.IdleCliTests.Run_WithOptionalAppName_PreservesNameInPayload,
            Cli.IdleCliTests.Run_WhenSteamworksInitFails_ReturnsFailureReason,
            Cli.GetAchievementDataCliTests.Run_WithInvalidAppId_ReturnsLegacyJsonError,
            Cli.GetAchievementDataCliTests.Run_WithMissingInstallation_ReturnsLegacyJsonError,
            Cli.GetAchievementDataCliTests.Run_WithAchievementItemId_ReturnsAchievementJson,
            Cli.GetAchievementDataCliTests.Run_WithStatItemId_ReturnsStatJson,
            Cli.GetAchievementDataCliTests.Run_WithMissingItemId_ReturnsIdNotFoundError,
            Cli.GetAchievementDataCliTests.Run_WithoutItemId_WritesAggregateCacheFileAndReturnsSuccessPath,
            Cli.GetAchievementDataCliTests.Run_WhenSteamworksInitFails_ReturnsFailureReason,
            Cli.AchievementMutationCliTests.Run_SingleMutation_WithMissingAchievementId_ReturnsRequiredError,
            Cli.AchievementMutationCliTests.Run_UnlockAchievement_Success_ReturnsSuccessMessage,
            Cli.AchievementMutationCliTests.Run_LockAchievement_Success_ReturnsSuccessMessage,
            Cli.AchievementMutationCliTests.Run_SingleMutation_WithMissingAchievement_ReturnsFailureMessage,
            Cli.AchievementMutationCliTests.Run_SingleMutation_WhenSteamworksInitFails_ReturnsFailureReason,
            Cli.AchievementMutationCliTests.Run_ToggleAchievement_UnlockPath_ReturnsUnlockSuccessMessage,
            Cli.AchievementMutationCliTests.Run_ToggleAchievement_LockPath_ReturnsLockSuccessMessage,
            Cli.AchievementMutationCliTests.Run_ToggleAchievement_ValidationFailure_ReturnsError,
            Cli.AchievementMutationCliTests.Run_UnlockAllAchievements_Success_ReturnsSuccessMessage,
            Cli.AchievementMutationCliTests.Run_UnlockAllAchievements_PartialFailure_ReturnsError,
            Cli.AchievementMutationCliTests.Run_LockAllAchievements_Success_ReturnsSuccessMessage,
            Cli.AchievementMutationCliTests.Run_LockAllAchievements_PostResetValidationFailure_ReturnsError,
            Cli.StatsMutationCliTests.Run_UpdateStats_WithMissingJsonArray_ReturnsRequiredError,
            Cli.StatsMutationCliTests.Run_UpdateStats_WithInvalidJson_ReturnsFormatError,
            Cli.StatsMutationCliTests.Run_UpdateStats_Success_ReturnsSuccessMessage,
            Cli.StatsMutationCliTests.Run_UpdateStats_PartialFailure_ReturnsError,
            Cli.StatsMutationCliTests.Run_UpdateStats_StoreFailure_ReturnsError,
            Cli.StatsMutationCliTests.Run_UpdateStats_ValidationFailure_ReturnsError,
            Cli.StatsMutationCliTests.Run_UpdateStats_WhenSteamworksInitFails_ReturnsFailureReason,
            Cli.StatsMutationCliTests.Run_ResetAllStats_Success_ReturnsSuccessMessage,
            Cli.StatsMutationCliTests.Run_ResetAllStats_ResetFailure_ReturnsError,
            Cli.StatsMutationCliTests.Run_ResetAllStats_ValidationFailure_ReturnsError,
            Cli.StatsMutationCliTests.Run_ResetAllStats_WhenSteamworksInitFails_ReturnsFailureReason,
            SteamAppManifestParserTests.Parse_AppManifest_ReturnsExpectedValues,
            SteamLibraryFoldersParserTests.Parse_MultipleLibraryFolders_ReturnsExpectedEntries,
            SteamLibraryScannerTests.ScanInstalledApps_FindsManifestsAcrossLibraries,
            SteamCompatDataScannerTests.Scan_FindsCompatDataAndPfx,
            SteamCompatibilityToolScannerTests.Scan_FindsCustomAndBundledCompatibilityTools,
            SteamConfigCompatibilityParserTests.Parse_CompatToolMapping_ReturnsAssignments,
            SteamLoginUsersParserTests.Parse_LoginUsers_ReturnsMostRecentUserFirst,
            SteamUserConfigScannerTests.Scan_FindsLocalAndSharedConfigs,
            SteamCompatibilityReportServiceTests.Build_WithMissingFiles_ReturnsEmptyReport,
            SteamCompatibilityReportServiceTests.Build_MergesAppCompatDataAndMappings,
            SteamStateReportServiceTests.Build_SummarizesSteamStateFiles,
            SteamPlatformRuntimeTests.Current_SelectsPlatformSpecificServices,
            LinuxSteamClientLibraryTests.FindLibraryPath_Prefers64BitClient_WhenMultipleCandidatesExist,
            LinuxSteamClientLibraryTests.FindLibraryPath_ReturnsNull_WhenClientLibraryDoesNotExist,
            LinuxSteamApiLibraryResolverTests.FindLibraryPath_PrefersLocalLibraryFromSteamGameFolders,
            LinuxSteamApiLibraryResolverTests.FindLibraryPath_UsesEnvironmentOverrideWhenPresent
        };

        foreach (var test in tests)
        {
            try
            {
                test();
                Console.WriteLine($"PASS {test.Method.DeclaringType?.Name}.{test.Method.Name}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"FAIL {test.Method.DeclaringType?.Name}.{test.Method.Name}: {ex.Message}");
                return 1;
            }
        }

        return 0;
    }
}
