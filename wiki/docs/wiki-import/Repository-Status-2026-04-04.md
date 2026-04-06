---
title: Repository Status 2026-04-04
description: File-by-file implementation snapshot mirrored from current repository state.
sidebar_position: 34
tags:
  - audience:contributor
  - area:status
---

## Repository Status (2026-04-04)

This page mirrors the current implementation state and highlights partial or pending areas.

## Core runtime (`FrikaMF`)

| File | Status | Summary |
| --- | --- | --- |
| `Core.cs` | done | lifecycle orchestration, logging, compat runtime initialization |
| `FfiBridge.cs` | done | Rust mod loading and event forwarding |
| `GameApi.cs` | done | API table surface exported to native mods |
| `EventDispatcher.cs` | done | event marshalling to native mods |
| `HarmonyPatches.cs` | done | gameplay patch surface |
| `Hooker.cs` | done | scan/catalog hook installation |
| `FfmLangserverCompat.cs` | partial | adapter discovery + diagnostics + conflict warnings (no strict lock mode yet) |
| `HireRosterService.cs` | done | exports framework hire snapshot (`available-hires.json`) |
| `LocalisationBridge.cs` | done | custom localization resolver registry + hook bridge |
| `PluginSyncService.cs` | partial | central plugin sync polling/downloading baseline |
| `MultiplayerBridge.cs` | partial | bridge available, not full feature-complete for production multiplayer parity |
| `DC2WebBridge.cs` | done | web-style UI adapter pipeline |

## Standalone mods

See: [Standalone Mods](StandaloneMods)

## Not fully implemented yet

- Strict hook ownership arbitration mode.
- End-to-end language server compatibility SDK package.
- Fully complete 16-player backend implementation for production use.

## Roadmap linkage

- Active roadmap: [ROADMAP](ROADMAP)
- Active task list: [TASKLIST](TASKLIST)
