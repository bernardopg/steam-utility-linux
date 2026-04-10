using SteamUtility.Core.Models;
using SteamUtility.Core.Vdf;

namespace SteamUtility.Core.Services;

public sealed class SteamUserConfigScanner
{
    public IReadOnlyList<SteamUserConfigEntry> Scan(SteamInstallation installation)
    {
        var results = new List<SteamUserConfigEntry>();
        var userdataRoot = Path.Combine(installation.RootPath, "userdata");

        if (!Directory.Exists(userdataRoot))
        {
            return results;
        }

        foreach (var userDirectory in Directory.EnumerateDirectories(userdataRoot))
        {
            if (!ulong.TryParse(Path.GetFileName(userDirectory), out var steamId))
            {
                continue;
            }

            var configDirectory = Path.Combine(userDirectory, "config");
            AddConfig(results, steamId, Path.Combine(configDirectory, "localconfig.vdf"), "local");
            AddConfig(results, steamId, Path.Combine(configDirectory, "sharedconfig.vdf"), "shared");
        }

        return results
            .OrderBy(static entry => entry.SteamId)
            .ThenBy(static entry => entry.ConfigType, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void AddConfig(
        ICollection<SteamUserConfigEntry> results,
        ulong steamId,
        string configPath,
        string configType)
    {
        if (!File.Exists(configPath))
        {
            return;
        }

        var root = SimpleVdfReader.Parse(File.ReadAllText(configPath));
        var appIds = ExtractAppIds(root);
        results.Add(new SteamUserConfigEntry(steamId, configType, configPath, appIds));
    }

    private static IReadOnlyList<int> ExtractAppIds(VdfObject root)
    {
        var appsRoot = root
            .GetChildren("UserLocalConfigStore").FirstOrDefault()?
            .GetChildren("Software").FirstOrDefault()?
            .GetChildren("Valve").FirstOrDefault()?
            .GetChildren("Steam").FirstOrDefault()?
            .GetChildren("apps").FirstOrDefault();

        if (appsRoot is null)
        {
            return [];
        }

        return appsRoot.Children
            .Select(child => int.TryParse(child.Key, out var appId) ? appId : (int?)null)
            .Where(static appId => appId.HasValue)
            .Select(static appId => appId!.Value)
            .ToArray();
    }
}
