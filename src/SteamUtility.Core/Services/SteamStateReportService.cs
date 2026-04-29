using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamStateReportService
{
    private readonly SteamLibraryScanner _libraryScanner = new();
    private readonly SteamCompatDataScanner _compatDataScanner = new();
    private readonly SteamCompatibilityToolScanner _compatibilityToolScanner = new();
    private readonly SteamConfigCompatibilityParser _configParser = new();
    private readonly SteamLoginUsersParser _loginUsersParser = new();
    private readonly SteamUserConfigScanner _userConfigScanner = new();

    public SteamEnvironmentSummary Build(SteamInstallation installation)
    {
        var apps = _libraryScanner.ScanInstalledApps(installation);
        var compatData = _compatDataScanner.Scan(installation);
        var tools = _compatibilityToolScanner.Scan(installation);
        var configPath = Path.Combine(installation.RootPath, "config", "config.vdf");
        var mappings = _configParser.Parse(configPath);

        var reportAppIds = new SortedSet<int>(apps.Select(static a => a.AppId));
        reportAppIds.UnionWith(compatData.Select(static c => c.AppId));
        reportAppIds.UnionWith(mappings.Select(static m => m.AppId));

        var loginUsers = LoadLoginUsers(installation.RootPath);
        var userConfigs = _userConfigScanner.Scan(installation);
        var activeUser = loginUsers.FirstOrDefault(user => user.MostRecent)
            ?? loginUsers.OrderByDescending(user => user.Timestamp ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        var activeUserConfigs = activeUser is null
            ? []
            : userConfigs.Where(entry => entry.SteamId == activeUser.SteamId).ToArray();
        var activeUserAppIds = activeUserConfigs
            .SelectMany(static entry => entry.AppIds)
            .Distinct()
            .OrderBy(static appId => appId)
            .ToArray();

        return new SteamEnvironmentSummary(
            RootPath: installation.RootPath,
            LibraryCount: installation.LibraryFolders.Count,
            InstalledAppCount: apps.Count,
            CompatDataCount: compatData.Count,
            CompatibilityToolCount: tools.Count,
            ExplicitCompatibilityMappings: mappings.Count,
            ReportEntryCount: reportAppIds.Count,
            LoginUserCount: loginUsers.Count,
            ActiveSteamId: activeUser?.SteamId,
            ActiveAccountName: activeUser?.AccountName ?? activeUser?.PersonaName,
            UserConfigFileCount: userConfigs.Count,
            UserAppScopeCount: userConfigs.Sum(entry => entry.AppIds.Count),
            ActiveUserConfigs: activeUserConfigs,
            ActiveUserAppIds: activeUserAppIds);
    }

    private IReadOnlyList<SteamLoginUser> LoadLoginUsers(string rootPath)
    {
        var loginUsersPath = Path.Combine(rootPath, "config", "loginusers.vdf");
        if (!File.Exists(loginUsersPath))
        {
            return [];
        }

        return _loginUsersParser.Parse(File.ReadAllText(loginUsersPath));
    }
}
