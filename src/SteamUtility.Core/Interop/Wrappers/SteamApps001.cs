using System.Runtime.InteropServices;
using SteamUtility.Core.Interop.Interfaces;

namespace SteamUtility.Core.Interop.Wrappers;

public sealed class SteamApps001 : NativeWrapper<ISteamApps001>
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int NativeGetAppData(
        IntPtr self,
        uint appId,
        IntPtr key,
        IntPtr value,
        int valueLength);

    public string? GetAppData(uint appId, string key)
    {
        using var keyHandle = Utf8StringHandle.From(key);
        const int valueLength = 1024;
        var valuePointer = Marshal.AllocHGlobal(valueLength);

        try
        {
            var result = Call<int, NativeGetAppData>(
                NativeFunctions.GetAppData,
                InstanceAddress,
                appId,
                keyHandle.Pointer,
                valuePointer,
                valueLength);

            return result == 0 ? null : Marshal.PtrToStringUTF8(valuePointer);
        }
        finally
        {
            Marshal.FreeHGlobal(valuePointer);
        }
    }
}
