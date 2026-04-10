# Proton and CompatData Discovery

## Purpose
Map Linux compatibility data that is relevant for Windows game execution through Proton.

## What is scanned
### Compatibility tools
- `compatibilitytools.d/` under the Steam root for custom tools
- `steamapps/common/` under the Steam root for bundled Proton/runtime installs

### Per-game compatibility data
- `steamapps/compatdata/<AppId>/`
- `steamapps/compatdata/<AppId>/pfx`

## Why this matters
This is the Linux-side replacement for assumptions that a Windows-native runtime environment exists automatically.

## Current implementation status
Implemented:
- Discovery of custom compatibility tool roots
- Heuristic discovery of bundled Proton/runtime folders
- Discovery of `compatdata` entries by AppID
- Exposure path ready for CLI commands

Not implemented yet:
- Parse active compatibility tool assignments from config
- Map which tool is assigned to which app
- Inspect prefixes in detail
- Launch or emulate original Windows-only features
