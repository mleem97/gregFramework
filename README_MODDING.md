<!-- markdownlint-disable MD007 MD010 -->

# Modding Framework Usage Guide

This guide explains how to use the `Frikadelle Modding Framework` in `FrikaModFramework` as a base for custom mods for `Data Center`.

---

## Goal

Use this repository as a base to:

- communicate with Data Center through MelonLoader + IL2CPP,
- discover stable hook points in `Assembly-CSharp`,
- emit normalized framework events,
- build your own C# or Rust mods on top.

---

## Prerequisites

- `Data Center` installed
- `MelonLoader` installed for the game
- `.NET 6 SDK`
- Optional for development: decompiled sources in `il2cpp-unpack`

## Rust Plugin Reference

If you want to write plugins in Rust, use the Rust bridge project:

- `https://github.com/Joniii11/DataCenter-RustBridge`

---

## Build

Run from the repository root:

```powershell
dotnet build .\FrikaMF.csproj -c Debug
dotnet build .\FrikaMF.csproj -c Release
dotnet build .\HexLabelMod\HexLabelMod.csproj -c Release
```

Main framework output:

- `bin/Debug/net6.0/DataCenterModLoader.dll`
- `bin/Release/net6.0/DataCenterModLoader.dll`

Label mod output:

- `HexLabelMod/bin/Release/net6.0/HexLabelMod.dll`

## C# Checks

Run direct .NET checks (no Node/pnpm required):

```powershell
dotnet build .\FrikaMF.csproj -c Release -p:TreatWarningsAsErrors=true -nologo
dotnet build .\HexLabelMod\HexLabelMod.csproj -c Release -nologo
```

---

## Installation into the Game

1. Build the project(s).
2. Copy `DataCenterModLoader.dll` into the game `Mods` folder.
3. Optional: copy `HexLabelMod.dll` into the game `Mods` folder.
4. Start the game.
5. Verify in `MelonLoader/Latest.log` that the assembly is loaded.

---

## Build Modes

### Debug Build (development mode)

Debug mode enables development tooling such as:

- asset export hotkeys,
- IL2CPP diagnostics export,
- runtime hook scan/install helpers.

### Prod/Release Build (runtime mode)

Release mode keeps only core framework/runtime communication and disables development controls.

---

## Generated Diagnostics

At startup, the framework exports a consolidated signal snapshot:

- `Mods/ExportedAssets/Diagnostics/game-signals-full.txt`

This includes:

- event and trigger catalog,
- gameplay-relevant index from `il2cpp-unpack`,
- discovered runtime hook candidates.

---

## Building Your Own Mod on Top

Before first build/use: run the game once with MelonLoader so generated assemblies and runtime metadata are available.

### 1) Add or adjust hook sources

- Hook definitions live in `FrikaMF/JoniMF/HarmonyPatches.cs`.
- Prefer deterministic, low-noise methods in `Assembly-CSharp`.
- Wrap all patch logic in safe `try/catch` blocks.

### 2) Emit events

- Event IDs live in `FrikaMF/JoniMF/EventIds.cs`.
- Dispatch helpers live in `FrikaMF/JoniMF/EventDispatcher.cs`.
- Use:
	- `FireSimple(eventId)` for signal-only events,
	- `FireValueChanged(eventId, oldValue, newValue, delta)` for numeric changes,
	- typed helpers (`FireShopItemAdded`, `FireMonthEnded`, etc.) for structured payloads.

### 3) Consume events in native Rust mods

- Native plugin loading/forwarding is implemented in `FrikaMF/JoniMF/FfiBridge.cs`.
- Your plugin receives events via optional `mod_on_event(eventId, dataPtr, dataLen)`.
- Place Rust plugin DLLs in `Data Center/Mods/RustMods`.
- Use the Rust bridge project for ABI and helper crates:
	- `https://github.com/Joniii11/DataCenter-RustBridge`

### 4) Extend game callable APIs

- C# export table is in `FrikaMF/JoniMF/GameApi.cs`.
- Runtime game access helpers are in `FrikaMF/JoniMF/GameHooks.cs`.
- Keep additions append-only to avoid ABI breakage.

## StreamingAssets Game-Object Packs (recommended)

Goal: keep object-related data together in one folder and avoid requiring extra helper mods.

Use native game content packs under:

- `Data Center/Data Center_Data/StreamingAssets/Mods/<PackName>`

Typical files:

- `config.json`
- `model.obj`
- `model.mtl`
- `texture.png`
- `icon.png`

Scaffold command:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\New-StreamingAssetModPack.ps1 -GamePath "C:\Program Files (x86)\Steam\steamapps\common\Data Center" -ModName "MyServerPack"
```

Then align `config.json` with the current in-game `ExampleMod` schema for your version.

### Minimal C# patch example

```csharp
[HarmonyPatch(typeof(Server), nameof(Server.PowerButton))]
internal static class ServerPowerPatch
{
	private static void Postfix(Server __instance)
	{
		EventDispatcher.FireServerPowered(__instance.isOn);
	}
}
```

### Minimal Rust event receiver example

```rust
#[no_mangle]
pub extern "C" fn mod_on_event(event_id: u32, _data_ptr: *const u8, _data_len: u32) {
	if event_id == 10 {
		println!("Server power changed");
	}
}
```

## Local Release Upload

Because game DLL references are local and intentionally not available in GitHub runners, release assets are uploaded from local builds:

```powershell
. .\scripts\Publish-LocalRelease.ps1
$env:GITHUB_TOKEN = "<github_token_with_repo_scope>"
Publish-LocalRelease -Tag "v0.1.5"
```


---

> WARNING
> This project is still evolving. API/event contracts can change while interfaces are stabilized.

## Safety Recommendations

- Wrap runtime loops and hook callbacks in `try-catch`.
- Log errors through framework events (for example `ModErrorEvent`).
- Keep hooks selective and avoid patching high-frequency methods blindly.
- Validate hook targets against `Assembly-CSharp` behavior before shipping.

---

## Troubleshooting

- If the mod is not loaded, check `MelonLoader/Latest.log` first.
- If diagnostics are missing, verify write permissions for the game directory.
- If hook installation fails, inspect `hook-install-errors.txt` in diagnostics output.

## Automatic Hook Installation (No Manual Per-Hook Work)

You can auto-install runtime hooks via launch options, so you do not have to add each hook manually.

- Scan current `Assembly-CSharp` candidates and install (default max = `250`):
	- `--ffm-hooks-auto`
- Install all discovered scan candidates (very broad):
	- `--ffm-hooks-auto --ffm-hooks-all`
- Set explicit scan limit:
	- `--ffm-hooks-auto --ffm-hooks-max=1500`
- Install from an exported catalog file (`assembly-hooks.txt`):
	- `--ffm-hooks-catalog="C:\\path\\to\\assembly-hooks.txt"`
	- Optional limit: `--ffm-hooks-max=5000`

All installation errors are written to diagnostics (`hook-install-errors.txt`).

### One-Command `hooker.cs` scaffold

If you want to create a bridge scaffold file in one command:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\New-HookerBridge.ps1
```

This ensures `FrikaMF/JoniMF/Hooker.cs` exists (without overwriting an existing implementation).

### Runtime Hooker command flags (game launch options)

- `--hooker-auto`
- `--hooker-auto --hooker-all`
- `--hooker-auto --hooker-max=1500`
- `--hooker-catalog="C:\path\to\assembly-hooks.txt"`
- `--hooker-catalog="C:\path\to\assembly-hooks.txt" --hooker-max=5000`

---

## Summary

`Frikadelle Modding Framework` in `FrikaModFramework` provides an all-in-one base for Data Center modding:

- core game communication,
- diagnostics and discovery,
- centralized event-driven extension model.

## How You Can Help

- Add missing events for relevant gameplay actions.
- Improve payload typing/documentation for existing events.
- Add tested helper APIs in `GameApi.cs`.
- Contribute docs and examples for first-time mod authors.
