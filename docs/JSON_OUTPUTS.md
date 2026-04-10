# JSON Outputs

## Versioning
- Structured report models use `schemaVersion = 1`.
- When a report shape changes in a breaking way, bump the version and document the new fields.
- Commands that still emit legacy ad hoc JSON should be treated as compatibility-first outputs, not versioned contracts yet.

## Versioned outputs today
- `apps` returns `SteamAppManifest` records with `schemaVersion = 1`
- `libraries` returns `SteamLibraryFolder` records with `schemaVersion = 1`
- `compatdata` returns `SteamCompatDataEntry` records with `schemaVersion = 1`
- `compat-tools` returns `SteamCompatibilityTool` records with `schemaVersion = 1`
- `compat-mapping` returns `SteamAppCompatibilityAssignment` records with `schemaVersion = 1`
- `compat-report` returns `SteamCompatibilityReportEntry` records with `schemaVersion = 1`
- `state-report` returns `SteamEnvironmentSummary` records with `schemaVersion = 1`

## Diagnostics
- `--diagnostics` adds extra log lines to stderr for discovery and report commands.
- Diagnostics are intentionally separate from JSON output so scripts can keep reading stdout.
