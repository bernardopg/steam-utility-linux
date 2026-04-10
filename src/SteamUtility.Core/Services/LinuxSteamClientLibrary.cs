using System.Runtime.InteropServices;
using SteamUtility.Core.Interop;

namespace SteamUtility.Core.Services;

public static class LinuxSteamClientLibrary
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private delegate IntPtr CreateInterfaceDelegate(string versionString, IntPtr returnCode);

    private static readonly string[] CandidateRelativePaths =
    [
        Path.Combine("linux64", "steamclient.so"),
        Path.Combine("ubuntu12_64", "steamclient.so"),
        Path.Combine("linux32", "steamclient.so"),
        Path.Combine("ubuntu12_32", "steamclient.so"),
        "steamclient.so"
    ];

    private static nint _libraryHandle;
    private static CreateInterfaceDelegate? _createInterface;

    public static string? FindLibraryPath(string steamRoot)
    {
        return CandidateRelativePaths
            .Select(relativePath => Path.Combine(steamRoot, relativePath))
            .FirstOrDefault(File.Exists);
    }

    public static TInterface CreateInterface<TInterface>(string versionString)
        where TInterface : INativeWrapper, new()
    {
        if (_createInterface is null)
        {
            throw new InvalidOperationException("Steam client library has not been loaded.");
        }

        var interfaceAddress = _createInterface(versionString, IntPtr.Zero);
        if (interfaceAddress == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to create native Steam interface '{versionString}'.");
        }

        var wrapper = new TInterface();
        wrapper.Initialize(interfaceAddress);
        return wrapper;
    }

    public static bool TryLoad(string steamRoot)
    {
        if (_libraryHandle != IntPtr.Zero && _createInterface is not null)
        {
            return true;
        }

        var libraryPath = FindLibraryPath(steamRoot);
        if (string.IsNullOrWhiteSpace(libraryPath))
        {
            return false;
        }

        if (!NativeLibrary.TryLoad(libraryPath, out _libraryHandle))
        {
            _libraryHandle = IntPtr.Zero;
            return false;
        }

        if (!NativeLibrary.TryGetExport(_libraryHandle, "CreateInterface", out var export))
        {
            NativeLibrary.Free(_libraryHandle);
            _libraryHandle = IntPtr.Zero;
            return false;
        }

        _createInterface = Marshal.GetDelegateForFunctionPointer<CreateInterfaceDelegate>(export);
        return true;
    }
}
