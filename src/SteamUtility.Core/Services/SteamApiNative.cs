using System.Runtime.InteropServices;
using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

internal static class SteamApiNative
{
    private static IntPtr _libraryHandle;

    private static SteamApiInitDelegate? _steamApiInit;
    private static SteamApiShutdownDelegate? _steamApiShutdown;
    private static SteamApiRunCallbacksDelegate? _steamApiRunCallbacks;
    private static SteamUserV021Delegate? _steamUserV021;
    private static SteamUserStatsV012Delegate? _steamUserStatsV012;
    private static GetSteamIdDelegate? _getSteamId;
    private static RequestCurrentStatsDelegate? _requestCurrentStats;
    private static GetStatIntDelegate? _getStatInt;
    private static GetStatFloatDelegate? _getStatFloat;
    private static SetStatIntDelegate? _setStatInt;
    private static SetStatFloatDelegate? _setStatFloat;
    private static GetAchievementDelegate? _getAchievement;
    private static SetAchievementDelegate? _setAchievement;
    private static ClearAchievementDelegate? _clearAchievement;
    private static StoreStatsDelegate? _storeStats;
    private static GetAchievementDisplayAttributeDelegate? _getAchievementDisplayAttribute;
    private static GetNumAchievementsDelegate? _getNumAchievements;
    private static GetAchievementNameDelegate? _getAchievementName;
    private static ResetAllStatsDelegate? _resetAllStats;
    private static RequestGlobalAchievementPercentagesDelegate? _requestGlobalAchievementPercentages;
    private static GetAchievementAchievedPercentDelegate? _getAchievementAchievedPercent;

    public static void EnsureLoaded(SteamInstallation installation)
    {
        if (_libraryHandle != IntPtr.Zero)
        {
            return;
        }

        var libraryPath = LinuxSteamApiLibraryResolver.FindLibraryPath(installation)
            ?? throw new SteamworksInitializationException("Could not locate libsteam_api.so.");

        if (!NativeLibrary.TryLoad(libraryPath, out _libraryHandle))
        {
            throw new SteamworksInitializationException($"Failed to load libsteam_api.so from '{libraryPath}'.");
        }

        _steamApiInit = GetExport<SteamApiInitDelegate>("SteamAPI_Init");
        _steamApiShutdown = GetExport<SteamApiShutdownDelegate>("SteamAPI_Shutdown");
        _steamApiRunCallbacks = GetExport<SteamApiRunCallbacksDelegate>("SteamAPI_RunCallbacks");
        _steamUserV021 = GetExport<SteamUserV021Delegate>("SteamAPI_SteamUser_v021");
        _steamUserStatsV012 = GetExport<SteamUserStatsV012Delegate>("SteamAPI_SteamUserStats_v012");
        _getSteamId = GetExport<GetSteamIdDelegate>("SteamAPI_ISteamUser_GetSteamID");
        _requestCurrentStats = GetExport<RequestCurrentStatsDelegate>("SteamAPI_ISteamUserStats_RequestCurrentStats");
        _getStatInt = GetExport<GetStatIntDelegate>("SteamAPI_ISteamUserStats_GetStatInt32");
        _getStatFloat = GetExport<GetStatFloatDelegate>("SteamAPI_ISteamUserStats_GetStatFloat");
        _setStatInt = GetExport<SetStatIntDelegate>("SteamAPI_ISteamUserStats_SetStatInt32");
        _setStatFloat = GetExport<SetStatFloatDelegate>("SteamAPI_ISteamUserStats_SetStatFloat");
        _getAchievement = GetExport<GetAchievementDelegate>("SteamAPI_ISteamUserStats_GetAchievement");
        _setAchievement = GetExport<SetAchievementDelegate>("SteamAPI_ISteamUserStats_SetAchievement");
        _clearAchievement = GetExport<ClearAchievementDelegate>("SteamAPI_ISteamUserStats_ClearAchievement");
        _storeStats = GetExport<StoreStatsDelegate>("SteamAPI_ISteamUserStats_StoreStats");
        _getAchievementDisplayAttribute = GetExport<GetAchievementDisplayAttributeDelegate>("SteamAPI_ISteamUserStats_GetAchievementDisplayAttribute");
        _getNumAchievements = GetExport<GetNumAchievementsDelegate>("SteamAPI_ISteamUserStats_GetNumAchievements");
        _getAchievementName = GetExport<GetAchievementNameDelegate>("SteamAPI_ISteamUserStats_GetAchievementName");
        _resetAllStats = GetExport<ResetAllStatsDelegate>("SteamAPI_ISteamUserStats_ResetAllStats");
        _requestGlobalAchievementPercentages = GetExport<RequestGlobalAchievementPercentagesDelegate>("SteamAPI_ISteamUserStats_RequestGlobalAchievementPercentages");
        _getAchievementAchievedPercent = GetExport<GetAchievementAchievedPercentDelegate>("SteamAPI_ISteamUserStats_GetAchievementAchievedPercent");
    }

    public static bool Init() => _steamApiInit!();

    public static void Shutdown() => _steamApiShutdown!();

    public static void RunCallbacks() => _steamApiRunCallbacks!();

    public static IntPtr GetSteamUser() => _steamUserV021!();

    public static IntPtr GetSteamUserStats() => _steamUserStatsV012!();

    public static ulong GetSteamId(IntPtr steamUser) => _getSteamId!(steamUser);

    public static bool RequestCurrentStats(IntPtr steamUserStats) => _requestCurrentStats!(steamUserStats);

    public static bool GetStat(IntPtr steamUserStats, string name, out int value) => _getStatInt!(steamUserStats, name, out value);

    public static bool GetStat(IntPtr steamUserStats, string name, out float value) => _getStatFloat!(steamUserStats, name, out value);

    public static bool SetStat(IntPtr steamUserStats, string name, int value) => _setStatInt!(steamUserStats, name, value);

    public static bool SetStat(IntPtr steamUserStats, string name, float value) => _setStatFloat!(steamUserStats, name, value);

    public static bool GetAchievement(IntPtr steamUserStats, string name, out bool achieved) => _getAchievement!(steamUserStats, name, out achieved);

    public static bool SetAchievement(IntPtr steamUserStats, string name) => _setAchievement!(steamUserStats, name);

    public static bool ClearAchievement(IntPtr steamUserStats, string name) => _clearAchievement!(steamUserStats, name);

    public static bool StoreStats(IntPtr steamUserStats) => _storeStats!(steamUserStats);

    public static string GetAchievementDisplayAttribute(IntPtr steamUserStats, string name, string key)
    {
        var pointer = _getAchievementDisplayAttribute!(steamUserStats, name, key);
        return pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
    }

    public static uint GetNumAchievements(IntPtr steamUserStats) => _getNumAchievements!(steamUserStats);

    public static string GetAchievementName(IntPtr steamUserStats, uint index)
    {
        var pointer = _getAchievementName!(steamUserStats, index);
        return pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
    }

    public static bool ResetAllStats(IntPtr steamUserStats, bool achievementsToo) => _resetAllStats!(steamUserStats, achievementsToo);

    public static ulong RequestGlobalAchievementPercentages(IntPtr steamUserStats) => _requestGlobalAchievementPercentages!(steamUserStats);

    public static bool GetAchievementAchievedPercent(IntPtr steamUserStats, string name, out float percent)
        => _getAchievementAchievedPercent!(steamUserStats, name, out percent);

    private static TDelegate GetExport<TDelegate>(string exportName)
        where TDelegate : Delegate
    {
        if (!NativeLibrary.TryGetExport(_libraryHandle, exportName, out var export))
        {
            throw new SteamworksInitializationException($"Steam API export '{exportName}' was not found.");
        }

        return Marshal.GetDelegateForFunctionPointer<TDelegate>(export);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool SteamApiInitDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SteamApiShutdownDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SteamApiRunCallbacksDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SteamUserV021Delegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SteamUserStatsV012Delegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ulong GetSteamIdDelegate(IntPtr steamUser);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool RequestCurrentStatsDelegate(IntPtr steamUserStats);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool GetStatIntDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, out int data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool GetStatFloatDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, out float data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool SetStatIntDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool SetStatFloatDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool GetAchievementDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.I1)] out bool achieved);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool SetAchievementDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool ClearAchievementDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool StoreStatsDelegate(IntPtr steamUserStats);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetAchievementDisplayAttributeDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.LPUTF8Str)] string achievementName, [MarshalAs(UnmanagedType.LPUTF8Str)] string key);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint GetNumAchievementsDelegate(IntPtr steamUserStats);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetAchievementNameDelegate(IntPtr steamUserStats, uint index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool ResetAllStatsDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.I1)] bool achievementsToo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ulong RequestGlobalAchievementPercentagesDelegate(IntPtr steamUserStats);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool GetAchievementAchievedPercentDelegate(IntPtr steamUserStats, [MarshalAs(UnmanagedType.LPUTF8Str)] string achievementName, out float percent);
}
