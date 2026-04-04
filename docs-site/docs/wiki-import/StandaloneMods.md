---
title: Standalone Mods
description: Complete standalone mod inventory, dependency contract, and development status.
sidebar_position: 32
tags:
  - audience:moddev
  - area:standalone
  - area:runtime
---

# Standalone Mods

This page is the source of truth for all standalone mods under `StandaloneMods/`.

## Mandatory dependency contract

All standalone mods in this repository must be runtime-dependent on `FrikaModdingFramework.dll`.

Required behavior:

- Detect framework assembly (`FrikaModdingFramework` / `FrikaMF`) during initialization.
- Log a clear error and disable mod behavior if the dependency is missing.
- Never hard-crash gameplay when dependency is absent.

## Inventory

### `StandaloneMods/FMF.UIReplacementMod`

Purpose:

- Modern UI replacement via FMF `DC2WebBridge`.
- React/Vite authoring workflow and runtime asset export.
- Optional Discord Rich Presence bridge.
- Live UI reload support.

Key files:

- `Main.cs`: mod entrypoint + runtime checks + update loop.
- `ReactUiRuntime.cs`: bridge registration/apply and asset pipeline.
- `RuntimeOptions.cs`: runtime config load/store.
- `DiscordRichPresenceRuntime.cs`: optional Discord integration.
- `react-ui/*`: source workspace for UI templates.

Framework dependency status:

- Runtime dependency check: **implemented**.

### `StandaloneMods/FMF.HexLabelMod`

Purpose:

- UI hex labels for cables/racks with live config reload.

Key files:

- `Main.cs`: mod entrypoint, harmony patches, label behavior.
- `FMF.HexLabelMod.csproj`: standalone build setup.

Framework dependency status:

- Runtime dependency check: **implemented**.

### `StandaloneMods/FMF.JoniMLCompatMod`

Purpose:

- Compatibility migration target replacing legacy root-level `JoniML` runtime coupling.

Key files:

- `CompatMain.cs`: compatibility entrypoint.
- `FMF.JoniMLCompatMod.csproj`: standalone build setup.

Framework dependency status:

- Runtime dependency check: **implemented**.

### `StandaloneMods/FMF.GregifyEmployees`

Purpose:

- Replaces employee visuals with Greg baseline (employee 1).
- Replaces employee portrait/card images with `image.png`.
- Registers premium hire `RGB Greg` with animated color/emission overlay.
- Continuously applies replacement so mod-added employees are also affected.

Framework dependency status:

- Runtime dependency check: **implemented**.

## File-by-file implementation status

| Area | File | Status | Notes |
| --- | --- | --- | --- |
| UI replacement | `StandaloneMods/FMF.UIReplacementMod/Main.cs` | done | framework-gated runtime entrypoint |
| UI replacement | `StandaloneMods/FMF.UIReplacementMod/ReactUiRuntime.cs` | done | bridge registration + apply pipeline |
| UI replacement | `StandaloneMods/FMF.UIReplacementMod/RuntimeOptions.cs` | done | config persistence |
| UI replacement | `StandaloneMods/FMF.UIReplacementMod/DiscordRichPresenceRuntime.cs` | partial | optional reflection-based runtime (depends on DiscordRPC assembly) |
| Hex labels | `StandaloneMods/FMF.HexLabelMod/Main.cs` | done | framework dependency + harmony patch flow |
| Joni compat | `StandaloneMods/FMF.JoniMLCompatMod/CompatMain.cs` | done | migration compatibility entrypoint |
| Gregify employees | `StandaloneMods/FMF.GregifyEmployees/Main.cs` | done | greg model/image replacement + RGB Greg hire logic |
| Gregify employees | `StandaloneMods/FMF.GregifyEmployees/image.png` | done | portrait source asset |

## Known gaps

- Hard arbitration for multi-owner Harmony conflicts is not enabled yet; current mode is diagnostics + warning.
- Cross-language adapter manifests are discovered, but no strict claim lock protocol is enforced yet.

## Next implementation targets

1. Add optional strict conflict mode for hook claim collisions.
2. Add language adapter SDK examples (Rust/Python/Lua/Delphi manifest templates).
3. Add automated contract tests for framework dependency checks across standalone mods.
