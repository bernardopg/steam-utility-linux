using System.Text.Json;
using SteamUtility.Core.Abstractions;
using SteamUtility.Core.Models;
using SteamUtility.Core.Services;
using SteamUtility.Core.Utils;

const string DefaultOwnershipAppIdsUrl =
    "https://raw.githubusercontent.com/zevnda/steam-game-database/refs/heads/main/games.json";

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

    case "check_ownership":
    case "check-ownership":
        PrintCheckOwnership(installation, options);
        return;

    case "idle":
        PrintIdle(installation, options);
        return;

    case "get_achievement_data":
    case "get-achievement-data":
        PrintAchievementData(installation, options);
        return;

    case "unlock_achievement":
    case "unlock-achievement":
        PrintToggleSingleAchievement(installation, options, shouldUnlock: true);
        return;

    case "lock_achievement":
    case "lock-achievement":
        PrintToggleSingleAchievement(installation, options, shouldUnlock: false);
        return;

    case "toggle_achievement":
    case "toggle-achievement":
        PrintToggleAchievement(installation, options);
        return;

    case "unlock_all_achievements":
    case "unlock-all-achievements":
        PrintToggleAllAchievements(installation, options, shouldUnlock: true);
        return;

    case "lock_all_achievements":
    case "lock-all-achievements":
        PrintToggleAllAchievements(installation, options, shouldUnlock: false);
        return;

    case "update_stats":
    case "update-stats":
        PrintUpdateStats(installation, options);
        return;

    case "reset_all_stats":
    case "reset-all-stats":
        PrintResetAllStats(installation, options);
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

static void PrintCheckOwnership(SteamInstallation? installation, CliOptions options)
{
    if (options.Positionals.Length < 1)
    {
        PrintCheckOwnershipUsage();
        return;
    }

    if (installation is null)
    {
        WriteOwnershipStatus(
            options,
            success: false,
            totalChecked: 0,
            ownedCount: 0,
            outputPath: options.Positionals[0],
            error: "Steam installation not found.",
            suggestion: "Make sure Steam is installed for the current Linux user.",
            failureReason: SteamClientInitializeFailure.InstallPathNotFound.ToString());
        return;
    }

    List<uint> appIds;

    try
    {
        appIds = LoadOwnershipAppIds(options.Positionals.ElementAtOrDefault(1));
    }
    catch (Exception ex)
    {
        WriteOwnershipStatus(
            options,
            success: false,
            totalChecked: 0,
            ownedCount: 0,
            outputPath: options.Positionals[0],
            error: $"Failed to load app ids: {ex.Message}");
        return;
    }

    try
    {
        var service = new SteamOwnershipService();
        var ownedApps = service.GetOwnedApps(installation, appIds);
        var outputPath = options.Positionals[0];
        var outputDirectory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var payload = new
        {
            games = ownedApps.Select(app => new
            {
                appid = app.AppId,
                name = app.Name
            })
        };

        File.WriteAllText(outputPath, JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = false
        }));

        WriteOwnershipStatus(
            options,
            success: true,
            totalChecked: appIds.Count,
            ownedCount: ownedApps.Count,
            outputPath: outputPath);
    }
    catch (SteamClientInitializeException ex)
    {
        WriteOwnershipStatus(
            options,
            success: false,
            totalChecked: appIds.Count,
            ownedCount: 0,
            outputPath: options.Positionals[0],
            error: ex.Message,
            suggestion: "Make sure Steam is running and logged in before retrying.",
            failureReason: ex.FailureReason.ToString());
    }
}

static void PrintIdle(SteamInstallation? installation, CliOptions options)
{
    if (!TryGetSteamworksAppId(installation, options, out var appId))
    {
        return;
    }

    try
    {
        using var session = new SteamworksSession(installation!, appId);
        using var cancellation = new CancellationTokenSource();

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        WriteLegacyJson(new
        {
            success = "Steam API initialized",
            appId,
            mode = "linux",
            note = "Linux port does not create the Win32 idle window; it keeps the Steam API session alive until Ctrl+C."
        });

        while (!cancellation.IsCancellationRequested)
        {
            session.RunCallbacksUntil(TimeSpan.FromSeconds(1));
        }
    }
    catch (Exception ex)
    {
        WriteLegacyJson(new { error = ex.Message });
    }
}

static void PrintAchievementData(SteamInstallation? installation, CliOptions options)
{
    if (!TryGetSteamworksAppId(installation, options, out var appId))
    {
        return;
    }

    var cacheDirectory = options.Positionals.ElementAtOrDefault(1);
    var itemId = options.Positionals.ElementAtOrDefault(2);

    try
    {
        using var session = new SteamworksSession(installation!, appId);
        session.EnsureCurrentUserStatsLoaded();
        session.RequestGlobalAchievementPercentages();

        var schemaLoader = new StatsSchemaLoader();
        if (!schemaLoader.LoadUserGameStatsSchema(installation!, appId, out var achievements, out var stats))
        {
            achievements = [];
            stats = [];
        }

        if (!string.IsNullOrWhiteSpace(itemId))
        {
            var achievement = achievements.FirstOrDefault(achievement => achievement.Id == itemId);
            if (achievement is not null)
            {
                var flags = StatFlagHelper.GetFlags(achievement.Permission, incrementOnly: false, isAchievement: true);
                WriteJson(new
                {
                    type = "achievement",
                    id = achievement.Id,
                    name = achievement.Name,
                    description = achievement.Description,
                    iconNormal = achievement.IconNormal,
                    iconLocked = achievement.IconLocked,
                    permission = achievement.Permission,
                    hidden = achievement.IsHidden,
                    achieved = achievement.Achieved,
                    percent = achievement.Percent,
                    @protected = (flags & StatFlags.Protected) != 0,
                    flags = flags.ToString()
                });
                return;
            }

            var stat = stats.FirstOrDefault(stat => stat.Id == itemId);
            if (stat is not null)
            {
                var flags = StatFlagHelper.GetFlags(stat.Permission, stat.IncrementOnly);
                WriteJson(new
                {
                    type = "stat",
                    id = stat.Id,
                    name = stat.Name,
                    stat_type = stat.Type,
                    permission = stat.Permission,
                    min_value = stat.MinValue,
                    max_value = stat.MaxValue,
                    default_value = stat.DefaultValue,
                    value = stat.Value,
                    increment_only = stat.IncrementOnly,
                    @protected = (flags & StatFlags.Protected) != 0,
                    flags = flags.ToString()
                });
                return;
            }

            WriteLegacyJson(new { error = "ID not found" });
            return;
        }

        var payload = new
        {
            achievements = achievements.Select(achievement =>
            {
                var flags = StatFlagHelper.GetFlags(achievement.Permission, incrementOnly: false, isAchievement: true);
                return new
                {
                    id = achievement.Id,
                    name = achievement.Name,
                    description = achievement.Description,
                    iconNormal = achievement.IconNormal,
                    iconLocked = achievement.IconLocked,
                    permission = achievement.Permission,
                    hidden = achievement.IsHidden,
                    achieved = achievement.Achieved,
                    percent = achievement.Percent,
                    protected_achievement = (flags & StatFlags.Protected) != 0,
                    flags = flags.ToString()
                };
            }),
            stats = stats.Select(stat =>
            {
                var flags = StatFlagHelper.GetFlags(stat.Permission, stat.IncrementOnly);
                return new
                {
                    id = stat.Id,
                    name = stat.Name,
                    stat_type = stat.Type,
                    permission = stat.Permission,
                    value = stat.Value,
                    increment_only = stat.IncrementOnly,
                    protected_stat = (flags & StatFlags.Protected) != 0,
                    flags = flags.ToString()
                };
            })
        };

        var filePath = GetAchievementDataPath(session.SteamId, appId, cacheDirectory);
        File.WriteAllText(filePath, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        WriteLegacyJson(new { success = filePath });
    }
    catch (Exception ex)
    {
        WriteLegacyJson(new { error = ex.Message });
    }
}

static void PrintToggleSingleAchievement(SteamInstallation? installation, CliOptions options, bool shouldUnlock)
{
    if (!TryGetSteamworksAppId(installation, options, out var appId) || options.Positionals.Length < 2)
    {
        if (installation is not null && options.Positionals.Length < 2)
        {
            WriteLegacyJson(new { error = "achievement_id is required" });
        }

        return;
    }

    var achievementId = options.Positionals[1];

    try
    {
        using var session = new SteamworksSession(installation!, appId);
        session.EnsureCurrentUserStatsLoaded();

        if (!session.GetAchievement(achievementId, out _))
        {
            WriteLegacyJson(new { error = "Failed to get achievement data. The achievement might not exist" });
            return;
        }

        var success = shouldUnlock ? session.SetAchievement(achievementId) : session.ClearAchievement(achievementId);

        if (!success || !session.StoreStats())
        {
            WriteLegacyJson(new { error = shouldUnlock ? "Failed to unlock achievement" : "Failed to lock achievement" });
            return;
        }

        WriteLegacyJson(new { success = shouldUnlock ? "Successfully unlocked achievement" : "Successfully locked achievement" });
    }
    catch (Exception ex)
    {
        WriteLegacyJson(new { error = ex.Message });
    }
}

static void PrintToggleAchievement(SteamInstallation? installation, CliOptions options)
{
    if (!TryGetSteamworksAppId(installation, options, out var appId) || options.Positionals.Length < 2)
    {
        if (installation is not null && options.Positionals.Length < 2)
        {
            WriteLegacyJson(new { error = "achievement_id is required" });
        }

        return;
    }

    var achievementId = options.Positionals[1];

    try
    {
        using var session = new SteamworksSession(installation!, appId);
        session.EnsureCurrentUserStatsLoaded();

        if (!session.GetAchievement(achievementId, out var achieved))
        {
            WriteLegacyJson(new { error = "Failed to get achievement data. The achievement might not exist" });
            return;
        }

        var success = achieved ? session.ClearAchievement(achievementId) : session.SetAchievement(achievementId);

        if (!success || !session.StoreStats())
        {
            WriteLegacyJson(new { error = achieved ? "Failed to lock achievement" : "Failed to unlock achievement" });
            return;
        }

        WriteLegacyJson(new { success = achieved ? "Successfully locked achievement" : "Successfully unlocked achievement" });
    }
    catch (Exception ex)
    {
        WriteLegacyJson(new { error = ex.Message });
    }
}

static void PrintToggleAllAchievements(SteamInstallation? installation, CliOptions options, bool shouldUnlock)
{
    if (!TryGetSteamworksAppId(installation, options, out var appId))
    {
        return;
    }

    try
    {
        using var session = new SteamworksSession(installation!, appId);
        session.EnsureCurrentUserStatsLoaded();

        if (!shouldUnlock)
        {
            if (!session.ResetAllStats(true) || !session.StoreStats())
            {
                WriteLegacyJson(new { error = "Failed to lock all achievements" });
                return;
            }

            WriteLegacyJson(new { success = "Successfully lock all achievements" });
            return;
        }

        var total = (int)session.GetNumAchievements();
        var allSucceeded = true;

        for (var index = 0; index < total; index++)
        {
            var achievementId = session.GetAchievementName((uint)index);
            if (!session.SetAchievement(achievementId))
            {
                allSucceeded = false;
            }
        }

        if (!allSucceeded || !session.StoreStats())
        {
            WriteLegacyJson(new { error = "One or more achievements failed to unlock" });
            return;
        }

        WriteLegacyJson(new { success = "Successfully unlocked all achievements" });
    }
    catch (Exception ex)
    {
        WriteLegacyJson(new { error = ex.Message });
    }
}

static void PrintUpdateStats(SteamInstallation? installation, CliOptions options)
{
    if (!TryGetSteamworksAppId(installation, options, out var appId) || options.Positionals.Length < 2)
    {
        if (installation is not null && options.Positionals.Length < 2)
        {
            WriteLegacyJson(new { error = "stats JSON array is required" });
        }

        return;
    }

    StatUpdate[] updates;
    try
    {
        updates = JsonSerializer.Deserialize<StatUpdate[]>(
            options.Positionals[1],
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }
    catch (Exception ex)
    {
        WriteLegacyJson(new { error = $"Invalid stats format: {ex.Message}" });
        return;
    }

    try
    {
        using var session = new SteamworksSession(installation!, appId);
        session.EnsureCurrentUserStatsLoaded();

        var allSucceeded = true;

        foreach (var update in updates)
        {
            if (!TryApplyStatUpdate(session, update))
            {
                allSucceeded = false;
            }
        }

        if (!allSucceeded)
        {
            WriteLegacyJson(new { error = "One or more stats failed to update" });
            return;
        }

        if (!session.StoreStats())
        {
            WriteLegacyJson(new { error = "Failed to store updated stats" });
            return;
        }

        WriteLegacyJson(new { success = "Successfully updated all stats" });
    }
    catch (Exception ex)
    {
        WriteLegacyJson(new { error = ex.Message });
    }
}

static void PrintResetAllStats(SteamInstallation? installation, CliOptions options)
{
    if (!TryGetSteamworksAppId(installation, options, out var appId))
    {
        return;
    }

    try
    {
        using var session = new SteamworksSession(installation!, appId);
        session.EnsureCurrentUserStatsLoaded();

        if (!session.ResetAllStats(false) || !session.StoreStats())
        {
            WriteLegacyJson(new { error = "Failed to reset all stats" });
            return;
        }

        WriteLegacyJson(new { success = "Successfully reset all stats" });
    }
    catch (Exception ex)
    {
        WriteLegacyJson(new { error = ex.Message });
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

static void WriteOwnershipStatus(
    CliOptions options,
    bool success,
    int totalChecked,
    int ownedCount,
    string outputPath,
    string? error = null,
    string? suggestion = null,
    string? failureReason = null)
{
    var payload = new
    {
        success,
        totalChecked,
        ownedCount,
        outputPath,
        error,
        suggestion,
        failureReason
    };

    if (options.Json || !success)
    {
        WriteJson(payload);
        return;
    }

    Console.WriteLine($"Ownership scan finished. checked={totalChecked} owned={ownedCount} output={outputPath}");
}

static bool TryGetSteamworksAppId(SteamInstallation? installation, CliOptions options, out uint appId)
{
    appId = 0;

    if (installation is null)
    {
        WriteLegacyJson(new { error = "Steam installation not found." });
        return false;
    }

    if (options.Positionals.Length < 1 || !uint.TryParse(options.Positionals[0], out appId))
    {
        WriteLegacyJson(new { error = "Invalid app_id" });
        return false;
    }

    return true;
}

static string GetAchievementDataPath(ulong steamId, uint appId, string? cacheDirectory)
{
    var root = string.IsNullOrWhiteSpace(cacheDirectory)
        ? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cache",
            "steam-utility-linux")
        : cacheDirectory;

    var targetDirectory = Path.Combine(root, steamId.ToString(), "achievement_data");
    Directory.CreateDirectory(targetDirectory);
    return Path.Combine(targetDirectory, $"{appId}.json");
}

static bool TryApplyStatUpdate(SteamworksSession session, StatUpdate update)
{
    if (string.IsNullOrWhiteSpace(update.Name))
    {
        return false;
    }

    return update.Value switch
    {
        JsonElement { ValueKind: JsonValueKind.Number } element when element.TryGetInt32(out var intValue)
            => session.SetStat(update.Name, intValue),
        JsonElement { ValueKind: JsonValueKind.Number } element when element.TryGetSingle(out var floatValue)
            => session.SetStat(update.Name, floatValue),
        int intValue => session.SetStat(update.Name, intValue),
        long longValue when longValue is >= int.MinValue and <= int.MaxValue
            => session.SetStat(update.Name, (int)longValue),
        float floatValue => session.SetStat(update.Name, floatValue),
        double doubleValue => session.SetStat(update.Name, (float)doubleValue),
        decimal decimalValue => session.SetStat(update.Name, (float)decimalValue),
        string stringValue when int.TryParse(stringValue, out var parsedInt)
            => session.SetStat(update.Name, parsedInt),
        string stringValue when float.TryParse(stringValue, out var parsedFloat)
            => session.SetStat(update.Name, parsedFloat),
        _ => false
    };
}

static void WriteLegacyJson<T>(T payload)
{
    Console.WriteLine(JsonSerializer.Serialize(payload));
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
    Console.WriteLine("steam-utility-linux");
    Console.WriteLine("Usage:");
    Console.WriteLine("  detect         Detect the local Steam installation path on Linux");
    Console.WriteLine("  libraries      List discovered Steam library folders");
    Console.WriteLine("  apps           List installed Steam apps from appmanifest files");
    Console.WriteLine("  compatdata     List per-app compatdata directories");
    Console.WriteLine("  compat-tools   List bundled and custom compatibility tools");
    Console.WriteLine("  compat-mapping List explicit compatibility-tool mappings from config.vdf");
    Console.WriteLine("  compat-report  Merge apps, compatdata, and config mappings into one report");
    Console.WriteLine("  check_ownership <output_path> [app_ids_json_or_file]");
    Console.WriteLine("                 Check account ownership through steamclient.so and write games.json");
    Console.WriteLine("  idle <app_id> [app_name]");
    Console.WriteLine("  get_achievement_data <app_id> [storage_dir] [item_id]");
    Console.WriteLine("  unlock_achievement <app_id> <achievement_id>");
    Console.WriteLine("  lock_achievement <app_id> <achievement_id>");
    Console.WriteLine("  toggle_achievement <app_id> <achievement_id>");
    Console.WriteLine("  unlock_all_achievements <app_id>");
    Console.WriteLine("  lock_all_achievements <app_id>");
    Console.WriteLine("  update_stats <app_id> <json_array>");
    Console.WriteLine("  reset_all_stats <app_id>");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --json           Emit JSON output");
    Console.WriteLine("  --app-id <id>    Filter by AppID");
    Console.WriteLine("  --match <text>   Case-insensitive text filter");
}

static void PrintCheckOwnershipUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  check_ownership <output_path> [app_ids_json_or_file]");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  check_ownership /tmp/games.json");
    Console.WriteLine("  check_ownership /tmp/games.json \"[730,570,440]\"");
    Console.WriteLine("  check_ownership /tmp/games.json /path/to/app_ids.json");
}

static List<uint> LoadOwnershipAppIds(string? source)
{
    if (string.IsNullOrWhiteSpace(source))
    {
        using var httpClient = new HttpClient();
        var content = httpClient.GetStringAsync(DefaultOwnershipAppIdsUrl).GetAwaiter().GetResult();
        return JsonSerializer.Deserialize<List<uint>>(content) ?? [];
    }

    if (File.Exists(source))
    {
        var content = File.ReadAllText(source);
        return JsonSerializer.Deserialize<List<uint>>(content) ?? [];
    }

    return JsonSerializer.Deserialize<List<uint>>(source) ?? [];
}

internal sealed record CliOptions(string? Command, bool Json, int? AppId, string? Match, string[] Positionals)
{
    public static CliOptions Parse(string[] args)
    {
        string? command = null;
        var json = false;
        int? appId = null;
        string? match = null;
        var positionals = new List<string>();

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
                    if (command is null)
                    {
                        command = arg.ToLowerInvariant();
                    }
                    else
                    {
                        positionals.Add(arg);
                    }
                    break;
            }
        }

        return new CliOptions(command, json, appId, match, positionals.ToArray());
    }
}
