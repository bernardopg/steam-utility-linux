using System.Text;
using SteamUtility.Core.Services;
using SteamUtility.Tests.Fakes;

namespace SteamUtility.Tests.Native;

public static class StatsSchemaLoaderTests
{
    public static void LoadUserGameStatsSchema_ParsesStatsAndAchievements()
    {
        SteamApiNativeTestHost.Reset();

        var originalDirectory = Directory.GetCurrentDirectory();
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var currentDirectory = Path.Combine(tempRoot, "cwd");
        var installationRoot = Path.Combine(tempRoot, "steam");
        var steamAppsPath = Path.Combine(installationRoot, "steamapps");
        var schemaPath = Path.Combine(installationRoot, "appcache", "stats", "UserGameStatsSchema_70120.bin");

        Directory.CreateDirectory(currentDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(schemaPath)!);
        WriteSchemaFile(schemaPath);

        var installation = FakeSteamInstallationFactory.Create(installationRoot, steamAppsPath);
        var resolver = FakeSteamApiLibrary.CreateResolver(FakeSteamApiLibrary.FullLibraryPath);

        try
        {
            Directory.SetCurrentDirectory(currentDirectory);

            using (var session = new SteamworksSession(installation, 70120, resolver))
            {
                if (!session.SetStat("hedGamesPlayed", 42))
                {
                    throw new Exception("Expected integer stat mutation to succeed.");
                }

                if (!session.SetStat("hedAccuracy", 88.5f))
                {
                    throw new Exception("Expected float stat mutation to succeed.");
                }

                if (!session.SetStat("hedAverageRate", 3.25f))
                {
                    throw new Exception("Expected average-rate stat mutation to succeed.");
                }

                if (!session.SetAchievement("ACH_TUTORIAL_COMPLETED"))
                {
                    throw new Exception("Expected achievement mutation to succeed.");
                }

                if (!session.StoreStats())
                {
                    throw new Exception("Expected StoreStats to succeed.");
                }

                var loader = new StatsSchemaLoader();
                if (!loader.LoadUserGameStatsSchema(installation, 70120, session, out var achievements, out var stats))
                {
                    throw new Exception("Expected stats schema loader to succeed.");
                }

                if (achievements.Count != 1)
                {
                    throw new Exception("Expected one achievement definition.");
                }

                if (stats.Count != 3)
                {
                    throw new Exception("Expected three stat definitions.");
                }

                var achievement = achievements[0];
                if (achievement.Id != "ACH_TUTORIAL_COMPLETED") throw new Exception("Unexpected achievement id.");
                if (achievement.Name != "n00b") throw new Exception("Unexpected achievement name.");
                if (achievement.Description != "Tutorial level completed") throw new Exception("Unexpected achievement description.");
                if (achievement.IconNormal != "achievement-icon.png") throw new Exception("Unexpected achievement normal icon.");
                if (achievement.IconLocked != "achievement-icon-locked.png") throw new Exception("Unexpected achievement locked icon.");
                if (!achievement.Achieved) throw new Exception("Expected achievement to be marked as achieved.");
                if (!Approximately(achievement.Percent, 28.2f)) throw new Exception("Unexpected achievement percentage.");
                if (achievement.Permission != 0) throw new Exception("Unexpected achievement permission.");
                if (achievement.IsHidden) throw new Exception("Expected achievement to be visible.");

                var gamesPlayed = stats.Single(stat => stat.Id == "hedGamesPlayed");
                if (gamesPlayed.Type != "integer") throw new Exception("Unexpected integer stat type.");
                if (gamesPlayed.Name != "Games played") throw new Exception("Unexpected integer stat name.");
                if (gamesPlayed.IncrementOnly != true) throw new Exception("Expected integer stat to be increment-only.");
                if (gamesPlayed.Permission != 0) throw new Exception("Unexpected integer stat permission.");
                if (gamesPlayed.Value is not int gamesPlayedValue || gamesPlayedValue != 42) throw new Exception("Unexpected integer stat value.");
                if (gamesPlayed.DefaultValue is not int gamesPlayedDefault || gamesPlayedDefault != 7) throw new Exception("Unexpected integer stat default.");
                if (gamesPlayed.MinValue is not int gamesPlayedMin || gamesPlayedMin != 0) throw new Exception("Unexpected integer stat min.");
                if (gamesPlayed.MaxValue is not int gamesPlayedMax || gamesPlayedMax != 1000) throw new Exception("Unexpected integer stat max.");

                var accuracy = stats.Single(stat => stat.Id == "hedAccuracy");
                if (accuracy.Type != "float") throw new Exception("Unexpected float stat type.");
                if (accuracy.Name != "Accuracy") throw new Exception("Unexpected float stat name.");
                if (accuracy.IncrementOnly) throw new Exception("Expected float stat to allow direct updates.");
                if (accuracy.Permission != 0) throw new Exception("Unexpected float stat permission.");
                if (accuracy.Value is not float accuracyValue || !Approximately(accuracyValue, 88.5f)) throw new Exception("Unexpected float stat value.");
                if (accuracy.DefaultValue is not float accuracyDefault || !Approximately(accuracyDefault, 19.5f)) throw new Exception("Unexpected float stat default.");
                if (accuracy.MinValue is not float accuracyMin || !Approximately(accuracyMin, 0f)) throw new Exception("Unexpected float stat min.");
                if (accuracy.MaxValue is not float accuracyMax || !Approximately(accuracyMax, 100f)) throw new Exception("Unexpected float stat max.");

                var averageRate = stats.Single(stat => stat.Id == "hedAverageRate");
                if (averageRate.Type != "avgrate") throw new Exception("Unexpected average-rate stat type.");
                if (averageRate.Name != "Average Rate") throw new Exception("Unexpected average-rate stat name.");
                if (averageRate.IncrementOnly) throw new Exception("Expected average-rate stat to allow direct updates.");
                if (averageRate.Permission != 0) throw new Exception("Unexpected average-rate stat permission.");
                if (averageRate.Value is not float averageRateValue || !Approximately(averageRateValue, 3.25f)) throw new Exception("Unexpected average-rate stat value.");
                if (averageRate.DefaultValue is not float averageRateDefault || !Approximately(averageRateDefault, 2.25f)) throw new Exception("Unexpected average-rate stat default.");
                if (averageRate.MinValue is not float averageRateMin || !Approximately(averageRateMin, 0f)) throw new Exception("Unexpected average-rate stat min.");
                if (averageRate.MaxValue is not float averageRateMax || !Approximately(averageRateMax, 100f)) throw new Exception("Unexpected average-rate stat max.");
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            SteamApiNativeTestHost.Reset();

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    internal static void CreateSchemaFile(string path) => WriteSchemaFile(path);

    private static void WriteSchemaFile(string path)
    {
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        WriteContainer(writer, "70120", () =>
        {
            WriteContainer(writer, "stats", () =>
            {
                WriteContainer(writer, "hedGamesPlayed", () =>
                {
                    WriteString(writer, "name", "hedGamesPlayed");
                    WriteContainer(writer, "display", () => WriteString(writer, "name", "Games played"));
                    WriteInt32(writer, "type_int", 1);
                    WriteInt32(writer, "min", 0);
                    WriteInt32(writer, "max", 1000);
                    WriteInt32(writer, "default", 7);
                    WriteInt32(writer, "incrementonly", 1);
                    WriteInt32(writer, "permission", 0);
                });

                WriteContainer(writer, "hedAccuracy", () =>
                {
                    WriteString(writer, "name", "hedAccuracy");
                    WriteContainer(writer, "display", () => WriteString(writer, "name", "Accuracy"));
                    WriteInt32(writer, "type_int", 2);
                    WriteFloat(writer, "min", 0f);
                    WriteFloat(writer, "max", 100f);
                    WriteFloat(writer, "default", 19.5f);
                    WriteInt32(writer, "incrementonly", 0);
                    WriteInt32(writer, "permission", 0);
                });

                WriteContainer(writer, "hedAverageRate", () =>
                {
                    WriteString(writer, "name", "hedAverageRate");
                    WriteContainer(writer, "display", () => WriteString(writer, "name", "Average Rate"));
                    WriteInt32(writer, "type_int", 3);
                    WriteFloat(writer, "min", 0f);
                    WriteFloat(writer, "max", 100f);
                    WriteFloat(writer, "default", 2.25f);
                    WriteInt32(writer, "incrementonly", 0);
                    WriteInt32(writer, "permission", 0);
                });

                WriteContainer(writer, "achievements", () =>
                {
                    WriteInt32(writer, "type_int", 4);
                    WriteContainer(writer, "bits", () =>
                    {
                        WriteContainer(writer, "0", () =>
                        {
                            WriteString(writer, "name", "ACH_TUTORIAL_COMPLETED");
                            WriteContainer(writer, "display", () =>
                            {
                                WriteString(writer, "name", "n00b");
                                WriteString(writer, "desc", "Tutorial level completed");
                                WriteString(writer, "icon", "achievement-icon.png");
                                WriteString(writer, "icon_gray", "achievement-icon-locked.png");
                                WriteInt32(writer, "hidden", 0);
                            });
                            WriteInt32(writer, "permission", 0);
                        });
                    });
                });
            });
        });
    }

    private static void WriteContainer(BinaryWriter writer, string name, Action writeChildren)
    {
        writer.Write((byte)0);
        WriteUtf8String(writer, name);
        writeChildren();
        writer.Write((byte)8);
    }

    private static void WriteString(BinaryWriter writer, string name, string value)
    {
        writer.Write((byte)1);
        WriteUtf8String(writer, name);
        WriteUtf8String(writer, value);
    }

    private static void WriteInt32(BinaryWriter writer, string name, int value)
    {
        writer.Write((byte)2);
        WriteUtf8String(writer, name);
        writer.Write(value);
    }

    private static void WriteFloat(BinaryWriter writer, string name, float value)
    {
        writer.Write((byte)3);
        WriteUtf8String(writer, name);
        writer.Write(value);
    }

    private static void WriteUtf8String(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes);
        writer.Write((byte)0);
    }

    private static bool Approximately(float actual, float expected, float epsilon = 0.01f)
        => Math.Abs(actual - expected) <= epsilon;
}
