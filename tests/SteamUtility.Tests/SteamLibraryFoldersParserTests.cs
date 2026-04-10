using SteamUtility.Core.Services;

namespace SteamUtility.Tests;

public static class SteamLibraryFoldersParserTests
{
    public static void Parse_MultipleLibraryFolders_ReturnsExpectedEntries()
    {
        var parser = new SteamLibraryFoldersParser();
        var content = "\"libraryfolders\"\n{\n  \"0\"\n  {\n    \"path\"\t\t\"/home/test/.local/share/Steam\"\n    \"label\"\t\t\"\"\n    \"apps\"\n    {\n      \"570\"\t\t\"123\"\n    }\n  }\n  \"1\"\n  {\n    \"path\"\t\t\"/mnt/games/SteamLibrary\"\n    \"apps\"\n    {\n      \"730\"\t\t\"456\"\n    }\n  }\n}";

        var result = parser.Parse(content);

        if (result.Count != 2) throw new Exception($"Expected 2 folders, got {result.Count}.");
        if (result[0].Path != "/home/test/.local/share/Steam") throw new Exception("Unexpected first path.");
        if (!result[0].IsDefault) throw new Exception("Expected first folder to be default.");
        if (result[1].Path != "/mnt/games/SteamLibrary") throw new Exception("Unexpected second path.");
        if (result[1].AppIds is null || !result[1].AppIds.Contains(730)) throw new Exception("Expected second folder to contain app 730.");
    }
}
