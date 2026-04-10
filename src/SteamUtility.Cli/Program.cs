using SteamUtility.Core.Abstractions;
using SteamUtility.Core.Models;
using SteamUtility.Core.Services;

ISteamLocator locator = new LinuxSteamLocator();
var installationService = new SteamInstallationService(locator);
var installation = installationService.TryResolve();

if (args.Length == 0)
{
    PrintUsage();
    return;
}

switch (args[0].ToLowerInvariant())
{
    case "detect":
        Console.WriteLine(installation is null
            ? "Steam installation not found."
            : $"Steam installation found at: {installation.RootPath}");
        return;

    case "libraries":
        PrintLibraries(installation);
        return;

    case "apps":
        PrintApps(installation);
        return;

    default:
        PrintUsage();
        return;
}

static void PrintLibraries(SteamInstallation? installation)
{
    if (installation is null)
    {
        Console.WriteLine("Steam installation not found.");
        return;
    }

    Console.WriteLine($"Steam root: {installation.RootPath}");
    Console.WriteLine("Library folders:");

    foreach (var library in installation.LibraryFolders)
    {
        var marker = library.IsDefault ? "*" : "-";
        Console.WriteLine($"  {marker} [{library.Key}] {library.Path}");
    }
}

static void PrintApps(SteamInstallation? installation)
{
    if (installation is null)
    {
        Console.WriteLine("Steam installation not found.");
        return;
    }

    var scanner = new SteamLibraryScanner();
    var apps = scanner.ScanInstalledApps(installation);

    if (apps.Count == 0)
    {
        Console.WriteLine("No installed Steam app manifests were found.");
        return;
    }

    Console.WriteLine($"Detected {apps.Count} installed Steam app(s):");

    foreach (var app in apps)
    {
        Console.WriteLine($"  - {app.AppId}: {app.Name} [{app.InstallDirectory}]");
    }
}

static void PrintUsage()
{
    Console.WriteLine("steam-utility-linux bootstrap");
    Console.WriteLine("Usage:");
    Console.WriteLine("  detect      Detect the local Steam installation path on Linux");
    Console.WriteLine("  libraries   List discovered Steam library folders");
    Console.WriteLine("  apps        List installed Steam apps from appmanifest files");
}
