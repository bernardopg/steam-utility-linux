# Release Build Instructions

## Recommended local validation

The repository uses a custom test runner in `tests/SteamUtility.Tests`, so the most direct local validation path is:

```bash
dotnet restore
dotnet build steam-utility-multiplataform.sln -c Release
dotnet run --project tests/SteamUtility.Tests -c Release
```

## Optional local coverage reproduction

To reproduce the same style of coverage report collected in CI:

```bash
dotnet tool install --global dotnet-coverage
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet build tests/SteamUtility.Tests -c Release
dotnet-coverage collect "dotnet run --project tests/SteamUtility.Tests -c Release --no-build" -f cobertura -o artifacts/coverage/coverage.cobertura.xml
reportgenerator "-reports:artifacts/coverage/coverage.cobertura.xml" "-targetdir:artifacts/coverage/report" "-reporttypes:HtmlSummary;TextSummary;Cobertura"
```

## Local publish

```bash
dotnet publish src/SteamUtility.Cli -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish src/SteamUtility.Cli -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=true
dotnet publish src/SteamUtility.Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Suggested output paths

- Default publish output lands under `src/SteamUtility.Cli/bin/Release/<target-framework>/<runtime>/publish/`
- Release packaging should produce one zip archive per runtime

## Runtime matrix

- `linux-x64`
- `linux-arm64`
- `win-x64`

## GitHub release workflow

The release workflow is triggered by pushing a tag that matches `v*`.

It currently:
- validates the tag format (`v1.2.3`, `v1.2.3-rc.1`, etc.)
- checks whether `CHANGELOG.md` was updated for the tagged version
- runs the repository test step before publishing
- builds self-contained single-file release assets for `linux-x64`, `linux-arm64`, and `win-x64`
- generates `CHECKSUMS.sha256` for each runtime bundle
- uploads zipped assets to the GitHub release

## Notes

- The project is cross-platform, with Linux and Windows release assets built from the same CLI codebase.
- `dotnet run --project tests/SteamUtility.Tests -c Release` is the authoritative local validation command today.
- The release workflow runs the custom test runner with `dotnet run --project tests/SteamUtility.Tests -c Release --no-build --no-restore`; keep this aligned with the test harness because `dotnet test` is not authoritative for this repository.
- If the CLI command surface changes, update `README.md`, `docs/JSON_OUTPUTS.md`, and command-specific docs together.
- Keep docs aligned with runtime behavior whenever platform-specific wording changes.
