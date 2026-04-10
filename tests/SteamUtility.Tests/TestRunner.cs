namespace SteamUtility.Tests;

public static class TestRunner
{
    public static int Main()
    {
        var tests = new Action[]
        {
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
