using System.Runtime.InteropServices;
using SteamUtility.Core.Models;

namespace SteamUtility.Core.Services;

public static class LinuxSteamApiLibraryResolver
{
    public static string? FindLibraryPath(SteamInstallation installation)
    {
        var envOverride = Environment.GetEnvironmentVariable("STEAMWORKS_NATIVE_LIBRARY");
        if (!string.IsNullOrWhiteSpace(envOverride) && File.Exists(envOverride) && MatchesCurrentProcessArchitecture(envOverride))
        {
            return envOverride;
        }

        var gameDirectories = installation.LibraryFolders
            .Select(library => Path.Combine(library.Path, "steamapps", "common"))
            .Where(Directory.Exists)
            .SelectMany(commonPath => Directory.EnumerateDirectories(commonPath))
            .ToArray();

        foreach (var relativePath in CandidateRelativePaths)
        {
            foreach (var gameDirectory in gameDirectories)
            {
                var candidate = Path.Combine(gameDirectory, relativePath);
                if (File.Exists(candidate) && MatchesCurrentProcessArchitecture(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static readonly string[] CandidateRelativePaths =
    [
        Path.Combine("bin", "linux64", "libsteam_api.so"),
        Path.Combine("linux64", "libsteam_api.so"),
        Path.Combine("bin", "libsteam_api.so"),
        "libsteam_api.so"
    ];

    private static bool MatchesCurrentProcessArchitecture(string libraryPath)
    {
        try
        {
            using var stream = File.OpenRead(libraryPath);
            Span<byte> header = stackalloc byte[5];
            if (stream.Read(header) != header.Length)
            {
                return false;
            }

            var elfClass = header[4];
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => elfClass == 2,
                Architecture.X86 => elfClass == 1,
                _ => true
            };
        }
        catch
        {
            return false;
        }
    }
}
