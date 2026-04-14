# steam-utility-multiplataform

[![CI](https://github.com/bernardopg/steam-utility-multiplataform/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/bernardopg/steam-utility-multiplataform/actions/workflows/ci.yml)
[![Coverage](https://img.shields.io/badge/coverage-reporting-brightgreen)](https://github.com/bernardopg/steam-utility-multiplataform/actions/workflows/ci.yml)
[![Release](https://github.com/bernardopg/steam-utility-multiplataform/actions/workflows/release.yml/badge.svg?branch=main)](https://github.com/bernardopg/steam-utility-multiplataform/actions/workflows/release.yml)

Cross-platform port of the original `steam-utility` project with Linux and Windows backends.
Base project: https://github.com/zevnda/steam-utility

## Current status
The project is functional on Linux and Windows.

Implemented:
- .NET 8 solution and project structure
- Cross-platform core library and CLI entrypoint
- Runtime platform selection (Linux or Windows services)
- Steam root discovery on Linux and Windows
- Minimal Valve VDF parser
- `libraryfolders.vdf` parsing
- `appmanifest_*.acf` parsing
- Installed app discovery across library folders
- `compatdata` discovery (when present)
- Bundled/custom compatibility tool discovery
- `config/config.vdf` parsing for compatibility tool assignments
- Per-app compatibility report generation
- Linux and Windows Steam client loading
- Ownership lookup through the running Steam client
- Steamworks achievement/stat read and mutation commands
- CLI filtering, diagnostics, and JSON output
- Automated tests for parsers/scanners/reporting/runtime
- CI and release workflow with Linux and Windows artifacts
- JSON schema/versioning notes for structured outputs

## Repository structure
- `src/SteamUtility.Core` — core domain and platform discovery/runtime logic
- `src/SteamUtility.Cli` — executable entrypoint
- `tests/SteamUtility.Tests` — automated tests
- `docs/` — architecture, platform behavior, and release notes
- `TODO.md` — execution checklist / tracking board

## Requirements
- .NET 8 SDK
- Linux or Windows machine with Steam installed
- Steam running and logged in for ownership and Steamworks mutation commands

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

# mutate achievements/stats
dotnet run --project src/SteamUtility.Cli -- unlock_achievement 440 ACH_ID
dotnet run --project src/SteamUtility.Cli -- lock_achievement 440 ACH_ID
dotnet run --project src/SteamUtility.Cli -- toggle_achievement 440 ACH_ID
dotnet run --project src/SteamUtility.Cli -- unlock_all_achievements 440
dotnet run --project src/SteamUtility.Cli -- lock_all_achievements 440
dotnet run --project src/SteamUtility.Cli -- update_stats 440 "[{\"name\":\"STAT\",\"value\":100}]"
dotnet run --project src/SteamUtility.Cli -- reset_all_stats 440

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
- `--help` / `-h` — show command help

## Command aliases
The CLI accepts underscore and hyphen forms for selected commands:
- `check_ownership` / `check-ownership`
- `get_achievement_data` / `get-achievement-data`
- `unlock_achievement` / `unlock-achievement`
- `lock_achievement` / `lock-achievement`
- `toggle_achievement` / `toggle-achievement`
- `unlock_all_achievements` / `unlock-all-achievements`
- `lock_all_achievements` / `lock-all-achievements`
- `update_stats` / `update-stats`
- `reset_all_stats` / `reset-all-stats`

## What the project can answer right now
- Where Steam is installed on Linux or Windows
- Which library folders exist
- Which apps appear installed from manifests
- Which apps have compatdata prefixes (mainly relevant on Linux/Proton setups)
- Which Proton/runtime folders appear available (mainly relevant on Linux/Proton setups)
- Which apps have explicit compatibility-tool assignments in `config/config.vdf`
- Which Steam accounts are present in `config/loginusers.vdf`
- Which user-specific app scopes exist under `userdata/*/config`
- Which queried AppIDs are owned by the logged-in Steam account when the native client is running
- Achievement/stat data and mutations for apps that expose Steam user stats

## Platform notes
- Runtime services are selected automatically by OS.
- Linux discovery uses common Steam roots (`~/.steam/steam`, `~/.local/share/Steam`) and Linux native libraries.
- Windows discovery uses registry and fallback installation paths, then resolves Windows native libraries.
- Proton/compatdata commands remain available on both platforms, but they usually return meaningful data only on Linux environments using Proton.

## Output contracts
- JSON outputs that back the discovery/report commands carry `schemaVersion = 1`
- Versioning notes live in [docs/JSON_OUTPUTS.md](docs/JSON_OUTPUTS.md)

## Documentation index
- [docs/LIBRARY_DISCOVERY.md](docs/LIBRARY_DISCOVERY.md)
- [docs/PROTON_AND_COMPATDATA.md](docs/PROTON_AND_COMPATDATA.md)
- [docs/WINDOWS_DEPENDENCIES.md](docs/WINDOWS_DEPENDENCIES.md)
- [docs/CLI_PARITY_MATRIX.md](docs/CLI_PARITY_MATRIX.md)
- [docs/LINUX_REAL_STEAM_VALIDATION.md](docs/LINUX_REAL_STEAM_VALIDATION.md)
- [docs/PORTING_PLAN.md](docs/PORTING_PLAN.md)
- [docs/JSON_OUTPUTS.md](docs/JSON_OUTPUTS.md)
- [docs/RELEASE.md](docs/RELEASE.md)

## Release notes
- Local release build instructions are in [docs/RELEASE.md](docs/RELEASE.md)
- GitHub Actions CI and release workflows live under `.github/workflows/`

## Coverage
The CI workflow now collects coverage for the custom test runner, publishes the raw coverage files as a build artifact, and writes a short summary to the GitHub job page.

To reproduce the same report locally:

```bash
dotnet tool install --global dotnet-coverage
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet build tests/SteamUtility.Tests -c Release
dotnet-coverage collect "dotnet run --project tests/SteamUtility.Tests -c Release --no-build" -f cobertura -o artifacts/coverage/coverage.cobertura.xml
reportgenerator "-reports:artifacts/coverage/coverage.cobertura.xml" "-targetdir:artifacts/coverage/report" "-reporttypes:HtmlSummary;TextSummary;Cobertura"
```

## Next priorities
See `TODO.md` for the live checklist.
