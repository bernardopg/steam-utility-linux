namespace SteamUtility.Tests;

public static class TestRunner
{
    public static int Main()
    {
        var tests = new Action[]
        {
            SteamLibraryFoldersParserTests.Parse_MultipleLibraryFolders_ReturnsExpectedEntries,
            SteamConfigCompatibilityParserTests.Parse_CompatToolMapping_ReturnsAssignments,
            SteamCompatibilityReportServiceTests.Build_WithMissingFiles_ReturnsEmptyReport,
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
