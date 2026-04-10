# Release Build Instructions

## Local build
```bash
dotnet test tests/SteamUtility.Tests
dotnet publish src/SteamUtility.Cli -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

## Suggested output
- Published binaries land under `src/SteamUtility.Cli/bin/Release/<target-framework>/<runtime>/publish/`
- The release workflow packages the published CLI into zip archives per runtime

## Release workflow
- Push a tag that matches `v*`
- GitHub Actions builds the CLI for `linux-x64` and `linux-arm64`
- The workflow uploads zip assets to the GitHub release

## Notes
- The project is Linux-first, so release assets should stay focused on Linux runtimes unless a new platform target is added intentionally.
- If the CLI command surface changes, update `TODO.md`, `README.md`, and the JSON output notes together.
