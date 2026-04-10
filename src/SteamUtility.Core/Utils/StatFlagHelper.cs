namespace SteamUtility.Core.Utils;

[Flags]
public enum StatFlags
{
    None = 0,
    IncrementOnly = 1 << 0,
    Protected = 1 << 1,
    UnknownPermission = 1 << 2
}

public static class StatFlagHelper
{
    public static StatFlags GetFlags(int permission, bool incrementOnly, bool isAchievement = false)
    {
        var flags = StatFlags.None;

        if (!isAchievement && incrementOnly)
        {
            flags |= StatFlags.IncrementOnly;
        }

        if ((isAchievement && (permission & 3) != 0) || (!isAchievement && (permission & 2) != 0))
        {
            flags |= StatFlags.Protected;
        }

        if ((permission & ~(isAchievement ? 3 : 2)) != 0)
        {
            flags |= StatFlags.UnknownPermission;
        }

        return flags;
    }
}
