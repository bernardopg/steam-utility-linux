using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamConfigCompatibilityParserTests
{
    public static void Parse_CompatToolMapping_ReturnsAssignments()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, "\"InstallConfigStore\"\n{\n  \"Software\"\n  {\n    \"Valve\"\n    {\n      \"Steam\"\n      {\n        \"CompatToolMapping\"\n        {\n          \"570\"\n          {\n            \"name\"\t\t\"proton_experimental\"\n            \"Priority\"\t\t\"250\"\n          }\n        }\n      }\n    }\n  }\n}");

        try
        {
            var parser = new SteamConfigCompatibilityParser();
            var result = parser.Parse(path);

            if (result.Count != 1) throw new Exception($"Expected 1 mapping, got {result.Count}.");
            if (result[0].AppId != 570) throw new Exception("Unexpected AppId.");
            if (result[0].ToolName != "proton_experimental") throw new Exception("Unexpected tool name.");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
