using SteamUtility.Core.Abstractions;
using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamworksSession : IDisposable
{
    private readonly uint _appId;
    private readonly ISteamApiLibraryResolver _libraryResolver;
    private readonly string? _previousSteamAppId;
    private readonly string? _previousSteamGameId;
    private readonly SteamAppIdFileState[] _steamAppIdFiles;
    private bool _initialized;

    public SteamworksSession(
        SteamInstallation installation,
        uint appId,
        ISteamApiLibraryResolver? libraryResolver = null)
    {
        _appId = appId;
        _libraryResolver = libraryResolver ?? SteamPlatformRuntime.Current.ApiLibraryResolver;
        _previousSteamAppId = Environment.GetEnvironmentVariable("SteamAppId");
        _previousSteamGameId = Environment.GetEnvironmentVariable("SteamGameId");
        _steamAppIdFiles = CaptureSteamAppIdFiles();

        try
        {
            SteamApiNative.EnsureLoaded(installation, _libraryResolver);
            Environment.SetEnvironmentVariable("SteamAppId", appId.ToString());
            Environment.SetEnvironmentVariable("SteamGameId", appId.ToString());
            WriteSteamAppIdFiles(appId);

            if (!SteamApiNative.Init())
            {
                throw new SteamworksInitializationException(
                    SteamworksInitializationFailure.ApiInitFailed,
                    "Failed to initialize Steam API. Make sure Steam is running and the selected app id is valid.");
            }

            SteamUser = SteamApiNative.GetSteamUser();
            SteamUserStats = SteamApiNative.GetSteamUserStats();
            SteamId = SteamApiNative.GetSteamId(SteamUser);

            if (SteamUser == IntPtr.Zero || SteamUserStats == IntPtr.Zero)
            {
                SteamApiNative.Shutdown();
                throw new SteamworksInitializationException(
                    SteamworksInitializationFailure.UserInterfaceResolutionFailed,
                    "Failed to resolve Steam user or user stats interfaces.");
            }

            _initialized = true;
        }
        catch
        {
            RestoreProcessState();
            throw;
        }
    }

    public IntPtr SteamUser { get; }

    public IntPtr SteamUserStats { get; }

    public ulong SteamId { get; }

    public void EnsureCurrentUserStatsLoaded(TimeSpan? timeout = null)
    {
        if (!SteamApiNative.RequestCurrentStats(SteamUserStats))
        {
            throw new SteamworksInitializationException(
                SteamworksInitializationFailure.CurrentStatsRequestFailed,
                "Failed to request current stats from Steam.");
        }

        RunCallbacksUntil(timeout ?? TimeSpan.FromSeconds(3));
    }

    public void RequestGlobalAchievementPercentages(TimeSpan? timeout = null)
    {
        if (SteamApiNative.RequestGlobalAchievementPercentages(SteamUserStats) == 0)
        {
            throw new SteamworksInitializationException(
                SteamworksInitializationFailure.GlobalAchievementPercentagesRequestFailed,
                "Failed to request global achievement percentages from Steam.");
        }

        RunCallbacksUntil(timeout ?? TimeSpan.FromSeconds(3));
    }

    public void RunCallbacksUntil(TimeSpan duration)
    {
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < duration)
        {
            SteamApiNative.RunCallbacks();
            Thread.Sleep(50);
        }
    }

    public bool GetStat(string name, out int value) => SteamApiNative.GetStat(SteamUserStats, name, out value);

    public bool GetStat(string name, out float value) => SteamApiNative.GetStat(SteamUserStats, name, out value);

    public bool SetStat(string name, int value) => SteamApiNative.SetStat(SteamUserStats, name, value);

    public bool SetStat(string name, float value) => SteamApiNative.SetStat(SteamUserStats, name, value);

    public bool GetAchievement(string name, out bool achieved) => SteamApiNative.GetAchievement(SteamUserStats, name, out achieved);

    public bool SetAchievement(string name) => SteamApiNative.SetAchievement(SteamUserStats, name);

    public bool ClearAchievement(string name) => SteamApiNative.ClearAchievement(SteamUserStats, name);

    public bool StoreStats() => SteamApiNative.StoreStats(SteamUserStats);

    public string GetAchievementDisplayAttribute(string name, string key) => SteamApiNative.GetAchievementDisplayAttribute(SteamUserStats, name, key);

    public uint GetNumAchievements() => SteamApiNative.GetNumAchievements(SteamUserStats);

    public string GetAchievementName(uint index) => SteamApiNative.GetAchievementName(SteamUserStats, index);

    public bool ResetAllStats(bool achievementsToo) => SteamApiNative.ResetAllStats(SteamUserStats, achievementsToo);

    public bool GetAchievementAchievedPercent(string name, out float percent) => SteamApiNative.GetAchievementAchievedPercent(SteamUserStats, name, out percent);

    public void Dispose()
    {
        if (_initialized)
        {
            SteamApiNative.Shutdown();
            _initialized = false;
        }

        RestoreProcessState();
    }

    private static SteamAppIdFileState[] CaptureSteamAppIdFiles()
    {
        return new[]
        {
            Path.Combine(AppContext.BaseDirectory, "steam_appid.txt"),
            Path.Combine(Directory.GetCurrentDirectory(), "steam_appid.txt")
        }
        .Distinct(StringComparer.Ordinal)
        .Select(path => new SteamAppIdFileState(path, File.Exists(path), File.Exists(path) ? File.ReadAllText(path) : null))
        .ToArray();
    }

    private void WriteSteamAppIdFiles(uint appId)
    {
        foreach (var file in _steamAppIdFiles)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file.Path)!);
            File.WriteAllText(file.Path, appId.ToString());
        }
    }

    private void RestoreProcessState()
    {
        Environment.SetEnvironmentVariable("SteamAppId", _previousSteamAppId);
        Environment.SetEnvironmentVariable("SteamGameId", _previousSteamGameId);

        foreach (var file in _steamAppIdFiles)
        {
            if (file.Existed)
            {
                File.WriteAllText(file.Path, file.Contents ?? string.Empty);
            }
            else if (File.Exists(file.Path))
            {
                File.Delete(file.Path);
            }
        }
    }

    private sealed record SteamAppIdFileState(string Path, bool Existed, string? Contents);
}
