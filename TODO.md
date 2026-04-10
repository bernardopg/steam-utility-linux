# TODO

## Done
- [x] Create public repository scaffold
- [x] Move solution to .NET 8
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

## In progress
- [ ] Improve compatibility tool detection heuristics
- [ ] Parse more Steam config/state files beyond compatibility mapping
- [ ] Expand automated tests

## Next
- [ ] Parse active user-specific Steam config where relevant
- [ ] Detect tool assignment precedence and edge cases
- [ ] Map compatdata entries back to app names in a richer report
- [ ] Add JSON schema stability notes / versioning
- [ ] Add tests for compatibility tool scanner
- [ ] Add tests for compatdata scanner
- [ ] Add logging / diagnostics mode
- [ ] Add release build instructions

## Later
- [ ] Evaluate Linux-native Steam API loading strategy
- [ ] Reconstruct original commands one by one behind abstractions
- [ ] Decide what to do with Win32-only idle/window behavior
- [ ] Consider Tauri or other GUI only after core parity is clearer
- [ ] Add CI pipeline
- [ ] Package binaries/releases

## Notes
- The current milestone is discovery/parsing, not full behavioral parity.
- The riskiest missing area is anything coupled to Win32 or Windows-only Steam client loading.
