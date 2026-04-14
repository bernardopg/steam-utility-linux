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
| `check_ownership` | `<output_path> [app_ids]` | `<output_path> [app_ids_json_or_file]` | Command exists and writes upstream-style `games.json` payload | Usage/failure/success file output now regression-tested; default remote app-id source still untested | Yes for true ownership lookup | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Add default-source test seam and Linux/Windows real-session validation |
| `idle` | `<app_id> [app_name]` | `<app_id> [app_name]` | Command exists with Linux keep-alive note instead of Win32 hidden window | Invalid app id, missing installation, success payload, optional app name, and init failure are now regression-tested | Yes | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Extend toward real-session validation |
| `get_achievement_data` | `<app_id> [output_dir]` | `<app_id> [storage_dir] [item_id]` | Command exists and is richer than upstream | Invalid app id, missing installation, item lookup, aggregate file output, and init failure are now regression-tested | Yes | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Extend toward real-session validation and loader/session-specific regression tests |
| `unlock_achievement` | `<app_id> <ach_id>` | `<app_id> <achievement_id>` | Command exists | Missing achievement id, success, missing achievement, and init failure are now regression-tested | Yes | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Extend toward real-session validation and low-level session regression tests |
| `lock_achievement` | `<app_id> <ach_id>` | `<app_id> <achievement_id>` | Command exists | Success path is now regression-tested and shares the same single-mutation seam as unlock | Yes | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Extend toward real-session validation and low-level session regression tests |
| `toggle_achievement` | `<app_id> <ach_id>` | `<app_id> <achievement_id>` | Command exists | Unlock path, lock path, and validation failure are now regression-tested | Yes | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Extend toward real-session validation and low-level session regression tests |
| `unlock_all_achievements` | `<app_id>` | `<app_id>` | Command exists | Success and partial-failure paths are now regression-tested | Yes | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Extend toward real-session validation and bulk-mutation session tests |
| `lock_all_achievements` | `<app_id>` | `<app_id>` | Command exists | Success and post-reset validation failure are now regression-tested | Yes | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Extend toward real-session validation and bulk-mutation session tests |
| `update_stats` | `<app_id> <[stat_objects...]>` | `<app_id> <json_array>` | Command exists | Missing payload, invalid JSON, success, partial failure, store failure, validation failure, and init failure are now regression-tested | Yes | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Extend toward real-session validation and low-level stat session regression tests |
| `reset_all_stats` | `<app_id>` | `<app_id>` | Command exists | Success, reset failure, validation failure, and init failure are now regression-tested | Yes | deterministic CLI tests added + fake-backed native seam + real-environment validation still needed | Extend toward real-session validation and low-level stat session regression tests |

## Platform/runtime parity

| Area | Local status | Automated coverage | Gap |
| --- | --- | --- | --- |
| Linux runtime selection | Implemented | `SteamPlatformRuntimeTests` | good baseline, still needs real-session validation |
| Linux `steamclient.so` resolution | Implemented | `LinuxSteamClientLibraryTests` | no end-to-end proof with running Steam |
| Linux `libsteam_api.so` resolution | Implemented | `LinuxSteamApiLibraryResolverTests` | no end-to-end proof with running Steam |
| Windows runtime selection | Implemented | indirect only through `SteamPlatformRuntimeTests` on Windows hosts | add dedicated resolver/locator tests |
| Windows Steam locator | Implemented | none dedicated | add deterministic tests or docs |
| Windows client/API loaders | Implemented | none dedicated | add deterministic tests or docs |

## Current conclusion

The local repository already implements the full upstream command surface. The main missing work is not command creation; it is parity confidence:

1. deterministic CLI contract tests
2. fake-backed tests around native Steamworks-dependent behavior
3. explicit Linux and Windows real-environment validation
4. coverage reporting in CI
