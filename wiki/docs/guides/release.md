---
id: release
title: Release
sidebar_label: Release
description: Current release artifacts, version matrix, and download instructions for FrikaModFramework.
sidebar_position: 30
tags:
  - release
---

# Release

## Release artifacts

The following components are built and packaged for Steam Workshop distribution:

### Core Framework

| Component | Assembly | Workshop content path | Tags |
|-----------|----------|----------------------|------|
| FrikaModFramework | `FrikaModdingFramework.dll` | `content/Mods/` | modded, melonloader, framework, fmf |

### FMF Plugins

| Plugin | Assembly | Workshop content path | Tags |
|--------|----------|----------------------|------|
| FFM.Plugin.Multiplayer | `FFM.Plugin.Multiplayer.dll` | `content/FMF/Plugins/` | modded, fmf, plugin |
| FFM.Plugin.Sysadmin | `FFM.Plugin.Sysadmin.dll` | `content/FMF/Plugins/` | modded, fmf, plugin |
| FFM.Plugin.AssetExporter | `FFM.Plugin.AssetExporter.dll` | `content/FMF/Plugins/` | modded, fmf, plugin |
| FFM.Plugin.WebUIBridge | `FFM.Plugin.WebUIBridge.dll` | `content/FMF/Plugins/` | modded, fmf, plugin |
| FFM.Plugin.PlayerModels | `FFM.Plugin.PlayerModels.dll` | `content/FMF/Plugins/` | modded, fmf, plugin |

### Gameplay Mods

| Mod | Assembly | Workshop content path | Tags |
|-----|----------|----------------------|------|
| FMF.ConsoleInputGuard | `FMF.ConsoleInputGuard.dll` | `content/Mods/` | modded, melonloader, mod |
| FMF.GregifyEmployees | `FMF.GregifyEmployees.dll` | `content/Mods/` | modded, melonloader, mod |
| FMF.HexLabelMod | `FMF.HexLabelMod.dll` | `content/Mods/` | modded, melonloader, mod |
| FMF.JoniMLCompatMod | `FMF.JoniMLCompatMod.dll` | `content/Mods/` | modded, melonloader, mod |

### Gregtools Modmanager (WorkshopManager)

| Version | Component | Target | Description |
|---------|-----------|--------|-------------|
| **1.0** | Gregtools Modmanager | `net9.0-windows` (win10-x64), self-contained | Steam Workshop client + Mod Store + Mod Manager. No .NET runtime required. See [GregTools 1.0 Modmanager release](/wiki/releases/tools/gregtools-modmanager-1.0-release). |

## Installation for end users

### Quick start

1. Install **MelonLoader** (IL2CPP) for Data Center.
2. Start the game once, then close it.
3. Subscribe to mods via the **WorkshopManager** Mod Store or the [Steam Workshop](https://steamcommunity.com/app/4170200/workshop/).
4. Start the game.

### Manual installation

1. Download the DLL from the Workshop or a release.
2. Framework: place `FrikaModdingFramework.dll` in `<Data Center>/Mods/`.
3. Plugins: place `FFM.Plugin.*.dll` in `<Data Center>/FMF/Plugins/`.
4. Mods: place `FMF.*.dll` in `<Data Center>/Mods/`.
5. Start the game and check `MelonLoader/Latest.log`.

## Game directory structure

```text
Data Center/
├── Mods/
│   ├── FrikaModdingFramework.dll
│   ├── FMF.ConsoleInputGuard.dll
│   ├── FMF.GregifyEmployees.dll
│   ├── FMF.HexLabelMod.dll
│   └── FMF.JoniMLCompatMod.dll
├── FMF/
│   └── Plugins/
│       ├── FFM.Plugin.Multiplayer.dll
│       ├── FFM.Plugin.Sysadmin.dll
│       ├── FFM.Plugin.AssetExporter.dll
│       ├── FFM.Plugin.WebUIBridge.dll
│       └── FFM.Plugin.PlayerModels.dll
├── MelonLoader/
│   └── Latest.log
├── UserData/
│   └── ModCfg/
└── workshop/
    ├── FrikaModFramework/
    │   ├── content/Mods/FrikaModdingFramework.dll
    │   └── metadata.json
    ├── FFM.Plugin.Multiplayer/
    │   ├── content/FMF/Plugins/FFM.Plugin.Multiplayer.dll
    │   └── metadata.json
    ├── Gregtools Modmanager/
    │   ├── content/   (self-contained WorkshopUploader.exe + dependencies)
    │   └── metadata.json
    └── ... (one folder per component)
```

## Build and deploy (contributors)

```bash
# Build everything and package into Workshop folders
pwsh -File scripts/Deploy-Release-ToWorkshop.ps1

# Deploy to game Mods/ and FMF/Plugins/ for local testing
pwsh -File scripts/Deploy-Release-ToDataCenter.ps1
```

See also: [Contributor Guide](./contributor-workshop)

## Compatibility

| Component | Requires |
|-----------|----------|
| All mods/plugins | Data Center (Steam App 4170200) |
| All mods/plugins | MelonLoader (IL2CPP, stable) |
| FMF Plugins | FrikaModdingFramework.dll in Mods/ |
| Gregtools Modmanager | Windows 10 1809+ (self-contained, no runtime install needed) |
