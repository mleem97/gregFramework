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

### 1) Add or adjust hook sources

- Hook definitions live in `JoniML/HarmonyPatches.cs`.
- Prefer deterministic, low-noise methods in `Assembly-CSharp`.
- Wrap all patch logic in safe `try/catch` blocks.

### 2) Emit events

- Event IDs live in `JoniML/EventIds.cs`.
- Dispatch helpers live in `JoniML/EventDispatcher.cs`.
- Use:
	- `FireSimple(eventId)` for signal-only events,
	- `FireValueChanged(eventId, oldValue, newValue, delta)` for numeric changes,
	- typed helpers (`FireShopItemAdded`, `FireMonthEnded`, etc.) for structured payloads.

### 3) Consume events in native Rust mods

- Native plugin loading/forwarding is implemented in `JoniML/FfiBridge.cs`.
- Your plugin receives events via optional `mod_on_event(eventId, dataPtr, dataLen)`.
- Use the Rust bridge project for ABI and helper crates:
	- `https://github.com/Joniii11/DataCenter-RustBridge`

### 4) Extend game callable APIs

- C# export table is in `JoniML/GameApi.cs`.
- Runtime game access helpers are in `JoniML/GameHooks.cs`.
- Keep additions append-only to avoid ABI breakage.

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
