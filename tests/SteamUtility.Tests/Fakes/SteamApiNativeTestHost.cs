using System.Reflection;
using System.Runtime.InteropServices;
using SteamUtility.Core.Services;

namespace SteamUtility.Tests.Fakes;

internal static class SteamApiNativeTestHost
{
    private static readonly FieldInfo LibraryHandleField = typeof(SteamApiNative)
        .GetField("_libraryHandle", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("SteamApiNative library handle field was not found.");

    private static readonly FieldInfo[] DelegateFields = typeof(SteamApiNative)
        .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
        .Where(field => typeof(Delegate).IsAssignableFrom(field.FieldType))
        .ToArray();

    public static void Reset()
    {
        var handle = (IntPtr)LibraryHandleField.GetValue(null)!;
        if (handle != IntPtr.Zero)
        {
            NativeLibrary.Free(handle);
        }

        LibraryHandleField.SetValue(null, IntPtr.Zero);

        foreach (var field in DelegateFields)
        {
            field.SetValue(null, null);
        }
    }
}
