using SteamUtility.Core.Interop.Wrappers;
using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamClientConnection : IDisposable
{
    public SteamClient018? SteamClient { get; private set; }

    public SteamUtils005? SteamUtils { get; private set; }

    public SteamApps008? SteamApps008 { get; private set; }

    public SteamApps001? SteamApps001 { get; private set; }

    private bool _disposed;
    private int _pipeHandle;
    private int _userHandle;

    public void Initialize(SteamInstallation installation, long applicationId = 0)
    {
        if (installation is null)
        {
            throw new SteamClientInitializeException(
                SteamClientInitializeFailure.InstallPathNotFound,
                "Steam installation could not be resolved.");
        }

        if (applicationId != 0)
        {
            Environment.SetEnvironmentVariable("SteamAppId", applicationId.ToString());
        }

        if (!LinuxSteamClientLibrary.TryLoad(installation.RootPath))
        {
            throw new SteamClientInitializeException(
                SteamClientInitializeFailure.LibraryLoadFailed,
                "Failed to load steamclient.so from the local Steam installation.");
        }

        SteamClient = LinuxSteamClientLibrary.CreateInterface<SteamClient018>("SteamClient018");
        _pipeHandle = SteamClient.CreateSteamPipe();
        if (_pipeHandle == 0)
        {
            throw new SteamClientInitializeException(
                SteamClientInitializeFailure.PipeCreationFailed,
                "Failed to create a Steam IPC pipe.");
        }

        _userHandle = SteamClient.ConnectToGlobalUser(_pipeHandle);
        if (_userHandle == 0)
        {
            throw new SteamClientInitializeException(
                SteamClientInitializeFailure.UserConnectionFailed,
                "Failed to connect to the running Steam client user.");
        }

        SteamUtils = SteamClient.GetSteamUtils005(_pipeHandle);
        if (applicationId > 0 && SteamUtils.GetAppId() != (uint)applicationId)
        {
            throw new SteamClientInitializeException(
                SteamClientInitializeFailure.ApplicationIdMismatch,
                "The connected Steam client returned a different application id.");
        }

        SteamApps008 = SteamClient.GetSteamApps008(_userHandle, _pipeHandle);
        SteamApps001 = SteamClient.GetSteamApps001(_userHandle, _pipeHandle);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (SteamClient is not null && _pipeHandle > 0)
        {
            if (_userHandle > 0)
            {
                SteamClient.ReleaseUser(_pipeHandle, _userHandle);
                _userHandle = 0;
            }

            SteamClient.ReleaseSteamPipe(_pipeHandle);
            _pipeHandle = 0;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
