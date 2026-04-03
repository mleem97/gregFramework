# Architecture

Last updated: 2026-04-03

This page describes the high-level architecture and extension boundaries of `FrikaModFramework`.

## Runtime layers

1. **Game layer (`Assembly-CSharp`)**
   - Original game logic and IL2CPP runtime classes.
2. **Framework hook layer (`FrikaMF/JoniMF`)**
   - Harmony patches, event normalization, bridge exports.
3. **External mod layer (C# + Rust)**
   - Consumer mods and native plugins using exported contracts.

## Core projects

- `FrikaMF`: framework runtime, hooks, events, bridge, diagnostics.
- `HexLabelMod`: standalone visual helper mod with config-driven label placement.

## `FrikaMF` key components

- `Core.cs`: framework entrypoint and lifecycle orchestration.
- `HarmonyPatches.cs`: gameplay hook points.
- `EventIds.cs`: stable event ID contract.
- `EventDispatcher.cs`: payload marshalling and dispatch.
- `FfiBridge.cs`: native DLL loading and export invocation.
- `GameApi.cs`: callable API table passed to native mods.
- `GameHooks.cs`: safe helper wrappers for game state access/mutation.

## Data flow

`Assembly-CSharp method` → `Harmony patch` → `EventDispatcher` → `FfiBridge` → `mod_on_event`

This isolates volatile game internals from plugin-facing contracts.

## Runtime placement model

- C# DLLs: `Data Center/Mods`
- Rust DLLs: `Data Center/Mods/RustMods`
- Object/content packs: `Data Center/Data Center_Data/StreamingAssets/Mods`

## Diagnostics and verification

- Runtime dump exports and diagnostics are generated under framework diagnostics paths.
- Verified hook targets are tracked in `.wiki/HOOKS.md`.
- Post-update workflow should always include re-verifying critical hooks before shipping.

## Design rules

- Keep patches focused and reversible.
- Keep API/event contracts append-only when possible.
- Avoid hard dependencies on unstable UI/internal-only methods.
- Prefer deterministic gameplay hooks over generic engine utility methods.
