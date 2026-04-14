using System.Runtime.InteropServices;
using System.Text;
using SteamUtility.Core.Abstractions;
using SteamUtility.Core.Interop;
using SteamUtility.Core.Interop.Interfaces;
using SteamUtility.Core.Interop.Wrappers;

namespace SteamUtility.Tests.Fakes;

internal sealed class FakeSteamClientLibraryLoader : ISteamClientLibraryLoader, IDisposable
{
    private readonly HashSet<uint> _ownedAppIds;
    private readonly IReadOnlyDictionary<uint, string?> _appNames;
    private readonly List<Delegate> _delegates = [];
    private readonly List<IntPtr> _allocations = [];
    private readonly IntPtr _steamClientInstance;
    private readonly IntPtr _steamUtilsInstance;
    private readonly IntPtr _steamApps008Instance;
    private readonly IntPtr _steamApps001Instance;

    public FakeSteamClientLibraryLoader(
        IEnumerable<uint> ownedAppIds,
        IReadOnlyDictionary<uint, string?>? appNames = null,
        uint appId = 70120)
    {
        _ownedAppIds = ownedAppIds.ToHashSet();
        _appNames = appNames ?? new Dictionary<uint, string?>();
        AppId = appId;

        _steamUtilsInstance = CreateNativeInstance(CreateSteamUtilsVTable());
        _steamApps008Instance = CreateNativeInstance(CreateSteamApps008VTable());
        _steamApps001Instance = CreateNativeInstance(CreateSteamApps001VTable());
        _steamClientInstance = CreateNativeInstance(CreateSteamClientVTable());
    }

    public int TryLoadCalls { get; private set; }

    public int CreateInterfaceCalls { get; private set; }

    public int CreateSteamPipeCalls { get; private set; }

    public int ConnectToGlobalUserCalls { get; private set; }

    public int ReleaseSteamPipeCalls { get; private set; }

    public int ReleaseUserCalls { get; private set; }

    public string? LastRootPath { get; private set; }

    public uint AppId { get; }

    public string? FindLibraryPath(string steamRoot) => null;

    public bool TryLoad(string steamRoot)
    {
        TryLoadCalls++;
        LastRootPath = steamRoot;
        return true;
    }

    public TInterface CreateInterface<TInterface>(string versionString)
        where TInterface : INativeWrapper, new()
    {
        CreateInterfaceCalls++;

        if (typeof(TInterface) != typeof(SteamClient018))
        {
            throw new NotSupportedException($"Unsupported interface type '{typeof(TInterface).Name}'.");
        }

        var wrapper = new SteamClient018();
        wrapper.Initialize(_steamClientInstance);
        return (TInterface)(object)wrapper;
    }

    public void Dispose()
    {
        foreach (var allocation in _allocations)
        {
            Marshal.FreeHGlobal(allocation);
        }
    }

    private IntPtr CreateNativeInstance<TNativeFunctions>(TNativeFunctions vtable)
        where TNativeFunctions : struct
    {
        var vtablePtr = Marshal.AllocHGlobal(Marshal.SizeOf<TNativeFunctions>());
        Marshal.StructureToPtr(vtable, vtablePtr, false);
        _allocations.Add(vtablePtr);

        var instancePtr = Marshal.AllocHGlobal(IntPtr.Size);
        Marshal.WriteIntPtr(instancePtr, vtablePtr);
        _allocations.Add(instancePtr);
        return instancePtr;
    }

    private ISteamClient018 CreateSteamClientVTable()
    {
        return new ISteamClient018
        {
            CreateSteamPipe = FunctionPointer(new CreateSteamPipeDelegate(CreateSteamPipe)),
            ReleaseSteamPipe = FunctionPointer(new ReleaseSteamPipeDelegate(ReleaseSteamPipe)),
            ConnectToGlobalUser = FunctionPointer(new ConnectToGlobalUserDelegate(ConnectToGlobalUser)),
            ReleaseUser = FunctionPointer(new ReleaseUserDelegate(ReleaseUser)),
            GetISteamUtils = FunctionPointer(new GetISteamUtilsDelegate(GetISteamUtils)),
            GetISteamApps = FunctionPointer(new GetISteamAppsDelegate(GetISteamApps))
        };
    }

    private ISteamUtils005 CreateSteamUtilsVTable()
    {
        return new ISteamUtils005
        {
            GetAppID = FunctionPointer(new GetAppIdDelegate(GetAppId))
        };
    }

    private ISteamApps008 CreateSteamApps008VTable()
    {
        return new ISteamApps008
        {
            IsSubscribedApp = FunctionPointer(new IsSubscribedAppDelegate(IsSubscribedApp))
        };
    }

    private ISteamApps001 CreateSteamApps001VTable()
    {
        return new ISteamApps001
        {
            GetAppData = FunctionPointer(new GetAppDataDelegate(GetAppData))
        };
    }

    private IntPtr FunctionPointer(Delegate @delegate)
    {
        _delegates.Add(@delegate);
        return Marshal.GetFunctionPointerForDelegate(@delegate);
    }

    private int CreateSteamPipe(IntPtr self)
    {
        CreateSteamPipeCalls++;
        return 1;
    }

    private bool ReleaseSteamPipe(IntPtr self, int pipeHandle)
    {
        ReleaseSteamPipeCalls++;
        return true;
    }

    private int ConnectToGlobalUser(IntPtr self, int pipeHandle)
    {
        ConnectToGlobalUserCalls++;
        return 2;
    }

    private void ReleaseUser(IntPtr self, int pipeHandle, int userHandle)
    {
        ReleaseUserCalls++;
    }

    private IntPtr GetISteamUtils(IntPtr self, int pipeHandle, IntPtr versionPointer)
    {
        var version = Marshal.PtrToStringUTF8(versionPointer) ?? string.Empty;
        if (version == "SteamUtils005")
        {
            return _steamUtilsInstance;
        }

        throw new NotSupportedException($"Unsupported Steam utils version '{version}'.");
    }

    private IntPtr GetISteamApps(IntPtr self, int userHandle, int pipeHandle, IntPtr versionPointer)
    {
        var version = Marshal.PtrToStringUTF8(versionPointer) ?? string.Empty;
        return version switch
        {
            "STEAMAPPS_INTERFACE_VERSION008" => _steamApps008Instance,
            "STEAMAPPS_INTERFACE_VERSION001" => _steamApps001Instance,
            _ => throw new NotSupportedException($"Unsupported Steam apps version '{version}'.")
        };
    }

    private uint GetAppId(IntPtr self)
    {
        return AppId;
    }

    private bool IsSubscribedApp(IntPtr self, uint appId)
    {
        return _ownedAppIds.Contains(appId);
    }

    private int GetAppData(IntPtr self, uint appId, IntPtr keyPointer, IntPtr valuePointer, int valueLength)
    {
        var key = Marshal.PtrToStringUTF8(keyPointer) ?? string.Empty;
        if (!key.Equals("name", StringComparison.Ordinal))
        {
            return 0;
        }

        if (!_appNames.TryGetValue(appId, out var name) || string.IsNullOrWhiteSpace(name))
        {
            return 0;
        }

        var bytes = Encoding.UTF8.GetBytes(name);
        if (bytes.Length + 1 > valueLength)
        {
            return 0;
        }

        Marshal.Copy(bytes, 0, valuePointer, bytes.Length);
        Marshal.WriteByte(valuePointer, bytes.Length, 0);
        return 1;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int CreateSteamPipeDelegate(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool ReleaseSteamPipeDelegate(IntPtr self, int pipeHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int ConnectToGlobalUserDelegate(IntPtr self, int pipeHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ReleaseUserDelegate(IntPtr self, int pipeHandle, int userHandle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetISteamUtilsDelegate(IntPtr self, int pipeHandle, IntPtr versionPointer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetISteamAppsDelegate(IntPtr self, int userHandle, int pipeHandle, IntPtr versionPointer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint GetAppIdDelegate(IntPtr self);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private delegate bool IsSubscribedAppDelegate(IntPtr self, uint appId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetAppDataDelegate(IntPtr self, uint appId, IntPtr keyPointer, IntPtr valuePointer, int valueLength);
}
