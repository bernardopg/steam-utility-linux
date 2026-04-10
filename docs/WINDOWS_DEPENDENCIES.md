# Windows-Specific Dependencies To Replace

## Native UI and process model
The original project uses Win32 constructs that do not exist on Linux:
- `CreateWindowEx`
- `ShowWindow`
- `DestroyWindow`
- hidden message-window assumptions

## Native libraries
The original code assumes Windows dynamic libraries and Windows loader APIs:
- `steam_api.dll`
- `steamclient.dll`
- `LoadLibraryEx`
- `SetDllDirectory`

## Operating system integration
The original project depends on Windows-specific installation discovery:
- Windows Registry lookup for Steam root
- Windows path conventions
- x86 / .NET Framework desktop packaging assumptions

## Replacement strategy
- Use filesystem-based Steam detection on Linux
- Resolve Linux shared libraries explicitly
- Move native library loading behind interfaces
- Keep platform-specific code out of application logic
