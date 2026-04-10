using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace SteamUtility.Core.Interop;

internal sealed class Utf8StringHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private Utf8StringHandle()
        : base(ownsHandle: true)
    {
    }

    public IntPtr Pointer => handle;

    public static Utf8StringHandle From(string? value)
    {
        var handle = new Utf8StringHandle();

        if (!string.IsNullOrEmpty(value))
        {
            handle.SetHandle(Marshal.StringToCoTaskMemUTF8(value));
        }

        return handle;
    }

    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            Marshal.FreeCoTaskMem(handle);
            handle = IntPtr.Zero;
        }

        return true;
    }
}
