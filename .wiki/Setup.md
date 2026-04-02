# Setup

This page describes the current local setup for `FrikaModFramework`.

## Requirements

- `Data Center` installed
- `MelonLoader` installed for the game
- `.NET 6 SDK`
- Optional: decompiled sources in `il2cpp-unpack`

## Repository Structure (current)

- `FrikaMF.csproj` and `FrikaMF.sln` for the framework mod
- `HexLabelMod/HexLabelMod.csproj` for the standalone label mod
- `.wiki/` for project wiki pages

## Build Commands

Use PowerShell from repository root:

```powershell
dotnet build .\FrikaMF.csproj -c Debug
dotnet build .\FrikaMF.csproj -c Release
dotnet build .\HexLabelMod\HexLabelMod.csproj -c Release
```

Optional deploy helper:

```powershell
. .\scripts\Invoke-DataCenterModDeploy.ps1
Invoke-Deploy --1   # framework only
Invoke-Deploy --2   # HexLabelMod only
Invoke-Deploy --all # both
```

Local release upload (maintainer flow):

```powershell
. .\scripts\Publish-LocalRelease.ps1
$env:GITHUB_TOKEN = "<github_token_with_repo_scope>"
Publish-LocalRelease -Tag "vX.Y.Z"
```

## Runtime Placement

- Copy framework output (`DataCenterModLoader.dll`) into game `Mods` folder
- Copy `HexLabelMod.dll` into game `Mods` folder

## Notes

- `HexLabelMod` is configured for release-only builds.
- Keep game paths in `.csproj` aligned with your local Steam installation.
- Validate load state through `MelonLoader/Latest.log`.
