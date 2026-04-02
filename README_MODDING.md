# Modding Framework Usage Guide

This guide explains how to use the `Frikadelle Modding Framework` as a base to implement custom mods for `Data Center`.

---

## Goal

Use this mod as a central runtime framework to:

- communicate with the game via MelonLoader/IL2CPP,
- discover game-relevant signals (events, trigger-like methods, gameplay keywords),
- build your own feature mods on top of a shared event hub.

---

## Prerequisites

- `Data Center` installed
- `MelonLoader` installed for the game
- `.NET 6 SDK`
- Optional for development: decompiled sources in `il2cpp-unpack`

---

## Installation into the Game

1. Build this project.
2. Copy `FrikadelleModdingFramework.dll` into the game `Mods` folder.
3. Start the game.
4. Verify in `MelonLoader/Latest.log` that the assembly is loaded.

---

## Build Modes

### Debug Build (development mode)

Debug mode enables development tooling:

- asset export hotkeys,
- IL2CPP diagnostics export,
- runtime hook scan/install helpers.

### Prod/Release Build (runtime mode)

Release mode keeps only core game communication/framework behavior and disables development controls.

---

## Generated Diagnostics

At startup, the framework exports a full consolidated signal snapshot:

- `Mods/ExportedAssets/Diagnostics/game-signals-full.txt`

This includes:

- event/trigger catalog,
- gameplay-relevant index from `il2cpp-unpack`,
- discovered hook candidates.

---

## How to Build Your Own Mod on Top

### 1) Subscribe to framework events

Use the central hub in your mod code:

- `ModFramework.Events.Subscribe<ModInitializedEvent>(...)`
- `ModFramework.Events.Subscribe<HookTriggeredEvent>(...)`
- `ModFramework.Events.Subscribe<ModErrorEvent>(...)`

### 2) Add your custom logic

Implement your own handlers/services for:

- feature toggles,
- gameplay automation,
- UI extensions,
- content replacement/override workflows.

### 3) Keep modules isolated

Recommended pattern:

- `FeatureXService` (logic)
- `FeatureXHooks` (runtime hooks)
- `FeatureXEvents` (event models)

---

## Safety Recommendations

- Wrap runtime loops and hook callbacks in `try-catch`.
- Log errors through framework events (`ModErrorEvent`).
- Keep hooks selective and avoid patching high-frequency methods blindly.

---

## Troubleshooting

- If the mod is not loaded, check `MelonLoader/Latest.log` first.
- If diagnostics are missing, verify write permissions for the game directory.
- If hook installation fails, inspect `hook-install-errors.txt` in diagnostics output.

---

## Summary

`Frikadelle Modding Framework` can be used as an all-in-one base framework for Data Center modding:

- core game communication,
- diagnostics and discovery,
- centralized event-driven extension model.
