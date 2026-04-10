using SteamUtility.Core.Models;
using SteamUtility.Core.Utils;

namespace SteamUtility.Core.Services;

public sealed class StatsSchemaLoader
{
    public bool LoadUserGameStatsSchema(
        SteamInstallation installation,
        uint appId,
        out List<AchievementData> achievementDefinitions,
        out List<StatData> statDefinitions)
    {
        achievementDefinitions = [];
        statDefinitions = [];

        var schemaPath = Path.Combine(installation.RootPath, "appcache", "stats", $"UserGameStatsSchema_{appId}.bin");
        if (!File.Exists(schemaPath))
        {
            return false;
        }

        var keyValue = KeyValue.LoadAsBinary(schemaPath);
        if (keyValue is null)
        {
            return false;
        }

        var stats = keyValue[appId.ToString()]["stats"];
        if (!stats.Valid || stats.Children is null)
        {
            return false;
        }

        foreach (var stat in stats.Children.Where(stat => stat.Valid))
        {
            var rawType = ResolveStatType(stat);

            switch (rawType)
            {
                case 1:
                    LoadIntegerStat(statDefinitions, stat);
                    break;
                case 2:
                case 3:
                    LoadFloatingPointStat(statDefinitions, stat, rawType == 3);
                    break;
                case 4:
                case 5:
                    LoadAchievements(achievementDefinitions, stat);
                    break;
            }
        }

        return true;
    }

    private static int ResolveStatType(KeyValue stat)
    {
        if (stat["type_int"].Valid)
        {
            return stat["type_int"].AsInteger(0);
        }

        if (stat["type"].Valid)
        {
            if (stat["type"].Type == KeyValueType.String)
            {
                var typeString = stat["type"].AsString(string.Empty);
                if (typeString.Equals("INT", StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }

                if (typeString.Equals("FLOAT", StringComparison.OrdinalIgnoreCase))
                {
                    return 2;
                }

                if (typeString.Equals("AVGRATE", StringComparison.OrdinalIgnoreCase))
                {
                    return 3;
                }

                if (typeString.Equals("ACHIEVEMENT", StringComparison.OrdinalIgnoreCase) ||
                    typeString.Equals("ACHIEVEMENTS", StringComparison.OrdinalIgnoreCase))
                {
                    return 4;
                }
            }

            return stat["type"].AsInteger(0);
        }

        var hasBits = stat.Children?.Any(child =>
            string.Equals(child.Name, "bits", StringComparison.InvariantCultureIgnoreCase)) == true;

        if (hasBits)
        {
            return 4;
        }

        var hasFloatValue = (stat["min"].Valid && stat["min"].Type == KeyValueType.Float32)
            || (stat["max"].Valid && stat["max"].Type == KeyValueType.Float32)
            || (stat["default"].Valid && stat["default"].Type == KeyValueType.Float32);

        if (stat["window"].Valid)
        {
            return 3;
        }

        return hasFloatValue ? 2 : 1;
    }

    private static void LoadIntegerStat(ICollection<StatData> statDefinitions, KeyValue stat)
    {
        var session = SteamworksSession.Current ?? throw new InvalidOperationException("An active Steamworks session is required.");
        var statId = stat["name"].AsString(string.Empty);
        var statValue = 0;
        var success = session.GetStat(statId, out statValue);

        statDefinitions.Add(new StatData
        {
            Id = statId,
            Name = stat["display"]["name"].AsString(statId),
            Type = "integer",
            MinValue = stat["min"].AsInteger(int.MinValue),
            MaxValue = stat["max"].AsInteger(int.MaxValue),
            DefaultValue = stat["default"].AsInteger(0),
            Value = success ? statValue : 0,
            IncrementOnly = stat["incrementonly"].AsBoolean(false),
            Permission = stat["permission"].AsInteger(0)
        });
    }

    private static void LoadFloatingPointStat(ICollection<StatData> statDefinitions, KeyValue stat, bool averageRate)
    {
        var session = SteamworksSession.Current ?? throw new InvalidOperationException("An active Steamworks session is required.");
        var statId = stat["name"].AsString(string.Empty);
        var statValue = 0f;
        var success = session.GetStat(statId, out statValue);

        statDefinitions.Add(new StatData
        {
            Id = statId,
            Name = stat["display"]["name"].AsString(statId),
            Type = averageRate ? "avgrate" : "float",
            MinValue = stat["min"].AsFloat(float.MinValue),
            MaxValue = stat["max"].AsFloat(float.MaxValue),
            DefaultValue = stat["default"].AsFloat(0f),
            Value = success ? statValue : 0f,
            IncrementOnly = stat["incrementonly"].AsBoolean(false),
            Permission = stat["permission"].AsInteger(0)
        });
    }

    private static void LoadAchievements(ICollection<AchievementData> achievementDefinitions, KeyValue stat)
    {
        var session = SteamworksSession.Current ?? throw new InvalidOperationException("An active Steamworks session is required.");
        if (stat.Children is null)
        {
            return;
        }

        foreach (var bits in stat.Children.Where(child =>
                     string.Equals(child.Name, "bits", StringComparison.InvariantCultureIgnoreCase)))
        {
            if (!bits.Valid || bits.Children is null)
            {
                continue;
            }

            foreach (var bit in bits.Children)
            {
                var achievementId = bit["name"].AsString(string.Empty);
                var name = session.GetAchievementDisplayAttribute(achievementId, "name");
                var description = session.GetAchievementDisplayAttribute(achievementId, "desc");

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = bit["display"]["name"].AsString(string.Empty);
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    description = bit["display"]["desc"].AsString(string.Empty);
                }

                session.GetAchievement(achievementId, out var achieved);
                session.GetAchievementAchievedPercent(achievementId, out var percent);

                achievementDefinitions.Add(new AchievementData
                {
                    Id = achievementId,
                    Name = name,
                    Description = description,
                    IconNormal = bit["display"]["icon"].AsString(string.Empty),
                    IconLocked = bit["display"]["icon_gray"].AsString(string.Empty),
                    IsHidden = bit["display"]["hidden"].AsBoolean(false),
                    Permission = bit["permission"].AsInteger(0),
                    Achieved = achieved,
                    Percent = percent
                });
            }
        }
    }
}
