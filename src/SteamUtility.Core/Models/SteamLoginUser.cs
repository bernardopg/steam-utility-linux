namespace SteamUtility.Core.Models;

public sealed record SteamLoginUser(
    ulong SteamId,
    string? AccountName,
    string? PersonaName,
    bool MostRecent,
    string? Timestamp);
