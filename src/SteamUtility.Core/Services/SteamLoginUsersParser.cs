using SteamUtility.Core.Models;
using SteamUtility.Core.Vdf;

namespace SteamUtility.Core.Services;

public sealed class SteamLoginUsersParser
{
    public IReadOnlyList<SteamLoginUser> Parse(string vdfContent)
    {
        var root = SimpleVdfReader.Parse(vdfContent);
        var usersRoot = root.GetChildren("users").FirstOrDefault();

        if (usersRoot is null)
        {
            return [];
        }

        var results = new List<SteamLoginUser>();

        foreach (var entry in usersRoot.Children)
        {
            if (!ulong.TryParse(entry.Key, out var steamId))
            {
                continue;
            }

            var userObject = entry.Value.FirstOrDefault();
            if (userObject is null)
            {
                continue;
            }

            results.Add(new SteamLoginUser(
                SteamId: steamId,
                AccountName: userObject.GetSingleValue("AccountName"),
                PersonaName: userObject.GetSingleValue("PersonaName"),
                MostRecent: string.Equals(userObject.GetSingleValue("MostRecent"), "1", StringComparison.OrdinalIgnoreCase),
                Timestamp: userObject.GetSingleValue("Timestamp")));
        }

        return results
            .OrderByDescending(static user => user.MostRecent)
            .ThenByDescending(static user => user.Timestamp ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static user => user.SteamId)
            .ToArray();
    }
}
