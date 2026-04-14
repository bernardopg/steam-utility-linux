# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-04-14

### Added

#### Core library (`SteamUtility.Core`)
- Cross-platform .NET 8 solution with `SteamUtility.Core` and `SteamUtility.Cli` projects
- Runtime platform selector that automatically picks Linux or Windows services
- Linux Steam root discovery (`~/.steam/steam`, `~/.local/share/Steam`)
- Windows Steam root discovery via registry keys and fallback installation paths
- Minimal Valve VDF parser (`SimpleVdfReader` / `KeyValueParser`)
- `libraryfolders.vdf` parsing to enumerate Steam library folders
- `appmanifest_*.acf` parsing for installed app metadata
- Installed app discovery across all Steam library folders
- `compatdata/<AppId>` prefix discovery for Linux/Proton setups
- Bundled and custom compatibility tool discovery with detection heuristics
- `config/config.vdf` parsing for per-app compatibility tool assignments
- `config/loginusers.vdf` parsing for Steam account enumeration
- User-specific app config scanning under `userdata/*/config`
- Per-app compatibility report generation (merged apps + compatdata + tool mapping)
- Steam environment summary / state-report service
- Linux native Steam client loading (`steamclient.so`)
- Windows native Steam client loading (`steamclient.dll`)
- Linux Steamworks native library resolution (`libsteam_api.so`) with multi-path search
- Windows Steamworks native library resolution
- Steamworks session management (`SteamworksSession`) with callback pumping and timeout semantics
- Ownership lookup through the running Steam client via `ISteamApps001`
- Achievement read and mutation via `ISteamApps008` / `ISteamUserStats`
- Statistics read and mutation via Steamworks user-stats interfaces
- `StatsSchemaLoader` for reading and caching achievement/stat schemas
- Native COM-style interop wrappers (`NativeWrapper`, `NativeClass`, `Utf8StringHandle`)
- `SteamworksInitializationException` and `SteamClientInitializeException` aligned with upstream failure semantics
- JSON output contracts with `schemaVersion = 1` for all structured commands

#### CLI (`SteamUtility.Cli`)
- `detect` — locate the Steam root directory
- `libraries` — list configured Steam library folders
- `apps` — list installed apps from `appmanifest` files
- `compatdata` — list `compatdata` entries
- `compat-tools` — list discovered compatibility tools
- `compat-mapping` — list explicit tool assignments from `config.vdf`
- `compat-report` — merged compatibility report (apps + compatdata + mapping)
- `state-report` — full Steam environment summary
- `check_ownership` — query account ownership and write a `games.json` payload
- `get_achievement_data` — read and cache achievement/stat data for an app
- `unlock_achievement` / `lock_achievement` / `toggle_achievement` — per-achievement mutations
- `unlock_all_achievements` / `lock_all_achievements` — bulk achievement mutations
- `update_stats` — write stat values from a JSON payload
- `reset_all_stats` — reset all stats for an app
- `idle` — keep a Steam app session alive
- Command aliases accepting both underscore (`check_ownership`) and hyphen (`check-ownership`) forms
- Global options: `--json`, `--diagnostics`, `--app-id <id>`, `--match <text>`, `--help` / `-h`
- JSON structured output mode for all applicable commands

#### Tests (`SteamUtility.Tests`)
- Unit tests for VDF parsers, library/app/compatdata/tool scanners, and compatibility report service
- CLI parity tests for all upstream commands (`check_ownership`, `idle`, `get_achievement_data`, achievement mutations, stats mutations, dispatch/help)
- Fake implementations of `ISteamApiLibraryResolver`, `ISteamClientLibraryLoader`, and related abstractions for deterministic native-service testing
- Regression tests for `SteamworksSession`, `SteamOwnershipService`, `SteamApiNative`, and `StatsSchemaLoader`
- `NativeServiceRegressionTests` covering end-to-end initialization and failure paths
- `TestRunner` harness for in-process test execution and result reporting

#### CI / Release
- GitHub Actions CI workflow (`ci.yml`) — build and test on every push/PR
- GitHub Actions release workflow (`release.yml`) — triggered by `v*` tags; builds self-contained single-file binaries and publishes them as GitHub release assets
- Release matrix: `linux-x64`, `linux-arm64`, `win-x64`

#### Documentation
- `README.md` with command reference, global options, and platform notes
- `docs/LIBRARY_DISCOVERY.md` — Steam library discovery details
- `docs/PROTON_AND_COMPATDATA.md` — Proton and compatdata behavior
- `docs/WINDOWS_DEPENDENCIES.md` — Windows native dependency notes
- `docs/CLI_PARITY_MATRIX.md` — upstream command surface parity matrix
- `docs/LINUX_REAL_STEAM_VALIDATION.md` — Linux live-Steam manual validation checklist
- `docs/PORTING_PLAN.md` — cross-platform porting plan and decisions
- `docs/JSON_OUTPUTS.md` — JSON output contracts and schema versioning notes
- `docs/RELEASE.md` — local release build instructions
- `REPORT_REAL_VALIDATION_TEST_LINUX.md` — Linux validation execution report (AppID 70120)
- `TODO.md` — execution checklist and tracking board

[Unreleased]: https://github.com/bernardopg/steam-utility-multiplataform/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/bernardopg/steam-utility-multiplataform/releases/tag/v1.0.0
