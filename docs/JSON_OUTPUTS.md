# JSON Outputs

## Versioning

- Structured discovery/report models currently serialize a `SchemaVersion` field with value `1`.
- In earlier notes this was described conceptually as `schemaVersion`, but the current wire format uses the default `System.Text.Json` property naming, so consumers should expect PascalCase fields such as `SchemaVersion`, `AppId`, and `RootPath`.
- When a structured report shape changes in a breaking way, bump the schema version and document the new fields.
- Commands that still emit legacy ad hoc JSON should be treated as compatibility-first outputs, not versioned contracts yet.

## Versioned outputs today

- `apps` returns `SteamAppManifest` records with `SchemaVersion = 1`
- `libraries` returns `SteamLibraryFolder` records with `SchemaVersion = 1`
- `compatdata` returns `SteamCompatDataEntry` records with `SchemaVersion = 1`
- `compat-tools` returns `SteamCompatibilityTool` records with `SchemaVersion = 1`
- `compat-mapping` returns `SteamAppCompatibilityAssignment` records with `SchemaVersion = 1`
- `compat-report` returns `SteamCompatibilityReportEntry` records with `SchemaVersion = 1`
- `state-report` returns `SteamEnvironmentSummary` records with `SchemaVersion = 1`

## Non-versioned compatibility outputs

These commands intentionally keep legacy payload semantics for upstream compatibility and are not part of the schema-versioned report contracts:
- `check_ownership`
- `idle` (multi-game mode prints one legacy init-result JSON line per child idle process)
- `get_achievement_data`
- achievement/stat mutation commands (`unlock_*`, `lock_*`, `toggle_*`, `update_stats`, `reset_all_stats`)

## Diagnostics

- `--diagnostics` adds extra log lines to stderr for discovery and report commands.
- Diagnostics are intentionally separate from JSON output so scripts can keep reading stdout.
