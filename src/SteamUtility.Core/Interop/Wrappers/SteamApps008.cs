using System.Runtime.InteropServices;
using SteamUtility.Core.Interop.Interfaces;

namespace SteamUtility.Core.Interop.Wrappers;

public sealed class SteamApps008 : NativeWrapper<ISteamApps008>
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool IsSubscribedAppNative(IntPtr self, uint appId);

    public bool IsSubscribedApp(uint appId)
    {
        return Call<bool, IsSubscribedAppNative>(NativeFunctions.IsSubscribedApp, InstanceAddress, appId);
    }
}
