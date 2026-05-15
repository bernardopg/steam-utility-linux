# Linux Real Steam Validation Report

**Date:** 2026-04-14
**Machine:** Linux desktop
**Steam account:** `faze_spratty ([REDACTED_STEAM_ID])`
**Steam root:** `/home/bitter/.steam/steam`
**Library root:** `/home/bitter/.local/share/Steam`
**Validation target:** AppID `70120` — `Hacker Evolution Duality`

## Scope
Live validation of the Linux Steam integration, including:
- Steam root discovery
- library/app discovery
- account ownership checks
- achievement/stat data reads
- achievement mutation commands
- stat mutation commands

## Baseline environment
- Steam is installed and running.
- `steamclient.so` was found at `/home/bitter/.local/share/Steam/linux64/steamclient.so`.
- `libsteam_api.so` is present in the Steam library.
- The logged-in Steam user is active and owned AppID `70120`.

## Command results

| Command                                                                            | Result                        |
| ---------------------------------------------------------------------------------- | ----------------------------- |
| `dotnet build steam-utility-multiplataform.sln`                                    | Passed                        |
| `dotnet run --project src/SteamUtility.Cli -- detect`                              | Passed, root detected         |
| `dotnet run --project src/SteamUtility.Cli -- detect --json`                       | Passed, `found: true`         |
| `dotnet run --project src/SteamUtility.Cli -- libraries`                           | Passed, 1 library found       |
| `dotnet run --project src/SteamUtility.Cli -- apps --json`                         | Passed, installed apps listed |
| `dotnet run --project src/SteamUtility.Cli -- state-report --diagnostics`          | Passed                        |
| `dotnet run --project src/SteamUtility.Cli -- check_ownership /tmp/... "[70120]"`  | Passed, owned=1               |
| `dotnet run --project src/SteamUtility.Cli -- get_achievement_data 70120 /tmp/...` | Passed, cache written         |

### Discovery details
- `detect` returned: `Steam installation found at: /home/bitter/.steam/steam`
- `libraries` returned the library folder `/home/bitter/.local/share/Steam`
- `state-report` reported:
  - installed apps: `10`
  - compatdata entries: `9`
  - compatibility tools: `5`
  - active account: `faze_spratty ([REDACTED_STEAM_ID])`

### Ownership validation
`check_ownership` for `[70120]` returned:
```json
{"games":[{"appid":70120,"name":"Hacker Evolution Duality"}]}
```

### Achievement/stat data read
`get_achievement_data 70120` produced:
- 8 achievements
- 1 stat

Selected test items:
- Achievement: `ACH_TUTORIAL_COMPLETED` (`n00b`)
- Stat: `hedGamesPlayed` (`Games played`)

Initial state:
- `ACH_TUTORIAL_COMPLETED` = `false`
- `hedGamesPlayed` = `0`

## Achievement mutation validation

All achievement mutations were executed against AppID `70120`.

| Command                                           | Result |
| ------------------------------------------------- | ------ |
| `unlock_achievement 70120 ACH_TUTORIAL_COMPLETED` | Passed |
| `lock_achievement 70120 ACH_TUTORIAL_COMPLETED`   | Passed |
| `toggle_achievement 70120 ACH_TUTORIAL_COMPLETED` | Passed |
| `unlock_all_achievements 70120`                   | Passed |
| `lock_all_achievements 70120`                     | Passed |

Observed verification after each step:
- After `unlock_achievement`: `achieved: true`
- After `lock_achievement`: `achieved: false`
- After `toggle_achievement`: `achieved: true`
- After `unlock_all_achievements`: `achieved: true`
- After `lock_all_achievements`: `achieved: false`

### Achievement command output samples
```json
{"success":"Successfully unlocked achievement"}
{"success":"Successfully locked achievement"}
{"success":"Successfully unlocked all achievements"}
{"success":"Successfully locked all achievements"}
```

## Stat mutation validation

| Command                                                    | Result |
| ---------------------------------------------------------- | ------ |
| `update_stats 70120 [{"name":"hedGamesPlayed","value":1}]` | Passed |
| `reset_all_stats 70120`                                    | Passed |

Observed verification:
- After `update_stats`: `hedGamesPlayed` = `1`
- After `reset_all_stats`: `hedGamesPlayed` = `0`

### Stat command output samples
```json
{"success":"Successfully updated all stats"}
{"success":"Successfully reset all stats"}
```

## Bugs / issues found
No blocking bugs were observed in this live Linux Steam session.

Notes:
- The Steam API startup logs were present on every native command, but they did not affect execution.
- `toggle_achievement` reported `Successfully unlocked achievement` because the chosen achievement was locked before the toggle.

## Final state after validation
- `ACH_TUTORIAL_COMPLETED` was restored to `false`.
- `hedGamesPlayed` was restored to `0`.
- The account and game state were returned to their original observed values.

## Checklist
- [x] Linux Steam installation detected
- [x] Steam client libraries discovered
- [x] Live Steam session present
- [x] Ownership check for AppID `70120`
- [x] Achievement/stat data read for AppID `70120`
- [x] Unlock achievement
- [x] Lock achievement
- [x] Toggle achievement
- [x] Unlock all achievements
- [x] Lock all achievements
- [x] Update stat
- [x] Reset stats
- [x] Final state restored

## Conclusion
The Linux real-Steam validation completed successfully on a live Steam session, and the Steamworks mutation paths worked end-to-end for AppID `70120` (`Hacker Evolution Duality`).
