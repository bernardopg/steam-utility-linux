namespace SteamUtility.Core.Services;

public enum SteamClientInitializeFailure : byte
{
    None = 0,
    InstallPathNotFound,
    LibraryLoadFailed,
    ClientCreationFailed,
    PipeCreationFailed,
    UserConnectionFailed,
    ApplicationIdMismatch
}
