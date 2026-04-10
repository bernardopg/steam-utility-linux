# steam-utility-linux

Linux-first port scaffold for the original Windows-oriented `steam-utility` project.
Base project: https://github.com/zevnda/steam-utility

## Current status
This repository is already past the bootstrap stage.

Implemented today:
- .NET 8 solution and project structure
- Cross-platform core library + Linux CLI entrypoint
- Steam root discovery on common Linux paths
- Minimal Valve VDF parser
- `libraryfolders.vdf` parsing
- `appmanifest_*.acf` parsing
- Installed app discovery
- `compatdata` discovery
- Bundled/custom compatibility tool discovery
- `config/config.vdf` parsing for compatibility tool assignments
- Per-app compatibility report generation
- Linux `steamclient.so` loading scaffold
- Initial ownership lookup through the running Steam client
- Native Linux `libsteam_api.so` bridge for achievement/stat commands
- CLI filtering and JSON output support
- Initial test project for parsers/reporting

Still missing for parity with the original Windows project:
- Runtime validation of the state-changing achievement/stat commands
- Replacement strategy for Win32-only hidden-window behavior
- Broader test coverage
- Packaging/release workflow

## Repository structure
- `src/SteamUtility.Core` — core domain and Linux discovery logic
- `src/SteamUtility.Cli` — current executable entrypoint
- `tests/SteamUtility.Tests` — initial unit tests
- `docs/` — architecture and porting notes
- `TODO.md` — execution checklist / tracking board

## Requirements
- .NET 8 SDK
- Linux machine with Steam installed

## Current commands
```bash
# detect the Steam root
dotnet run --project src/SteamUtility.Cli -- detect

# list library folders
dotnet run --project src/SteamUtility.Cli -- libraries

# list installed apps from appmanifest files
dotnet run --project src/SteamUtility.Cli -- apps

# list compatdata entries
dotnet run --project src/SteamUtility.Cli -- compatdata

# list discovered compatibility tools
dotnet run --project src/SteamUtility.Cli -- compat-tools

# list explicit compatibility tool mappings from config.vdf
dotnet run --project src/SteamUtility.Cli -- compat-mapping

# merge apps + compatdata + mapping into one report
dotnet run --project src/SteamUtility.Cli -- compat-report

# query account ownership and write the upstream-compatible games.json payload
dotnet run --project src/SteamUtility.Cli -- check_ownership /tmp/games.json "[730,570,440]"

# read achievement/stat data and cache it locally
dotnet run --project src/SteamUtility.Cli -- get_achievement_data 440 /tmp/steam-utility-cache

# legacy achievement/stat commands from the upstream CLI
dotnet run --project src/SteamUtility.Cli -- unlock_achievement 440 ACH_ID
dotnet run --project src/SteamUtility.Cli -- update_stats 440 "[{\"name\":\"STAT\",\"value\":100}]"

# examples with filters / JSON
dotnet run --project src/SteamUtility.Cli -- apps --match proton
dotnet run --project src/SteamUtility.Cli -- compat-report --app-id 123456 --json
dotnet run --project src/SteamUtility.Cli -- check_ownership /tmp/games.json --json
```

## Global options
- `--json` — emit structured JSON instead of text
- `--app-id <id>` — filter by a specific AppID where applicable
- `--match <text>` — case-insensitive name/text filter where applicable

## What the project can answer right now
- Where Steam is installed on Linux
- Which library folders exist
- Which apps appear installed from manifests
- Which apps have compatdata prefixes
- Which Proton/runtime folders appear available
- Which apps have explicit compatibility-tool assignments in `config/config.vdf`
- Which queried AppIDs are owned by the logged-in Steam account when the native client is running
- Achievement/stat data and mutations for apps that expose Steam user stats

## Design direction
The port is being built in layers:
1. Filesystem and config discovery
2. Linux compatibility/runtime mapping
3. Feature reconstruction behind clean interfaces
4. Replacement or removal of Windows-only behavior

## Next priorities
See `TODO.md` for the live checklist.
