# Linux Porting Plan

## Objective
Port the current Windows-first Steam Utility architecture to a maintainable Linux-first codebase.

## Immediate principles
- Move to modern .NET (`net8.0`)
- Isolate platform-specific behavior behind interfaces
- Keep the first milestone CLI-only
- Avoid direct Win32 assumptions in core logic
- Treat Steam discovery and native loading as separate concerns

## Phase 1
1. Create a cross-platform core library
2. Implement Linux Steam path discovery
3. Stand up a Linux CLI bootstrap
4. Document Windows-specific dependencies that must be replaced

## Phase 2
1. Model Steam installation and library folders
2. Parse Linux Steam configuration and compatibility data
3. Replace registry-driven discovery with filesystem-driven discovery
4. Design a native library loading abstraction for Linux shared objects

## Phase 3
1. Decide which features remain cross-platform
2. Rebuild or drop Win32-only window/message-loop behavior
3. Introduce tests for path detection and configuration parsing
4. Reassess whether a GUI layer is needed

## Known architectural blockers from the original project
- .NET Framework 4.8 targeting
- Windows registry lookup for Steam installation
- Win32 window lifecycle calls
- Explicit `user32.dll` / `kernel32.dll` imports
- Windows-native Steam client loading assumptions

## First success criterion
`dotnet run --project src/SteamUtility.Cli -- detect`
should resolve a valid Steam root on a Linux machine with Steam installed.
