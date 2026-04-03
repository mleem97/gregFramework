# Architecture

This page documents the current high-level architecture.

## Core Projects

- `FrikaMF`: main framework mod and event/hook infrastructure
- `HexLabelMod`: separate standalone mod for in-game hex labels

## `FrikaMF` Responsibilities

- Runtime integration with MelonLoader and IL2CPP
- Runtime event bridge (`FrikaMF/JoniMF/EventIds.cs`, `FrikaMF/JoniMF/EventDispatcher.cs`)
- Harmony hook sources (`FrikaMF/JoniMF/HarmonyPatches.cs`)
- Native plugin loading and event forwarding (`FrikaMF/JoniMF/FfiBridge.cs`)
- API function table for external mods (`FrikaMF/JoniMF/GameApi.cs`)
- Optional tooling around runtime discovery and debugging

## Runtime Content/Code Separation

- C# framework/mod DLLs: `Data Center/Mods`
- Rust/native plugin DLLs: `Data Center/Mods/RustMods`
- Game object content packs: `Data Center/Data Center_Data/StreamingAssets/Mods`

This enables users to add new object packs (for example server/switch assets) without additional helper mods, as long as the game-side `ExampleMod` schema is respected.

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
