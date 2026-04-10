using System.Text.Json;
using SteamUtility.Core.Abstractions;
using SteamUtility.Core.Models;
using SteamUtility.Core.Services;

ISteamLocator locator = new LinuxSteamLocator();
var installationService = new SteamInstallationService(locator);
var installation = installationService.TryResolve();
var options = CliOptions.Parse(args);

if (string.IsNullOrWhiteSpace(options.Command))
{
    PrintUsage();
    return;
}

switch (options.Command)
{
    case "detect":
        PrintDetect(installation, options);
        return;

    case "libraries":
        PrintLibraries(installation, options);
        return;

    case "apps":
        PrintApps(installation, options);
        return;

    case "compatdata":
        PrintCompatData(installation, options);
        return;

    case "compat-tools":
        PrintCompatibilityTools(installation, options);
        return;

    case "compat-mapping":
        PrintCompatibilityMappings(installation, options);
        return;

    case "compat-report":
        PrintCompatibilityReport(installation, options);
        return;

    default:
        PrintUsage();
        return;
}

static void PrintDetect(SteamInstallation? installation, CliOptions options)
{
    if (options.Json)
    {
        WriteJson(new
        {
            found = installation is not null,
            rootPath = installation?.RootPath
        });
        return;
    }

    Console.WriteLine(installation is null
        ? "Steam installation not found."
        : $"Steam installation found at: {installation.RootPath}");
}

static void PrintLibraries(SteamInstallation? installation, CliOptions options)
{
    if (installation is null)
    {
        WriteNotFound(options);
        return;
    }

    var libraries = installation.LibraryFolders
        .Where(library => MatchesText(library.Path, options.Match))
        .Where(library => !options.AppId.HasValue || library.AppIds?.Contains(options.AppId.Value) == true)
        .ToArray();

    if (options.Json)
    {
        WriteJson(libraries);
        return;
    }

    Console.WriteLine($"Steam root: {installation.RootPath}");
    Console.WriteLine("Library folders:");

    foreach (var library in libraries)
    {
        var marker = library.IsDefault ? "*" : "-";
        Console.WriteLine($"  {marker} [{library.Key}] {library.Path}");
    }
}

static void PrintApps(SteamInstallation? installation, CliOptions options)
{
    if (installation is null)
    {
        WriteNotFound(options);
        return;
    }

    var scanner = new SteamLibraryScanner();
    var apps = scanner.ScanInstalledApps(installation)
        .Where(app => !options.AppId.HasValue || app.AppId == options.AppId.Value)
        .Where(app => MatchesText(app.Name, options.Match) || MatchesText(app.InstallDirectory, options.Match))
        .ToArray();

    if (options.Json)
    {
        WriteJson(apps);
        return;
    }

    if (apps.Length == 0)
    {
        Console.WriteLine("No installed Steam app manifests were found.");
        return;
    }

    Console.WriteLine($"Detected {apps.Length} installed Steam app(s):");

    foreach (var app in apps)
    {
        Console.WriteLine($"  - {app.AppId}: {app.Name} [{app.InstallDirectory}]");
    }
}

static void PrintCompatData(SteamInstallation? installation, CliOptions options)
{
    if (installation is null)
    {
        WriteNotFound(options);
        return;
    }

    var scanner = new SteamCompatDataScanner();
    var entries = scanner.Scan(installation)
        .Where(entry => !options.AppId.HasValue || entry.AppId == options.AppId.Value)
        .ToArray();

    if (options.Json)
    {
        WriteJson(entries);
        return;
    }

    if (entries.Length == 0)
    {
        Console.WriteLine("No compatdata entries were found.");
        return;
    }

    Console.WriteLine($"Detected {entries.Length} compatdata entr{(entries.Length == 1 ? "y" : "ies")}:");

    foreach (var entry in entries)
    {
        Console.WriteLine($"  - AppId {entry.AppId}: {entry.CompatDataPath}");
    }
}

static void PrintCompatibilityTools(SteamInstallation? installation, CliOptions options)
{
    if (installation is null)
    {
        WriteNotFound(options);
        return;
    }

    var scanner = new SteamCompatibilityToolScanner();
    var tools = scanner.Scan(installation)
        .Where(tool => MatchesText(tool.Name, options.Match) || MatchesText(tool.RootPath, options.Match))
        .ToArray();

    if (options.Json)
    {
        WriteJson(tools);
        return;
    }

    if (tools.Length == 0)
    {
        Console.WriteLine("No compatibility tools were found.");
        return;
    }

    Console.WriteLine($"Detected {tools.Length} compatibility tool(s):");

    foreach (var tool in tools)
    {
        var kind = tool.IsCustom ? "custom" : "bundled";
        Console.WriteLine($"  - {tool.Name} ({kind}) -> {tool.RootPath}");
    }
}

static void PrintCompatibilityMappings(SteamInstallation? installation, CliOptions options)
{
    if (installation is null)
    {
        WriteNotFound(options);
        return;
    }

    var parser = new SteamConfigCompatibilityParser();
    var configPath = Path.Combine(installation.RootPath, "config", "config.vdf");
    var mappings = parser.Parse(configPath)
        .Where(mapping => !options.AppId.HasValue || mapping.AppId == options.AppId.Value)
        .Where(mapping => MatchesText(mapping.ToolName, options.Match))
        .ToArray();

    if (options.Json)
    {
        WriteJson(mappings);
        return;
    }

    if (mappings.Length == 0)
    {
        Console.WriteLine("No explicit compatibility mappings were found in config.vdf.");
        return;
    }

    Console.WriteLine($"Detected {mappings.Length} explicit compatibility mapping(s):");

    foreach (var mapping in mappings)
    {
        Console.WriteLine($"  - AppId {mapping.AppId}: {mapping.ToolName}");
    }
}

static void PrintCompatibilityReport(SteamInstallation? installation, CliOptions options)
{
    if (installation is null)
    {
        WriteNotFound(options);
        return;
    }

    var reportService = new SteamCompatibilityReportService();
    var report = reportService.Build(installation)
        .Where(entry => !options.AppId.HasValue || entry.AppId == options.AppId.Value)
        .Where(entry => MatchesText(entry.Name, options.Match)
            || MatchesText(entry.AssignedTool, options.Match)
            || MatchesText(entry.InstallDirectory, options.Match))
        .ToArray();

    if (options.Json)
    {
        WriteJson(report);
        return;
    }

    if (report.Length == 0)
    {
        Console.WriteLine("No compatibility report entries could be built.");
        return;
    }

    Console.WriteLine($"Compatibility report entries: {report.Length}");

    foreach (var entry in report)
    {
        var compatData = entry.HasCompatData ? "yes" : "no";
        var tool = string.IsNullOrWhiteSpace(entry.AssignedTool) ? "<none>" : entry.AssignedTool;
        Console.WriteLine($"  - {entry.AppId}: {entry.Name} | compatdata={compatData} | tool={tool}");
    }
}

static void WriteNotFound(CliOptions options)
{
    if (options.Json)
    {
        WriteJson(new { error = "Steam installation not found." });
        return;
    }

    Console.WriteLine("Steam installation not found.");
}

static bool MatchesText(string? value, string? pattern)
{
    if (string.IsNullOrWhiteSpace(pattern))
    {
        return true;
    }

    return !string.IsNullOrWhiteSpace(value)
        && value.Contains(pattern, StringComparison.OrdinalIgnoreCase);
}

static void WriteJson<T>(T value)
{
    Console.WriteLine(JsonSerializer.Serialize(value, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
}

static void PrintUsage()
{
    Console.WriteLine("steam-utility-linux bootstrap");
    Console.WriteLine("Usage:");
    Console.WriteLine("  detect         Detect the local Steam installation path on Linux");
    Console.WriteLine("  libraries      List discovered Steam library folders");
    Console.WriteLine("  apps           List installed Steam apps from appmanifest files");
    Console.WriteLine("  compatdata     List per-app compatdata directories");
    Console.WriteLine("  compat-tools   List bundled and custom compatibility tools");
    Console.WriteLine("  compat-mapping List explicit compatibility-tool mappings from config.vdf");
    Console.WriteLine("  compat-report  Merge apps, compatdata, and config mappings into one report");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --json           Emit JSON output");
    Console.WriteLine("  --app-id <id>    Filter by AppID");
    Console.WriteLine("  --match <text>   Case-insensitive text filter");
}

internal sealed record CliOptions(string? Command, bool Json, int? AppId, string? Match)
{
    public static CliOptions Parse(string[] args)
    {
        string? command = null;
        var json = false;
        int? appId = null;
        string? match = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "--json":
                    json = true;
                    break;

                case "--app-id" when i + 1 < args.Length && int.TryParse(args[i + 1], out var parsedAppId):
                    appId = parsedAppId;
                    i++;
                    break;

                case "--match" when i + 1 < args.Length:
                    match = args[i + 1];
                    i++;
                    break;

                default:
                    command ??= arg.ToLowerInvariant();
                    break;
            }
        }

        return new CliOptions(command, json, appId, match);
    }
}
