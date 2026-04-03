---
title: Release Assets and Templates
description: Release artifact contract including DLL uploads and modder-ready ZIP bundles.
sidebar_position: 33
tags:
  - audience:moddev
  - area:release
  - area:templates
---

# Release Assets and Templates

This page defines what must be published in each release.

## Required release assets

### DLL assets

- `FrikaModdingFramework.dll`
- `FMF.UIReplacementMod.dll`
- `FMF.HexLabelMod.dll`
- `FMF.JoniMLCompatMod.dll`

### ZIP bundle for modders

Required file name pattern:

- `FrikaModFramework-ModdingBundle-v{XX.XX.XXXX}.zip`

Required ZIP content:

- `Templates/StandaloneModTemplate/*`
- `Templates/UiTemplate/react-ui/*`
- `Templates/FFM.Langserver.Compat/*`
- `StandaloneMods/FMF.UIReplacementMod/FMF.UIReplacementMod/react-app.*`
- `.wiki/*` (documentation for offline consumption)
- `scripts/New-StreamingAssetModPack.ps1`
- `scripts/Invoke-DataCenterModDeploy.ps1`

Optional content:

- generated release manifest with checksums
- sample content packs for `StreamingAssets/Mods`

## Workflow source

- `.github/workflows/release-assets.yml`

## Validation checklist

1. Build all required projects in `Release`.
2. Verify all four DLL outputs exist.
3. Verify all template directories exist.
4. Build bundle ZIP and include in release upload.
5. Verify release notes mention DLLs + ZIP bundle.
