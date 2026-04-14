# Linux Real Steam Validation Checklist

Use this checklist on a real Linux machine with Steam installed and logged in. The target environment for this repository is Linux-first desktop usage, including Arch Linux.

## Purpose

The automated test suite now proves deterministic CLI parity for the full upstream command surface, but it does not prove that native Steam integration works against a live Steam session, real `steamclient.so`, and real `libsteam_api.so` resolution.

This document is the manual validation gate for that last-mile proof.

## Preconditions

Required:
- Linux machine with Steam installed
- User account logged into Steam
- `.NET 8 SDK`
- this repository checked out locally
- at least one known game/app that exposes Steam user stats or achievements

Recommended before starting:
```bash
cd /path/to/steam-utility-multiplataform
dotnet build
dotnet run --project tests/SteamUtility.Tests
```

## Useful environment inspection

Verify the expected Steam roots exist:
```bash
ls -ld ~/.steam/steam ~/.local/share/Steam 2>/dev/null
```

Inspect common client library candidates:
```bash
find ~/.steam ~/.local/share/Steam -name 'steamclient.so' 2>/dev/null | head -20
find ~/.steam ~/.local/share/Steam -name 'libsteam_api.so' 2>/dev/null | head -20
```

## Baseline discovery commands

### 1. Detect Steam root
```bash
dotnet run --project src/SteamUtility.Cli -- detect
```
Expected:
- prints a valid Steam installation path

JSON mode:
```bash
dotnet run --project src/SteamUtility.Cli -- detect --json
```
Expected:
- `found: true`
- non-empty `rootPath`

### 2. Library folders
```bash
dotnet run --project src/SteamUtility.Cli -- libraries
```
Expected:
- at least one library folder
- default library marked

### 3. Installed apps
```bash
dotnet run --project src/SteamUtility.Cli -- apps
```
Expected:
- installed manifests listed

### 4. State report
```bash
dotnet run --project src/SteamUtility.Cli -- state-report --diagnostics
```
Expected:
- active account visible
- user config/state summary present
- diagnostics emitted to stderr

## Ownership validation

### 5. Query ownership for known app IDs
Pick app IDs you know the logged-in account owns.

Example:
```bash
dotnet run --project src/SteamUtility.Cli -- check_ownership /tmp/games.json "[440,570,730]"
cat /tmp/games.json
```
Expected:
- CLI reports success
- `/tmp/games.json` exists
- output shape is:
```json
{"games":[{"appid":440,"name":"..."}]}
```

Also validate JSON mode:
```bash
dotnet run --project src/SteamUtility.Cli -- check_ownership /tmp/games.json "[440,570,730]" --json
```
Expected:
- `success: true`
- `ownedCount >= 1`
- `outputPath` matches

Failure path to test:
- close Steam completely and rerun
Expected:
- failure JSON with `failureReason`
- human-readable `error`

## Achievement/stat data validation

Choose a known app with Steam achievements/stats.

### 6. Aggregate achievement/stat export
```bash
dotnet run --project src/SteamUtility.Cli -- get_achievement_data <APP_ID> /tmp/steam-utility-cache
```
Expected:
- success payload containing a written cache path
- output file exists under `/tmp/steam-utility-cache/<steamid>/achievement_data/<APP_ID>.json`
- file contains `achievements` and `stats` arrays

### 7. Specific item lookup
Use a known achievement ID or stat ID from the exported file.

```bash
dotnet run --project src/SteamUtility.Cli -- get_achievement_data <APP_ID> /tmp/steam-utility-cache <ITEM_ID>
```
Expected:
- JSON payload for a single achievement or stat
- `type` is `achievement` or `stat`

Failure path to test:
```bash
dotnet run --project src/SteamUtility.Cli -- get_achievement_data <APP_ID> /tmp/steam-utility-cache DOES_NOT_EXIST
```
Expected:
- `{"error":"ID not found"}`

## Achievement mutation validation

Only run these against a test title/account where mutation is safe.

### 8. Unlock one achievement
```bash
dotnet run --project src/SteamUtility.Cli -- unlock_achievement <APP_ID> <ACH_ID>
```
Expected:
- `Successfully unlocked achievement`
- re-running `get_achievement_data` for that item shows `achieved: true`

### 9. Lock one achievement
```bash
dotnet run --project src/SteamUtility.Cli -- lock_achievement <APP_ID> <ACH_ID>
```
Expected:
- `Successfully locked achievement`
- re-running `get_achievement_data` for that item shows `achieved: false`

### 10. Toggle one achievement
```bash
dotnet run --project src/SteamUtility.Cli -- toggle_achievement <APP_ID> <ACH_ID>
```
Expected:
- flips achievement state
- success message matches the direction taken

### 11. Unlock all achievements
```bash
dotnet run --project src/SteamUtility.Cli -- unlock_all_achievements <APP_ID>
```
Expected:
- `Successfully unlocked all achievements`

### 12. Lock all achievements
```bash
dotnet run --project src/SteamUtility.Cli -- lock_all_achievements <APP_ID>
```
Expected:
- `Successfully locked all achievements`

## Stat mutation validation

Again, use a safe test title/account.

### 13. Update stats
```bash
dotnet run --project src/SteamUtility.Cli -- update_stats <APP_ID> "[{\"name\":\"STAT_NAME\",\"value\":100}]"
```
Expected:
- `Successfully updated all stats`
- subsequent `get_achievement_data <APP_ID> ... <STAT_NAME>` reflects new value

Also test a float stat if the game has one:
```bash
dotnet run --project src/SteamUtility.Cli -- update_stats <APP_ID> "[{\"name\":\"FLOAT_STAT\",\"value\":19.5}]"
```

### 14. Reset all stats
```bash
dotnet run --project src/SteamUtility.Cli -- reset_all_stats <APP_ID>
```
Expected:
- `Successfully reset all stats`
- subsequent stat lookup returns default values

## Diagnostics validation

### 15. Run a diagnostic report in JSON mode
```bash
dotnet run --project src/SteamUtility.Cli -- state-report --json --diagnostics >/tmp/state-report.json 2>/tmp/state-report.stderr
cat /tmp/state-report.stderr
```
Expected:
- stderr contains diagnostic payloads
- stdout JSON remains valid and parseable

## Record results

For each command, record:
- machine/OS (example: Arch Linux)
- Steam install path used
- app IDs tested
- pass/fail
- exact stderr/stdout for failures
- whether the issue is:
  - CLI contract bug
  - native library resolution bug
  - live Steam session/state issue
  - game-specific limitation

## Known limitations to keep in mind

- Proton/compatdata commands are Linux-relevant, but Steamworks mutation commands depend more on the live Steam client and target title behavior than on Proton itself.
- Some games may expose no stats/achievements or may behave differently with server-backed progression.
- A deterministic CLI test passing does not guarantee the target title allows the mutation.

## Exit criteria

Linux real-session validation is considered complete when:
- all upstream commands have been exercised at least once on a live Linux Steam session
- failures are documented with command, app ID, and observed output
- any platform-specific issues are either fixed or added to `TODO.md`
