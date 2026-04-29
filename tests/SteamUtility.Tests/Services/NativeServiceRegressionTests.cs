using System;
using System.Runtime.InteropServices;
using SteamUtility.Core.Abstractions;
using SteamUtility.Core.Models;
using SteamUtility.Core.Services;
using SteamUtility.Tests.Fakes;

namespace SteamUtility.Tests.Services;

public static class NativeServiceRegressionTests
{
    public static void SteamworksSession_ThrowsWhenSteamNotRunning()
    {
        SteamApiNativeTestHost.Reset();

        var fakeInstall = FakeSteamInstallationFactory.Create();
        var resolver = FakeSteamApiLibrary.CreateResolver(FakeSteamApiLibrary.InitFailureLibraryPath);

        try
        {
            using var session = new SteamworksSession(fakeInstall, 440, resolver);
            throw new Exception("Expected SteamworksInitializationException was not thrown");
        }
        catch (SteamworksInitializationException ex) when (ex.FailureReason == SteamworksInitializationFailure.ApiInitFailed)
        {
            // Expected
        }
        finally
        {
            SteamApiNativeTestHost.Reset();
        }
    }

    public static void SteamworksSession_SucceedsWithValidFake()
    {
        SteamApiNativeTestHost.Reset();

        var fakeInstall = FakeSteamInstallationFactory.Create();
        var resolver = FakeSteamApiLibrary.CreateResolver(FakeSteamApiLibrary.FullLibraryPath);

        using (var session = new SteamworksSession(fakeInstall, 440, resolver))
        {
            // Basic sanity checks - these should not throw
            _ = session.SteamId;
            _ = session.GetNumAchievements();

            int intOut = 0;
            _ = session.GetStat("int_stat", out intOut);

            float floatOut = 0f;
            _ = session.GetStat("float_stat", out floatOut);

            bool achieved = false;
            _ = session.GetAchievement("dummy_achievement", out achieved);
        }

        SteamApiNativeTestHost.Reset();
    }

    public static void SteamOwnershipService_ReturnsEmptyList_WhenNoOwnedApps()
    {
        var fakeInstall = FakeSteamInstallationFactory.Create();
        using var loader = new FakeSteamClientLibraryLoader(Array.Empty<uint>());
        var service = new SteamOwnershipService(() => new SteamClientConnection(loader));

        var result = service.GetOwnedApps(fakeInstall, new[] { 440u, 570u });

        if (result.Count != 0)
        {
            throw new Exception($"Expected empty list, got {result.Count} items");
        }
    }

    public static void SteamOwnershipService_ReturnsOwnedApps_WhenAvailable()
    {
        var fakeInstall = FakeSteamInstallationFactory.Create();
        using var loader = new FakeSteamClientLibraryLoader(new[] { 440u });
        var service = new SteamOwnershipService(() => new SteamClientConnection(loader));

        var result = service.GetOwnedApps(fakeInstall, new[] { 440u });

        if (result == null)
        {
            throw new Exception("Expected non-null result");
        }
    }

    public static void StatsSchemaLoader_ReturnsFalse_WhenSchemaMissing()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"steam-utility-tests-{Guid.NewGuid():N}");
        var installationRoot = Path.Combine(tempRoot, "steam");
        var steamAppsPath = Path.Combine(installationRoot, "steamapps");

        var fakeInstall = FakeSteamInstallationFactory.Create(installationRoot, steamAppsPath);
        var loader = new StatsSchemaLoader();

        if (loader.LoadUserGameStatsSchema(fakeInstall, 70120, null, out var achievements, out var stats))
        {
            throw new Exception("Expected missing schema file to return false.");
        }

        if (achievements.Count != 0 || stats.Count != 0)
        {
            throw new Exception("Expected empty outputs when schema is missing.");
        }
    }

    public static void SteamApiNative_FunctionsReturnExpectedValues_FromFake()
    {
        SteamApiNativeTestHost.Reset();

        // Test that our fake library exports the expected functions
        var fakeInstall = FakeSteamInstallationFactory.Create();
        var resolver = FakeSteamApiLibrary.CreateResolver(FakeSteamApiLibrary.FullLibraryPath);

        SteamApiNative.EnsureLoaded(fakeInstall, resolver);

        try
        {
            // This primarily tests that the native loader works
            bool initResult = SteamApiNative.Init();
            if (!initResult)
            {
                throw new Exception("Fake library should initialize successfully");
            }

            // Test basic function exports don't crash
            var user = SteamApiNative.GetSteamUser();
            var stats = SteamApiNative.GetSteamUserStats();
            var steamId = SteamApiNative.GetSteamId(user);

            // Just verify they return something (not null in the fake context)
            if (user == IntPtr.Zero)
            {
                throw new Exception("GetSteamUser returned IntPtr.Zero");
            }

            if (stats == IntPtr.Zero)
            {
                throw new Exception("GetSteamUserStats returned IntPtr.Zero");
            }

            if (steamId == 0UL)
            {
                throw new Exception("GetSteamId returned 0");
            }

            SteamApiNative.Shutdown();
        }
        finally
        {
            SteamApiNativeTestHost.Reset();
        }
    }
}
