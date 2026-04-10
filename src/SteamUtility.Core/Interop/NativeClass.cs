using System.Runtime.InteropServices;

namespace SteamUtility.Core.Interop;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct NativeClass
{
    public readonly IntPtr VTablePointer;
}
