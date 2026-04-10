using System.Runtime.InteropServices;

namespace SteamUtility.Core.Interop.Interfaces;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ISteamApps001
{
    public IntPtr GetAppData;
}
