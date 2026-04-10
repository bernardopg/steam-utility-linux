using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamLoginUsersParserTests
{
    public static void Parse_LoginUsers_ReturnsMostRecentUserFirst()
    {
        var parser = new SteamLoginUsersParser();
        var content =
            "\"users\"\n{\n  \"11111111111111111\"\n  {\n    \"AccountName\"\t\t\"first\"\n    \"PersonaName\"\t\t\"First User\"\n    \"MostRecent\"\t\t\"0\"\n    \"Timestamp\"\t\t\"100\"\n  }\n  \"22222222222222222\"\n  {\n    \"AccountName\"\t\t\"second\"\n    \"PersonaName\"\t\t\"Second User\"\n    \"MostRecent\"\t\t\"1\"\n    \"Timestamp\"\t\t\"200\"\n  }\n}";

        var result = parser.Parse(content);

        if (result.Count != 2) throw new Exception($"Expected 2 users, got {result.Count}.");
        if (result[0].SteamId != 22222222222222222UL) throw new Exception("Expected most recent user first.");
        if (!result[0].MostRecent) throw new Exception("Expected MostRecent flag to be preserved.");
        if (result[1].SteamId != 11111111111111111UL) throw new Exception("Expected second user second.");
    }
}
