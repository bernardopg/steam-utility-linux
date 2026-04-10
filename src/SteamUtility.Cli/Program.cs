using SteamUtility.Core.Abstractions;
using SteamUtility.Core.Services;

ISteamLocator locator = new LinuxSteamLocator();
var steamRoot = locator.TryGetSteamRoot();

if (args.Length > 0 && string.Equals(args[0], "detect", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine(steamRoot is null
        ? "Steam installation not found."
        : $"Steam installation found at: {steamRoot}");
    return;
}

Console.WriteLine("steam-utility-linux bootstrap");
Console.WriteLine("Usage:");
Console.WriteLine("  detect   Detect the local Steam installation path on Linux");
