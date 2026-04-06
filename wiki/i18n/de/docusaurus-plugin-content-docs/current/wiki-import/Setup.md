# Setup

Last updated: 2026-04-03

This page documents the current development and runtime setup for `FrikaModFramework`.

## Prerequisites

- `Data Center` installed locally
- `MelonLoader` installed for the game
- `.NET 6 SDK`
- Optional but recommended: decompiled sources under `il2cpp-unpack`

Before the first framework build, run the game once with MelonLoader. This generates runtime metadata required for stable development.

## Quick setup (maintainer path)

```powershell
dotnet restore .\framework\framework/FrikaMF.csproj
dotnet build .\framework\framework/FrikaMF.csproj -c Debug -nologo
dotnet build .\framework\framework/FrikaMF.csproj -c Release -nologo
```

If your game is not installed in the default path, set the game directory explicitly:

```powershell
dotnet build .\framework\framework/FrikaMF.csproj -c Debug -p:GameDir="D:\Games\Data Center"
```

Alternative via environment variable:

```powershell
$env:DATA_CENTER_GAME_DIR = "D:\Games\Data Center"
dotnet build .\framework\framework/FrikaMF.csproj -c Debug
```

Important: after installing MelonLoader, launch the game once so `MelonLoader\Il2CppAssemblies` is generated.

Optional deploy shortcut:

```powershell
. .\scripts\Invoke-DataCenterModDeploy.ps1
Invoke-Deploy --all
```

## Runtime folder layout (important)

- C# framework/mod DLLs: `Data Center/Mods`
- Rust/native plugin DLLs: `Data Center/Mods/RustMods`
- Content/object packs: `Data Center/Data Center_Data/StreamingAssets/Mods/<PackName>`

## Build outputs

- Framework: `bin/Release/net6.0/FrikaModdingFramework.dll`
- Label mod (optional): `HexLabelMod/bin/Release/net6.0/HexLabelMod.dll`

Install by copying those DLLs into the game `Mods` folder, then verify load in `MelonLoader/Latest.log`.

## Content pack scaffold

Create a new object/content pack folder:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\New-StreamingAssetModPack.ps1 -GamePath "C:\Program Files (x86)\Steam\steamapps\common\Data Center" -ModName "MyServerPack"
```

Generated baseline files:

- `config.json`
- `model.obj`
- `model.mtl`
- `texture.png`
- `icon.png`

## Release/versioning workflow

Framework releases use `XX.XX.XXXX` format and `FrikaMF/ReleaseVersion.cs` as source of truth.

Use only:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump major
pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump medium
pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump minor
```

For local release uploads:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\Publish-LocalRelease.ps1
```

## Wiki maintenance

Wiki source is stored in `.wiki/` and synced via:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\Sync-Wiki.ps1
```

## Troubleshooting

- If DLL does not load: inspect `MelonLoader/Latest.log` first.
- If diagnostics are missing: verify game folder write permission.
- If hook install fails: check diagnostics output and `.wiki/HOOKS.md` verification status.
