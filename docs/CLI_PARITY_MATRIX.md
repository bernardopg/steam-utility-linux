# CLI Parity Matrix

This document tracks parity between the upstream `zevnda/steam-utility` CLI and `steam-utility-multiplataform`.

Legend:
- covered now = already backed by meaningful automated coverage
- needs deterministic CLI test = can be validated without a live Steam client
- needs fake-backed native seam = needs test seams around Steamworks/native services
- needs real-environment validation = requires a real Steam installation/session to fully prove behavior

## Generic CLI behavior

| Behavior | Upstream | Local status | Evidence | Next action |
| --- | --- | --- | --- | --- |
| No args prints usage | Yes | Implemented and covered | `src/SteamUtility.Cli/SteamUtilityCli.cs`, `tests/SteamUtility.Tests/Cli/CliHelpAndDispatchTests.cs` | extend to more option combinations later |
| `--help` / `-h` prints usage | Yes | Implemented and covered | `HasHelpRequest` + `PrintUsage` in `src/SteamUtility.Cli/SteamUtilityCli.cs`, `tests/SteamUtility.Tests/Cli/CliHelpAndDispatchTests.cs` | extend to more option combinations later |
| Unknown command handling | Yes | Implemented and covered | default switch branch in `src/SteamUtility.Cli/SteamUtilityCli.cs`, `tests/SteamUtility.Tests/Cli/CliHelpAndDispatchTests.cs` | extend to more option combinations later |
| Underscore command names | Yes | Implemented | switch cases in `src/SteamUtility.Cli/SteamUtilityCli.cs` | add broader alias coverage across more commands |
| Hyphen aliases | No upstream requirement, local enhancement | Implemented and partially covered | switch cases in `src/SteamUtility.Cli/SteamUtilityCli.cs`, alias coverage in `tests/SteamUtility.Tests/Cli/CliHelpAndDispatchTests.cs` | add broader alias coverage across more commands |
| JSON output mode | Mixed upstream behavior | Implemented and partially covered | `--json` in `CliOptions`, JSON assertions in CLI tests | extend to more commands |
| Diagnostics mode | Local enhancement | Implemented | `--diagnostics` | add deterministic CLI tests |

## Upstream command surface

| Command | Upstream args | Local args | Current parity assessment | Success/error contract status | Live Steam required | Testability tier | Next action |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `check_ownership` | `<output_path> [app_ids]` | `<output_path> [app_ids_json_or_file]` | Command exists and writes upstream-style `games.json` payload | Usage/failure/success file output is regression-tested; Linux live validation exists for explicit AppIDs; the default remote app-id source path is now deterministically covered as well | Yes for true ownership lookup | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Focus next on Windows real-session validation |
| `idle` | `<app_id> [app_name]` | `<app_id> [app_name]` | Command exists with Linux keep-alive note instead of Win32 hidden window | Invalid app id, missing installation, success payload, optional app name, and init failure are regression-tested; Linux live validation exists | Yes | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Extend toward Windows real-session validation |
| `get_achievement_data` | `<app_id> [output_dir]` | `<app_id> [storage_dir] [item_id]` | Command exists and is richer than upstream | Invalid app id, missing installation, item lookup, aggregate file output, and init failure are regression-tested; Linux live validation exists | Yes | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Extend toward Windows real-session validation and loader/session-specific regression tests |
| `unlock_achievement` | `<app_id> <ach_id>` | `<app_id> <achievement_id>` | Command exists | Missing achievement id, success, missing achievement, and init failure are regression-tested; Linux live validation exists | Yes | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Extend toward Windows real-session validation and low-level session regression tests |
| `lock_achievement` | `<app_id> <ach_id>` | `<app_id> <achievement_id>` | Command exists | Success path is regression-tested and shares the same single-mutation seam as unlock; Linux live validation exists | Yes | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Extend toward Windows real-session validation and low-level session regression tests |
| `toggle_achievement` | `<app_id> <ach_id>` | `<app_id> <achievement_id>` | Command exists | Unlock path, lock path, and validation failure are regression-tested; Linux live validation exists | Yes | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Extend toward Windows real-session validation and low-level session regression tests |
| `unlock_all_achievements` | `<app_id>` | `<app_id>` | Command exists | Success and partial-failure paths are regression-tested; Linux live validation exists | Yes | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Extend toward Windows real-session validation and bulk-mutation session tests |
| `lock_all_achievements` | `<app_id>` | `<app_id>` | Command exists | Success and post-reset validation failure are regression-tested; Linux live validation exists | Yes | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Extend toward Windows real-session validation and bulk-mutation session tests |
| `update_stats` | `<app_id> <[stat_objects...]>` | `<app_id> <json_array>` | Command exists | Missing payload, invalid JSON, success, partial failure, store failure, validation failure, and init failure are regression-tested; Linux live validation exists | Yes | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Extend toward Windows real-session validation and low-level stat session regression tests |
| `reset_all_stats` | `<app_id>` | `<app_id>` | Command exists | Success, reset failure, validation failure, and init failure are regression-tested; Linux live validation exists | Yes | deterministic CLI tests added + fake-backed native seam + Linux real-environment validation completed + Windows real-environment validation pending | Extend toward Windows real-session validation and low-level stat session regression tests |

## Platform/runtime parity

| Area | Local status | Automated coverage | Gap |
| --- | --- | --- | --- |
| Linux runtime selection | Implemented | `SteamPlatformRuntimeTests` | real Linux session validation exists; preserve coverage as platform code evolves |
| Linux `steamclient.so` resolution | Implemented | `LinuxSteamClientLibraryTests` | unit-tested and validated in a live Linux Steam session; no equivalent Windows proof yet |
| Linux `libsteam_api.so` resolution | Implemented | `LinuxSteamApiLibraryResolverTests` | unit-tested and validated in a live Linux Steam session; no equivalent Windows proof yet |
| Windows runtime selection | Implemented | indirect only through `SteamPlatformRuntimeTests` on Windows hosts | add dedicated Windows validation |
| Windows Steam locator | Implemented | none dedicated | add deterministic tests or documented manual validation |
| Windows client/API loaders | Implemented | none dedicated | add deterministic tests or documented manual validation |

## Current conclusion

The local repository already implements the full upstream command surface.

Current confidence status:
1. deterministic CLI coverage exists for the full upstream command set
2. high-risk native services have fake-backed regression coverage
3. Linux has real Steam-session validation for the upstream command flow
4. CI coverage reporting exists and publishes coverage artifacts/summaries

Main remaining work:
1. Windows-specific validation for locator/loader/runtime paths
2. Windows real-environment validation for the Steam-native command flow
3. release-workflow/test-harness hardening around the custom test runner
