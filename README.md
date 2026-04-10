# steam-utility-multiplataform

Cross-platform port of the original `steam-utility` project with Linux and Windows backends.
Base project: https://github.com/zevnda/steam-utility

## Current status
This repository is already past the bootstrap stage.

Implemented:
- .NET 8 solution and project structure
- Cross-platform core library + CLI entrypoint
- Steam root discovery on Linux and Windows
- Minimal Valve VDF parser
- `libraryfolders.vdf` parsing
- `appmanifest_*.acf` parsing
- Installed app discovery
- `compatdata` discovery
- Bundled/custom compatibility tool discovery
- `config/config.vdf` parsing for compatibility tool assignments
- Per-app compatibility report generation
- Linux and Windows Steam client loading scaffolds
- Initial ownership lookup through the running Steam client
- Native Linux and Windows Steamworks bridges for achievement/stat commands
- CLI filtering and JSON output support
- Initial test project for parsers/reporting
- Runtime validation of the state-changing achievement/stat commands
- Platform-specific replacement for the Win32 hidden-window idle behavior
- Broader test coverage
- CI and release workflow
- JSON schema/versioning notes for structured outputs

## Repository structure
- `src/SteamUtility.Core` — core domain and platform discovery/runtime logic
- `src/SteamUtility.Cli` — executable entrypoint
- `tests/SteamUtility.Tests` — initial unit tests
- `docs/` — architecture and porting notes
- `TODO.md` — execution checklist / tracking board

## Requirements
- .NET 8 SDK
- Linux or Windows machine with Steam installed

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

# summarize Steam library, user state, and config files
dotnet run --project src/SteamUtility.Cli -- state-report

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

# extra diagnostics
dotnet run --project src/SteamUtility.Cli -- state-report --diagnostics
```

## Global options
- `--json` — emit structured JSON instead of text
- `--diagnostics` — emit additional diagnostic logging to stderr
- `--app-id <id>` — filter by a specific AppID where applicable
- `--match <text>` — case-insensitive name/text filter where applicable

## What the project can answer right now
- Where Steam is installed on Linux
- Which library folders exist
- Which apps appear installed from manifests
- Which apps have compatdata prefixes
- Which Proton/runtime folders appear available
- Which apps have explicit compatibility-tool assignments in `config/config.vdf`
- Which Steam accounts are present in `config/loginusers.vdf`
- Which user-specific app scopes exist under `userdata/*/config`
- Which queried AppIDs are owned by the logged-in Steam account when the native client is running
- Achievement/stat data and mutations for apps that expose Steam user stats

## Output contracts
- JSON outputs that back the discovery/report commands carry `schemaVersion = 1`
- Versioning notes live in [docs/JSON_OUTPUTS.md](docs/JSON_OUTPUTS.md)

## Design direction
The port is being built in layers:
1. Filesystem and config discovery
2. Platform-specific compatibility/runtime mapping
3. Feature reconstruction behind clean interfaces
4. Replacement or removal of platform-specific behavior

## Release notes
- Local release build instructions are in [docs/RELEASE.md](docs/RELEASE.md)
- GitHub Actions CI and release workflows live under `.github/workflows/`

## Next priorities
See `TODO.md` for the live checklist.
