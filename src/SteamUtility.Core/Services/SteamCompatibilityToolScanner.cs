using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamCompatibilityToolScanner
{
    public IReadOnlyList<SteamCompatibilityTool> Scan(SteamInstallation installation)
    {
        var results = new List<SteamCompatibilityTool>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddToolsFromDirectory(Path.Combine(installation.RootPath, "compatibilitytools.d"), isCustom: true, results, seenPaths);
        AddToolsFromDirectory(Path.Combine(installation.RootPath, "steamapps", "common"), isCustom: false, results, seenPaths);

        return results
            .OrderBy(static tool => tool.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void AddToolsFromDirectory(
        string directoryPath,
        bool isCustom,
        ICollection<SteamCompatibilityTool> results,
        ISet<string> seenPaths)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        foreach (var entry in Directory.EnumerateDirectories(directoryPath))
        {
            var name = Path.GetFileName(entry);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (!LooksLikeCompatibilityTool(name, isCustom))
            {
                continue;
            }

            if (!seenPaths.Add(entry))
            {
                continue;
            }

            results.Add(new SteamCompatibilityTool(
                Name: name,
                RootPath: entry,
                IsCustom: isCustom));
        }
    }

    private static bool LooksLikeCompatibilityTool(string name, bool isCustom)
    {
        if (isCustom)
        {
            return true;
        }

        return name.Contains("Proton", StringComparison.OrdinalIgnoreCase)
            || name.Contains("SteamLinuxRuntime", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Steam Linux Runtime", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Soldier", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Sniper", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Scout", StringComparison.OrdinalIgnoreCase);
    }
}
