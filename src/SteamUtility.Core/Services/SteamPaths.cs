namespace SteamUtility.Core.Services;

internal static class SteamPaths
{
    public static string ResolveSteamAppsPath(string libraryPath)
    {
        var candidate = Path.Combine(libraryPath, "steamapps");
        return Directory.Exists(candidate) ? candidate : libraryPath;
    }
}
