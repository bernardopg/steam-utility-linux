using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public sealed class SteamOwnershipService
{
    private readonly Func<SteamClientConnection> _connectionFactory;

    public SteamOwnershipService()
        : this(static () => new SteamClientConnection())
    {
    }

    internal SteamOwnershipService(Func<SteamClientConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public IReadOnlyList<SteamOwnedApp> GetOwnedApps(SteamInstallation installation, IEnumerable<uint> appIds)
    {
        ArgumentNullException.ThrowIfNull(installation);
        ArgumentNullException.ThrowIfNull(appIds);

        var distinctAppIds = appIds.Distinct().ToArray();
        var ownedApps = new List<SteamOwnedApp>(distinctAppIds.Length);

        using var connection = _connectionFactory();
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
