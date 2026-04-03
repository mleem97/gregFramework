# FrikaModFramework Wiki

Last updated: 2026-04-03

Welcome to the official wiki for `FrikaModFramework`.

## What this project provides

- A MelonLoader-based runtime framework for `Data Center`.
- Stable C# hook points and normalized event dispatch for mod authors.
- A native bridge interface for Rust plugin integration.
- Optional diagnostics and hook catalog exports to reduce update pain after game patches.

## Start here (new contributors)

1. Open [Setup](Setup) and complete prerequisites.
2. Build once and verify mod load in `MelonLoader/Latest.log`.
3. Read [Architecture](Architecture) to understand runtime boundaries.
4. Follow [Modding Guide](Modding-Guide) for hooks/events workflow.
5. Use [Device Reference](Device-Reference) for gameplay device details and current known limits.

## Common goals

- Add new gameplay hooks in `FrikaMF/JoniMF/HarmonyPatches.cs`.
- Expand event coverage in `FrikaMF/JoniMF/EventIds.cs` and `FrikaMF/JoniMF/EventDispatcher.cs`.
- Extend game-callable API table in `FrikaMF/JoniMF/GameApi.cs`.
- Improve docs with verified, reproducible device and hook information.

## Required references

- Repository: `https://github.com/mleem97/FrikaModFramework`
- Rust bridge reference: `https://github.com/Joniii11/DataCenter-RustBridge`
- Verified hook table: `HOOKS.md` (repo root)

## Wiki sync

Use repo-local source pages under `.wiki/`, then sync them to the GitHub wiki:

- PowerShell: `pwsh -ExecutionPolicy Bypass -File .\scripts\Sync-Wiki.ps1`
- cmd: `powershell -ExecutionPolicy Bypass -File .\scripts\Sync-Wiki.ps1`
- bash/sh: `pwsh -ExecutionPolicy Bypass -File ./scripts/Sync-Wiki.ps1`

Optional wrappers:

- `npm run wiki:sync`
- `pnpm wiki:sync`
