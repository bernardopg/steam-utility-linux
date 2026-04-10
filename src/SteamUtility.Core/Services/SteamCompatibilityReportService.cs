using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamCompatibilityReportService
{
    private readonly SteamLibraryScanner _libraryScanner = new();
    private readonly SteamCompatDataScanner _compatDataScanner = new();
    private readonly SteamConfigCompatibilityParser _configParser = new();
    private readonly SteamCompatibilityToolScanner _toolScanner = new();

    public IReadOnlyList<SteamCompatibilityReportEntry> Build(SteamInstallation installation)
    {
        var apps = _libraryScanner.ScanInstalledApps(installation);
        var compatData = _compatDataScanner.Scan(installation).ToDictionary(static item => item.AppId);
        var tools = _toolScanner.Scan(installation);
        var configPath = Path.Combine(installation.RootPath, "config", "config.vdf");
        var assignments = _configParser.Parse(configPath).ToDictionary(static item => item.AppId);

        var appIds = new SortedSet<int>(apps.Select(static app => app.AppId));
        appIds.UnionWith(compatData.Keys);
        appIds.UnionWith(assignments.Keys);

        var appLookup = apps.ToDictionary(static app => app.AppId);
        var results = new List<SteamCompatibilityReportEntry>();

        foreach (var appId in appIds)
        {
            appLookup.TryGetValue(appId, out var app);
            compatData.TryGetValue(appId, out var compat);
            assignments.TryGetValue(appId, out var assignment);
            var resolvedTool = assignment is null
                ? null
                : ResolveToolAssignment(assignment.ToolName, tools);

            results.Add(new SteamCompatibilityReportEntry(
                AppId: appId,
                Name: app?.Name ?? $"App {appId}",
                InstallDirectory: app?.InstallDirectory,
                HasCompatData: compat is not null,
                CompatDataPath: compat?.CompatDataPath,
                CompatPfxPath: compat?.PfxPath,
                LibraryPath: compat?.LibraryPath,
                AssignedTool: assignment?.ToolName,
                AssignedToolPriority: assignment?.ToolPriority,
                AssignedToolConfig: assignment?.ToolConfig,
                ResolvedToolPath: resolvedTool?.RootPath,
                ResolvedToolIsCustom: resolvedTool?.IsCustom ?? false));
        }

        return results;
    }

    private static SteamCompatibilityTool? ResolveToolAssignment(
        string toolName,
        IReadOnlyList<SteamCompatibilityTool> tools)
    {
        if (string.IsNullOrWhiteSpace(toolName) || tools.Count == 0)
        {
            return null;
        }

        var normalizedTarget = NormalizeToolName(toolName);
        if (string.IsNullOrWhiteSpace(normalizedTarget))
        {
            return null;
        }

        var candidates = tools
            .Select(tool => new
            {
                Tool = tool,
                Score = ScoreToolMatch(normalizedTarget, NormalizeToolName(tool.Name))
            })
            .Where(candidate => candidate.Score is not null)
            .OrderBy(candidate => candidate.Score!.Value)
            .ThenByDescending(candidate => candidate.Tool.IsCustom)
            .ThenBy(candidate => candidate.Tool.Name.Length)
            .ToArray();

        return candidates.FirstOrDefault()?.Tool;
    }

    private static int? ScoreToolMatch(string normalizedTarget, string normalizedCandidate)
    {
        if (string.IsNullOrWhiteSpace(normalizedCandidate))
        {
            return null;
        }

        if (normalizedTarget == normalizedCandidate)
        {
            return 0;
        }

        if (normalizedTarget.Contains(normalizedCandidate, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.Contains(normalizedTarget, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return null;
    }

    private static string NormalizeToolName(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);

        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        return builder.ToString();
    }
}
