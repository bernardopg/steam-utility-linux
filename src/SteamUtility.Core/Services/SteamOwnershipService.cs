using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamOwnershipService
{
    public IReadOnlyList<SteamOwnedApp> GetOwnedApps(SteamInstallation installation, IEnumerable<uint> appIds)
    {
        ArgumentNullException.ThrowIfNull(installation);
        ArgumentNullException.ThrowIfNull(appIds);

        var distinctAppIds = appIds.Distinct().ToArray();
        var ownedApps = new List<SteamOwnedApp>(distinctAppIds.Length);

        using var connection = new SteamClientConnection();
        connection.Initialize(installation);

        foreach (var appId in distinctAppIds)
        {
            try
            {
                if (connection.SteamApps008?.IsSubscribedApp(appId) != true)
                {
                    continue;
                }

                var name = connection.SteamApps001?.GetAppData(appId, "name");
                ownedApps.Add(new SteamOwnedApp(appId, string.IsNullOrWhiteSpace(name) ? $"App {appId}" : name));
            }
            catch
            {
                // Keep the scan resilient when individual app queries fail.
            }
        }

        return ownedApps;
    }
}
