using System.Runtime.InteropServices;
using SteamUtility.Core.Interop.Interfaces;

namespace SteamUtility.Core.Interop.Wrappers;

public sealed class SteamClient018 : NativeWrapper<ISteamClient018>
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int CreateSteamPipeNative(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool ReleaseSteamPipeNative(IntPtr self, int pipeHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ConnectToGlobalUserNative(IntPtr self, int pipeHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ReleaseUserNative(IntPtr self, int pipeHandle, int userHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetISteamUtilsNative(IntPtr self, int pipeHandle, IntPtr versionPointer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetISteamAppsNative(IntPtr self, int userHandle, int pipeHandle, IntPtr versionPointer);

    public int CreateSteamPipe()
    {
        return Call<int, CreateSteamPipeNative>(NativeFunctions.CreateSteamPipe, InstanceAddress);
    }

    public bool ReleaseSteamPipe(int pipeHandle)
    {
        return Call<bool, ReleaseSteamPipeNative>(NativeFunctions.ReleaseSteamPipe, InstanceAddress, pipeHandle);
    }

    public int ConnectToGlobalUser(int pipeHandle)
    {
        return Call<int, ConnectToGlobalUserNative>(NativeFunctions.ConnectToGlobalUser, InstanceAddress, pipeHandle);
    }

    public void ReleaseUser(int pipeHandle, int userHandle)
    {
        Call<ReleaseUserNative>(NativeFunctions.ReleaseUser, InstanceAddress, pipeHandle, userHandle);
    }

    public SteamUtils005 GetSteamUtils005(int pipeHandle)
    {
        return GetISteamUtils<SteamUtils005>(pipeHandle, "SteamUtils005");
    }

    public SteamApps008 GetSteamApps008(int userHandle, int pipeHandle)
    {
        return GetISteamApps<SteamApps008>(userHandle, pipeHandle, "STEAMAPPS_INTERFACE_VERSION008");
    }

    public SteamApps001 GetSteamApps001(int userHandle, int pipeHandle)
    {
        return GetISteamApps<SteamApps001>(userHandle, pipeHandle, "STEAMAPPS_INTERFACE_VERSION001");
    }

    private TInterface GetISteamUtils<TInterface>(int pipeHandle, string version)
        where TInterface : INativeWrapper, new()
    {
        using var versionHandle = Utf8StringHandle.From(version);
        var interfaceAddress = Call<IntPtr, GetISteamUtilsNative>(
            NativeFunctions.GetISteamUtils,
            InstanceAddress,
            pipeHandle,
            versionHandle.Pointer);

        var wrapper = new TInterface();
        wrapper.Initialize(interfaceAddress);
        return wrapper;
    }

    private TInterface GetISteamApps<TInterface>(int userHandle, int pipeHandle, string version)
        where TInterface : INativeWrapper, new()
    {
        using var versionHandle = Utf8StringHandle.From(version);
        var interfaceAddress = Call<IntPtr, GetISteamAppsNative>(
            NativeFunctions.GetISteamApps,
            InstanceAddress,
            userHandle,
            pipeHandle,
            versionHandle.Pointer);

        var wrapper = new TInterface();
        wrapper.Initialize(interfaceAddress);
        return wrapper;
    }
}
