# Linux Steam Library Discovery

## Purpose
Replace the Windows Registry-based Steam discovery path with a Linux-native filesystem flow.

## Primary discovery flow
1. Resolve Steam root directory
2. Resolve primary `steamapps` directory
3. Read `steamapps/libraryfolders.vdf`
4. Enumerate additional library folders
5. Later: scan `appmanifest_*.acf` inside each library

## Common Linux Steam root candidates
- `~/.steam/steam`
- `~/.local/share/Steam`

## Files of interest
- `steamapps/libraryfolders.vdf`
- `steamapps/appmanifest_*.acf`
- `config/config.vdf`
- `config/loginusers.vdf`
- `userdata/*/config/localconfig.vdf`
- `userdata/*/config/sharedconfig.vdf`

## Current implementation status
Implemented:
- Steam root discovery for common Linux paths
- Minimal VDF reader for quoted-key Valve files
- Parsing of `libraryfolders.vdf`
- Aggregated installation model
- App manifest parsing
- Proton / compatibility tool discovery
- Native Linux Steam API loading
- Active Steam login user parsing
- User-specific config scanning for app scopes

Not implemented yet:
- Feature parity with the original Windows-only idle behavior
