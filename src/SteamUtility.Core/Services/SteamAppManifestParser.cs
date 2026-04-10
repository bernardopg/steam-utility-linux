using SteamUtility.Core.Models;
using SteamUtility.Core.Vdf;

namespace SteamUtility.Core.Services;

public sealed class SteamAppManifestParser
{
    public SteamAppManifest? Parse(string manifestPath, string libraryPath)
    {
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        var root = SimpleVdfReader.Parse(File.ReadAllText(manifestPath));
        var appState = root.GetChildren("AppState").FirstOrDefault();
        if (appState is null)
        {
            return null;
        }

        var appIdRaw = appState.GetSingleValue("appid");
        if (!int.TryParse(appIdRaw, out var appId))
        {
            return null;
        }

        var name = appState.GetSingleValue("name") ?? $"App {appId}";
        var installDir = appState.GetSingleValue("installdir") ?? string.Empty;
        var stateFlags = appState.GetSingleValue("StateFlags") ?? string.Empty;

        return new SteamAppManifest(
            AppId: appId,
            Name: name,
            InstallDirectory: installDir,
            ManifestPath: manifestPath,
            LibraryPath: libraryPath,
            StateFlags: stateFlags);
    }
}
