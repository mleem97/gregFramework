# Architecture

This page documents the current high-level architecture.

## Core Projects

- `FrikaMF`: main framework mod and event/hook infrastructure
- `HexLabelMod`: separate standalone mod for in-game hex labels

## `FrikaMF` Responsibilities

- Runtime integration with MelonLoader and IL2CPP
- Runtime event bridge (`JoniML/EventIds.cs`, `JoniML/EventDispatcher.cs`)
- Harmony hook sources (`JoniML/HarmonyPatches.cs`)
- Native plugin loading and event forwarding (`JoniML/FfiBridge.cs`)
- API function table for external mods (`JoniML/GameApi.cs`)
- Optional tooling around runtime discovery and debugging

## `HexLabelMod` Responsibilities

- Adds white hex labels for cable-related objects
- Reads configurable positions/sizes from `hexposition.cfg`
- Supports live reload loop (interval-based)
- Uses Input System-compatible hotkey handling for toggle actions

## Runtime Data and Docs

- Diagnostics files under `Mods/ExportedAssets/Diagnostics`
- Wiki pages under `.wiki/`
- Main user docs in `README.md` and `README_MODDING.md`

## Design Notes

- Keep mods isolated per project to avoid unintended coupling.
- Prefer explicit config files for visual tuning.
- Keep hook/event targeting focused on `Assembly-CSharp`.
- Keep API and event contracts append-only for ABI stability.
