---
title: Standalone Plugins & Mods
sidebar_label: Overview
---

This section contains the first-party standalone modules currently maintained in this repository.

## Classification

- **Plugins** extend the framework runtime capabilities.
- **Mods** extend the game behavior/content by using framework and plugin capabilities.

## Loading path

- Both plugin and mod DLLs are discovered from `Gamefolder/Plugins`.
- Rust mods are also loaded from `Gamefolder/Plugins` when used through FrikaMF.

## Standalone Plugins

- [FFM.Plugin.AssetExporter](./ffm-plugin-asset-exporter.md)
- [FFM.Plugin.Multiplayer](./ffm-plugin-multiplayer.md)
- [FFM.Plugin.PlayerModels](./ffm-plugin-player-models.md)
- [FFM.Plugin.Sysadmin](./ffm-plugin-sysadmin.md)
- [FFM.Plugin.WebUIBridge](./ffm-plugin-web-ui-bridge.md)

## Standalone Mods

- [FMF.ConsoleInputGuard](./fmf-console-input-guard.md)
- [FMF.GregifyEmployees](./fmf-gregify-employees.md)
- [FMF.HexLabelMod](./fmf-hex-label-mod.md)
- [FMF.LangCompatBridge](./fmf-lang-compat-bridge.md)
- [FMF.UIReplacementMod](./fmf-ui-replacement-mod.md)

## Additional context

- [`Standalone Mods` wiki page](/wiki/wiki-import/StandaloneMods)
