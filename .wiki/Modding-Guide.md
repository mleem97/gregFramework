# Modding Guide

This guide reflects the current workflow for extending the project.

## Start Points

- Read `README_MODDING.md` for framework usage
- Review `FrikaMF/JoniMF/HarmonyPatches.cs` and dispatcher files
- Use `.wiki/Architecture.md` to understand project split

## Rust Plugin Development

If you want to write plugins in Rust, use the Rust bridge project:

- `https://github.com/Joniii11/DataCenter-RustBridge`

## Typical Workflow

1. Build framework or standalone mod project in `Release` for runtime use.
1. Copy C# DLL output into the game `Mods` folder.
1. Put Rust plugins (optional) into `Mods/RustMods`.
1. Put custom object packs into `Data Center_Data/StreamingAssets/Mods/<PackName>`.
1. Launch game and verify in `MelonLoader/Latest.log`.
1. Iterate on code and config values.

## Hooks and Events Workflow

1. Add a patch in `FrikaMF/JoniMF/HarmonyPatches.cs`.
2. Add or reuse an ID in `FrikaMF/JoniMF/EventIds.cs`.
3. Dispatch via `FrikaMF/JoniMF/EventDispatcher.cs` with the correct payload shape.
4. Receive events in Rust through `mod_on_event(eventId, dataPtr, dataLen)`.

### C# patch example

```csharp
[HarmonyPatch(typeof(Server), nameof(Server.PowerButton))]
internal static class ServerPowerButtonPatch
{
    private static void Postfix(Server __instance)
    {
        EventDispatcher.FireServerPowered(__instance.isOn);
    }
}
```

### Rust event receiver example

```rust
#[no_mangle]
pub extern "C" fn mod_on_event(event_id: u32, data_ptr: *const u8, data_len: u32) {
    if event_id == 10 {
        println!("Server power event received");
    }
}
```

## Game-Object Packs (StreamingAssets/Mods)

Use one folder per object pack. Example structure:

```text
Data Center_Data/StreamingAssets/Mods/MyServerPack/
  config.json
  model.obj
  model.mtl
  texture.png
  icon.png
```

### Scaffold command

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\New-StreamingAssetModPack.ps1 -GamePath "C:\Program Files (x86)\Steam\steamapps\common\Data Center" -ModName "MyServerPack"
```

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
