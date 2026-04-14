using System.Diagnostics;
using SteamUtility.Core.Abstractions;

namespace SteamUtility.Tests.Fakes;

internal static class FakeSteamApiLibrary
{
    private static readonly Lazy<string> FullLibraryPathCache = new(() => BuildLibrary("full"));
    private static readonly Lazy<string> InitFailureLibraryPathCache = new(() => BuildLibrary("init-failure", "FAKE_STEAM_API_INIT_FAILURE"));
    private static readonly Lazy<string> MissingExportLibraryPathCache = new(() => BuildLibrary("missing-export", "FAKE_STEAM_API_MISSING_EXPORTS"));

    public static string FullLibraryPath => FullLibraryPathCache.Value;

    public static string InitFailureLibraryPath => InitFailureLibraryPathCache.Value;

    public static string MissingExportLibraryPath => MissingExportLibraryPathCache.Value;

    public static ISteamApiLibraryResolver CreateResolver(string libraryPath)
        => new FixedPathSteamApiLibraryResolver(libraryPath);

    private static string BuildLibrary(string variant, string? define = null)
    {
        var buildRoot = Path.Combine(Path.GetTempPath(), "steam-utility-multiplataform-fakes", "steam-api", variant);
        Directory.CreateDirectory(buildRoot);

        var outputPath = Path.Combine(buildRoot, "libsteam_api.so");
        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        var sourcePath = Path.Combine(buildRoot, "fake_steam_api.c");
        File.WriteAllText(sourcePath, Source);

        var processStartInfo = new ProcessStartInfo("gcc")
        {
            WorkingDirectory = buildRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        processStartInfo.ArgumentList.Add("-shared");
        processStartInfo.ArgumentList.Add("-fPIC");
        processStartInfo.ArgumentList.Add("-O2");
        processStartInfo.ArgumentList.Add("-std=c11");

        if (define is not null)
        {
            processStartInfo.ArgumentList.Add($"-D{define}");
        }

        processStartInfo.ArgumentList.Add("-o");
        processStartInfo.ArgumentList.Add(outputPath);
        processStartInfo.ArgumentList.Add(sourcePath);

        using var process = Process.Start(processStartInfo)
            ?? throw new InvalidOperationException("Failed to start gcc.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Failed to compile fake Steam API library '{outputPath}'.{Environment.NewLine}{stdout}{stderr}");
        }

        return outputPath;
    }

    private sealed class FixedPathSteamApiLibraryResolver(string libraryPath) : ISteamApiLibraryResolver
    {
        public string? FindLibraryPath(SteamUtility.Core.Models.SteamInstallation installation) => libraryPath;
    }

    private const string Source = """
#include <stdbool.h>
#include <stdint.h>
#include <string.h>

#if defined(_WIN32)
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif

static int g_initial_int_stat = 7;
static float g_initial_float_stat = 19.5f;
static float g_initial_average_rate = 2.25f;
static int g_current_int_stat = 7;
static float g_current_float_stat = 19.5f;
static float g_current_average_rate = 2.25f;
static int g_achievement_unlocked = 0;

static int g_dummy_user;
static int g_dummy_stats;
static const uint64_t g_steam_id = 76561198893709131ULL;

static const char* ResolveAchievementAttribute(const char* key)
{
    if (strcmp(key, "name") == 0)
    {
        return "n00b";
    }

    if (strcmp(key, "desc") == 0)
    {
        return "Tutorial level completed";
    }

    if (strcmp(key, "icon") == 0)
    {
        return "achievement-icon.png";
    }

    if (strcmp(key, "icon_gray") == 0)
    {
        return "achievement-icon-locked.png";
    }

    if (strcmp(key, "hidden") == 0)
    {
        return "0";
    }

    return "";
}

EXPORT bool SteamAPI_Init(void)
{
#if defined(FAKE_STEAM_API_INIT_FAILURE)
    return false;
#else
    return true;
#endif
}

EXPORT void SteamAPI_Shutdown(void)
{
}

EXPORT void SteamAPI_RunCallbacks(void)
{
}

EXPORT void* SteamAPI_SteamUser_v021(void)
{
    return &g_dummy_user;
}

EXPORT void* SteamAPI_SteamUserStats_v012(void)
{
    return &g_dummy_stats;
}

EXPORT uint64_t SteamAPI_ISteamUser_GetSteamID(void* steamUser)
{
    (void)steamUser;
    return g_steam_id;
}

EXPORT bool SteamAPI_ISteamUserStats_RequestCurrentStats(void* steamUserStats)
{
    (void)steamUserStats;
    return true;
}

EXPORT bool SteamAPI_ISteamUserStats_GetStatInt32(void* steamUserStats, const char* name, int* data)
{
    (void)steamUserStats;

    if (strcmp(name, "hedGamesPlayed") == 0)
    {
        *data = g_current_int_stat;
        return true;
    }

    return false;
}

EXPORT bool SteamAPI_ISteamUserStats_GetStatFloat(void* steamUserStats, const char* name, float* data)
{
    (void)steamUserStats;

    if (strcmp(name, "hedAccuracy") == 0)
    {
        *data = g_current_float_stat;
        return true;
    }

    if (strcmp(name, "hedAverageRate") == 0)
    {
        *data = g_current_average_rate;
        return true;
    }

    return false;
}

EXPORT bool SteamAPI_ISteamUserStats_SetStatInt32(void* steamUserStats, const char* name, int value)
{
    (void)steamUserStats;

    if (strcmp(name, "hedGamesPlayed") == 0)
    {
        g_current_int_stat = value;
        return true;
    }

    return false;
}

EXPORT bool SteamAPI_ISteamUserStats_SetStatFloat(void* steamUserStats, const char* name, float value)
{
    (void)steamUserStats;

    if (strcmp(name, "hedAccuracy") == 0)
    {
        g_current_float_stat = value;
        return true;
    }

    if (strcmp(name, "hedAverageRate") == 0)
    {
        g_current_average_rate = value;
        return true;
    }

    return false;
}

EXPORT bool SteamAPI_ISteamUserStats_GetAchievement(void* steamUserStats, const char* name, bool* achieved)
{
    (void)steamUserStats;

    if (strcmp(name, "ACH_TUTORIAL_COMPLETED") == 0)
    {
        *achieved = g_achievement_unlocked != 0;
        return true;
    }

    return false;
}

EXPORT bool SteamAPI_ISteamUserStats_SetAchievement(void* steamUserStats, const char* name)
{
    (void)steamUserStats;

    if (strcmp(name, "ACH_TUTORIAL_COMPLETED") == 0)
    {
        g_achievement_unlocked = 1;
        return true;
    }

    return false;
}

EXPORT bool SteamAPI_ISteamUserStats_ClearAchievement(void* steamUserStats, const char* name)
{
    (void)steamUserStats;

    if (strcmp(name, "ACH_TUTORIAL_COMPLETED") == 0)
    {
        g_achievement_unlocked = 0;
        return true;
    }

    return false;
}

EXPORT bool SteamAPI_ISteamUserStats_StoreStats(void* steamUserStats)
{
    (void)steamUserStats;
    return true;
}

EXPORT const char* SteamAPI_ISteamUserStats_GetAchievementDisplayAttribute(
    void* steamUserStats,
    const char* achievementName,
    const char* key)
{
    (void)steamUserStats;

    if (strcmp(achievementName, "ACH_TUTORIAL_COMPLETED") != 0)
    {
        return "";
    }

    return ResolveAchievementAttribute(key);
}

EXPORT uint32_t SteamAPI_ISteamUserStats_GetNumAchievements(void* steamUserStats)
{
    (void)steamUserStats;
    return 1;
}

EXPORT const char* SteamAPI_ISteamUserStats_GetAchievementName(void* steamUserStats, uint32_t index)
{
    (void)steamUserStats;
    return index == 0 ? "ACH_TUTORIAL_COMPLETED" : "";
}

EXPORT bool SteamAPI_ISteamUserStats_ResetAllStats(void* steamUserStats, bool achievementsToo)
{
    (void)steamUserStats;

    g_current_int_stat = g_initial_int_stat;
    g_current_float_stat = g_initial_float_stat;
    g_current_average_rate = g_initial_average_rate;

    if (achievementsToo)
    {
        g_achievement_unlocked = 0;
    }

    return true;
}

EXPORT uint64_t SteamAPI_ISteamUserStats_RequestGlobalAchievementPercentages(void* steamUserStats)
{
    (void)steamUserStats;
    return 1;
}

#ifndef FAKE_STEAM_API_MISSING_EXPORTS
EXPORT bool SteamAPI_ISteamUserStats_GetAchievementAchievedPercent(
    void* steamUserStats,
    const char* achievementName,
    float* percent)
{
    (void)steamUserStats;

    if (strcmp(achievementName, "ACH_TUTORIAL_COMPLETED") == 0)
    {
        *percent = 28.2f;
        return true;
    }

    return false;
}
#endif
""";
}
