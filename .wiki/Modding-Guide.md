# Modding Guide

This guide reflects the current workflow for extending the project.

## Start Points

- Read `README_MODDING.md` for framework usage
- Review `JoniML/HarmonyPatches.cs` and dispatcher files
- Use `.wiki/Architecture.md` to understand project split

## Rust Plugin Development

If you want to write plugins in Rust, use the Rust bridge project:

- `https://github.com/Joniii11/DataCenter-RustBridge`

## Typical Workflow

1. Build framework or standalone mod project in `Release` for runtime use.
2. Copy DLL output into the game `Mods` folder.
3. Launch game and verify in `MelonLoader/Latest.log`.
4. Iterate on code and config values.

## Hooks and Events Workflow

1. Add a patch in `JoniML/HarmonyPatches.cs`.
2. Add or reuse an ID in `JoniML/EventIds.cs`.
3. Dispatch via `JoniML/EventDispatcher.cs` with the correct payload shape.
4. Receive events in Rust through `mod_on_event(eventId, dataPtr, dataLen)`.

### Event payload examples

- `FireSimple(eventId)`: no payload.
- `FireValueChanged(...)`: 3x `float` payload (`old`, `new`, `delta`).
- `FireMonthEnded(int)`: single integer payload.
- `FireCustomEmployeeHired(string)`: UTF-8 string payload with null terminator.

## Build and Deploy Fast Loop

```powershell
. .\scripts\Invoke-DataCenterModDeploy.ps1
Invoke-Deploy --all
```

Use `-WhatIf` for dry-run validation.

## Hex Label Mod Notes

- Project path: `HexLabelMod/HexLabelMod.csproj`
- Build mode: release-only
- Config file: `hexposition.cfg` in game `UserData`
- Hotkey handling: Input System-based implementation

## Best Practices

- Keep features in dedicated services/classes.
- Prefer event-driven hooks over broad invasive patches.
- Add configuration defaults and fallback behavior.
- Validate changes with a focused project build before wider checks.
- Keep dispatched event contracts stable once used externally.
