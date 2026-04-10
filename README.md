# steam-utility-linux

Linux-first port scaffold for the original Windows-oriented `steam-utility` project.

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
- CLI filtering and JSON output support
- Initial test project for parsers/reporting

Still missing for parity with the original Windows project:
- Native Steam API loading on Linux
- Full feature mapping of original commands
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

# examples with filters / JSON
dotnet run --project src/SteamUtility.Cli -- apps --match proton
dotnet run --project src/SteamUtility.Cli -- compat-report --app-id 123456 --json
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

## Design direction
The port is being built in layers:
1. Filesystem and config discovery
2. Linux compatibility/runtime mapping
3. Feature reconstruction behind clean interfaces
4. Replacement or removal of Windows-only behavior

## Next priorities
See `TODO.md` for the live checklist.
