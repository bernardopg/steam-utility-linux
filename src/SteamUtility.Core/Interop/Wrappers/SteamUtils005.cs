using System.Runtime.InteropServices;
using SteamUtility.Core.Interop.Interfaces;

namespace SteamUtility.Core.Interop.Wrappers;

public sealed class SteamUtils005 : NativeWrapper<ISteamUtils005>
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint GetAppIdNative(IntPtr self);

    public uint GetAppId()
    {
        return Call<uint, GetAppIdNative>(NativeFunctions.GetAppID, InstanceAddress);
    }
}
