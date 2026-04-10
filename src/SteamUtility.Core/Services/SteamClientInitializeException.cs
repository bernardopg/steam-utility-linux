namespace SteamUtility.Core.Services;

public sealed class SteamClientInitializeException(SteamClientInitializeFailure failureReason, string message)
    : Exception(message)
{
    public SteamClientInitializeFailure FailureReason { get; } = failureReason;
}
