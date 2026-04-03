# Modding Guide

Last updated: 2026-04-03

This guide describes the practical workflow for extending `FrikaModFramework` safely.

## Read this first

- `README_MODDING.md` for end-to-end usage details.
- `.wiki/Architecture.md` for boundaries and responsibilities.
- `HOOKS.md` for currently verified hook targets.

## Development loop

1. Build framework in `Debug` while developing.
2. Deploy DLLs to game `Mods` folder.
3. Launch game and confirm load in `MelonLoader/Latest.log`.
4. Verify behavior and diagnostics output.
5. Promote to `Release` once stable.

## Hooks and events pipeline

1. Add or update a hook patch in `FrikaMF/JoniMF/HarmonyPatches.cs`.
2. Register/confirm event IDs in `FrikaMF/JoniMF/EventIds.cs`.
3. Emit payload via `FrikaMF/JoniMF/EventDispatcher.cs`.
4. Consume in native plugin through `mod_on_event(eventId, dataPtr, dataLen)`.

### Minimal C# patch example

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

### Minimal Rust listener example

```rust
#[no_mangle]
pub extern "C" fn mod_on_event(event_id: u32, _data_ptr: *const u8, _data_len: u32) {
    if event_id == 10 {
        println!("Server power event received");
    }
}
```

## How to stay update-resilient

- Prefer hooking deterministic gameplay methods over generic Unity internals.
- Keep hook behavior small and fail-safe (`try/catch` in high-risk paths).
- Use runtime method exports to re-check hook candidates after each game update.
- Treat `HOOKS.md` as a verified contract: mark only confirmed targets as verified.

## Content packs (`StreamingAssets/Mods`)

Use one folder per pack:

```text
Data Center_Data/StreamingAssets/Mods/MyServerPack/
  config.json
  model.obj
  model.mtl
  texture.png
  icon.png
```

Scaffold helper:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\New-StreamingAssetModPack.ps1 -GamePath "C:\Program Files (x86)\Steam\steamapps\common\Data Center" -ModName "MyServerPack"
```

## Fast deploy loop

```powershell
. .\scripts\Invoke-DataCenterModDeploy.ps1
Invoke-Deploy --all
```

Use `-WhatIf` for dry-run validation.

## Event payload notes

- `FireSimple(eventId)`: no payload.
- `FireValueChanged(...)`: `old/new/delta` as `float` values.
- Typed helpers exist for structured payloads (shop, save/load, employees, etc.).

## Contributor expectations

- Keep event contracts stable once external mods depend on them.
- Update docs and hook verification notes together with code changes.
- Use Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`).

## Rust plugin reference

- `https://github.com/Joniii11/DataCenter-RustBridge`
