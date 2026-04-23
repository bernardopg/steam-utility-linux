# TODO

## Done
- [x] Create public repository scaffold
- [x] Initial migration to modern .NET (the repository now targets .NET 10)
- [x] Create `SteamUtility.Core`
- [x] Create `SteamUtility.Cli`
- [x] Add Linux Steam root locator
- [x] Add minimal VDF parser
- [x] Parse `libraryfolders.vdf`
- [x] Model Steam installation and library folders
- [x] Parse `appmanifest_*.acf`
- [x] Scan installed apps across library folders
- [x] Add CLI commands for `detect`, `libraries`, and `apps`
- [x] Scan `compatdata/<AppId>` folders
- [x] Scan bundled/custom compatibility tools
- [x] Add CLI commands for `compatdata` and `compat-tools`
- [x] Write `README.md`
- [x] Write `TODO.md`
- [x] Parse `config/config.vdf` compatibility mappings
- [x] Add merged compatibility report command
- [x] Add CLI filtering by AppID/text
- [x] Add JSON output mode
- [x] Add initial test project
- [x] Add Linux `steamclient.so` loader scaffold
- [x] Add initial `check_ownership` port through native Steam client interfaces
- [x] Port achievement/stat commands through native Linux `libsteam_api.so`
- [x] Improve compatibility tool detection heuristics
- [x] Expand automated tests
- [x] Parse more Steam config/state files beyond compatibility mapping
- [x] Broaden runtime validation of state-changing achievement/stat commands
- [x] Add explicit `--help` and `-h` handling in the CLI
- [x] Restore the optional app name argument for `idle <app_id> [app_name]`
- [x] Replace the Win32 hidden idle window with a Linux keep-alive loop
- [x] Align native initialization failures with upstream-style failure reasons
- [x] Match `get_achievement_data` callback and timeout semantics more closely
- [x] Align command success and error messages with upstream behavior where practical
- [x] Parse active user-specific Steam config where relevant
- [x] Detect tool assignment precedence and edge cases
- [x] Map compatdata entries back to app names in a richer report
- [x] Add JSON schema stability notes / versioning
- [x] Add tests for compatibility tool scanner
- [x] Add tests for compatdata scanner
- [x] Add logging / diagnostics mode
- [x] Add release build instructions
- [x] Add CI pipeline
- [x] Package binaries/releases

## Base parity
- [x] Add a platform selector that chooses Linux or Windows services at runtime
- [x] Add Windows Steam root discovery
- [x] Add Windows `steamclient.dll` loading scaffold
- [x] Add Windows Steamworks native library resolution
- [x] Add Windows release artifacts to the publishing workflow
- [x] Update README and release notes for cross-platform support

## Confirmed remaining work for upstream parity confidence
- [x] Add deterministic CLI parity coverage for generic dispatch/help behavior plus the upstream commands `check_ownership` and `idle`
- [x] Add deterministic CLI parity coverage for `get_achievement_data`, including item lookup, aggregate output file generation, and initialization failures
- [x] Add deterministic CLI parity coverage for achievement mutations (`unlock_achievement`, `lock_achievement`, `toggle_achievement`, `unlock_all_achievements`, `lock_all_achievements`)
- [x] Add deterministic CLI parity coverage for stats mutations (`update_stats`, `reset_all_stats`)
- [x] Publish a Linux real-Steam validation checklist documenting the live manual verification flow for all upstream commands
- [x] Execute Linux integration validation for the native Steam paths (`steamclient.so`, `libsteam_api.so`) with a real running Steam session, because automated tests still do not prove the Steamworks command path end-to-end
- [ ] Add Windows-specific automated or documented manual validation for `WindowsSteamLocator`, `WindowsSteamClientLibraryLoader`, and `WindowsSteamApiLibraryResolver`, which currently have no dedicated tests
- [ ] Execute Windows integration validation for the native Steam command flow (`check_ownership`, `idle`, `get_achievement_data`, achievement mutations, stats mutations`) with a real running Steam session, mirroring the completed Linux live validation
- [x] Add deterministic coverage for `check_ownership` when no AppID payload is provided, including the default remote source path (`DEFAULT_GAMES_URL`) and failure handling for fetch/parse errors
- [x] Add coverage collection/reporting (for example Coverlet + CI artifact/public summary); this is now implemented, and the repository's direct validation path remains `dotnet run --project tests/SteamUtility.Tests`
- [x] Add regression tests for the remaining high-risk native services (`SteamworksSession`, `SteamOwnershipService`, `SteamApiNative`, `StatsSchemaLoader`) after the new CLI extraction coverage

## Later
- [ ] Consider Tauri or other GUI only after core parity is clearer

## Notes
- The upstream command surface from `zevnda/steam-utility` is present in this repository (`check_ownership`, `idle`, `get_achievement_data`, `unlock_achievement`, `lock_achievement`, `toggle_achievement`, `unlock_all_achievements`, `lock_all_achievements`, `update_stats`, `reset_all_stats`).
- This repository already goes beyond upstream with extra discovery/report commands such as `detect`, `libraries`, `apps`, `compatdata`, `compat-tools`, `compat-mapping`, `compat-report`, and `state-report`.
- The main parity gap is no longer missing commands; it is proof and validation: Windows-specific runtime coverage, Windows real-session validation, and deterministic coverage for the default remote AppID source path used by `check_ownership`.
