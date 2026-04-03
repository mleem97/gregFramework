# FMF UI Replacement Mod

Standalone MelonLoader mod that performs a full modern UI replacement pipeline for `Data Center` using FMF `DC2WebBridge` with React-style source assets.

It supports live asset reload, build-time React export, configurable multiplayer target size, and optional Discord Rich Presence hooks.

## Other project reference

- `DataCenter_DHCPSwitches`: <https://github.com/mleem97/DataCenter_DHCPSwitches>

## FFM language compatibility hook

This mod can co-exist with language adapters discovered by the framework from:

- `Data Center/Mods/FFM.Langserver.Compat`

Use adapter manifests there to coordinate hook claims between runtimes and avoid silent hook collisions.

## Dependency

This mod requires **FrikaModdingFramework (FMF)** to be loaded.

- Required framework assembly: `FrikaModdingFramework.dll`
- Runtime check: verifies `DataCenterModLoader.Core` from `FrikaModdingFramework`
- If dependency is missing, the mod logs an error and stays inactive

## Features

- Scene-wide React-style UI replacement descriptors for key screens (`MainMenu`, `HRSystem`, `ComputerShop`) plus global canvas fallback
- Animated modern style tokens (fade/slide/aurora effects in CSS) exported into FMF profile variables
- Auto-refresh pass every ~2 seconds to style newly created UI roots
- Live reload when `react-app.html/.css/.tsx` files change (no game restart required)
- In-game status banner
- Hotkeys:
  - `Ctrl+U`: toggle UI replacement on/off
  - `Ctrl+Shift+U`: force immediate restyle + reload React assets

## UI asset locations (GameRoot-first)

The mod resolves UI assets in this order:

1. `GameRoot/UI/FMF.UIReplacementMod/`
2. `GameRoot/FrikaFramework/UI/FMF.UIReplacementMod/`
3. Fallback: `GameRoot/Mods/FMF.UIReplacementMod/`

This matches your requirement to keep UI mods under `GameRoot/UI` or `GameRoot/FrikaFramework/UI`.

## React UI source workspace

React source files are in:

`react-ui/`

Main files:

- `react-ui/src/App.tsx`
- `react-ui/src/styles.css`
- `react-ui/scripts/export-to-mod.mjs`

Build and export:

```powershell
Set-Location .\StandaloneMods\FMF.UIReplacementMod\react-ui
pnpm install
pnpm build
pnpm export:mod
```

The export script writes runtime-consumed assets to:

`StandaloneMods/FMF.UIReplacementMod/FMF.UIReplacementMod/`

Files produced:

- `react-app.html`
- `react-app.css`
- `react-app.tsx`

## Build

From repository root:

```powershell
dotnet build .\StandaloneMods\FMF.UIReplacementMod\FMF.UIReplacementMod.csproj -c Release
```

Important: The project now auto-runs React build/export during `dotnet build`.

- `pnpm install`
- `pnpm build`
- `pnpm export:mod`

This means you can build once and copy the resulting DLL + UI payload directly.

If your game is not in default Steam location:

```powershell
dotnet build .\StandaloneMods\FMF.UIReplacementMod\FMF.UIReplacementMod.csproj -c Release /p:GameDir="C:\Path\To\Data Center"
```

Or set env variable:

```powershell
$env:DATA_CENTER_GAME_DIR = "C:\Path\To\Data Center"
dotnet build .\StandaloneMods\FMF.UIReplacementMod\FMF.UIReplacementMod.csproj -c Release
```

## Install

Copy both DLLs into the game `Mods` folder:

- `FrikaModdingFramework.dll`
- `FMF.UIReplacementMod.dll`

Copy the React asset folder too:

- `FMF.UIReplacementMod\react-app.html`
- `FMF.UIReplacementMod\react-app.css`
- `FMF.UIReplacementMod\react-app.tsx`

Expected path:

```text
Data Center\Mods\FrikaModdingFramework.dll
Data Center\Mods\FMF.UIReplacementMod.dll
Data Center\UI\FMF.UIReplacementMod\react-app.html
Data Center\UI\FMF.UIReplacementMod\react-app.css
Data Center\UI\FMF.UIReplacementMod\react-app.tsx
```

You can alternatively place these assets under:

```text
Data Center\FrikaFramework\UI\FMF.UIReplacementMod\
```

## Live update workflow (Vite / pnpm dev)

Use this dev loop:

```powershell
Set-Location .\StandaloneMods\FMF.UIReplacementMod\react-ui
pnpm dev
```

After edits, export files for runtime pickup:

```powershell
pnpm export:mod
```

Then copy/sync files into `GameRoot/UI/FMF.UIReplacementMod/` (or keep your sync script). The mod detects file changes and reapplies UI live.

## Runtime config

Config file is created at:

```text
GameRoot\FrikaFramework\fmf-ui-replacement.config.json
```

Fields:

- `EnableDiscordRichPresence` (bool)
- `DiscordClientId` (string)
- `MaxPlayers` (int, clamped 2..32; default 16)
- `EnableLiveUiReload` (bool)

## Multiplayer note (16 players)

`MaxPlayers` is exposed/configured here for your target session size. For network backend, prefer dedicated relay/server transport (e.g. LiteNetLib/KCP) while Steam lobby/event APIs in current FMF are still partial.

## Discord Rich Presence

The mod includes optional runtime Discord RPC integration hooks.

- If a compatible `DiscordRPC` assembly is present/loaded, presence updates are sent periodically.
- If not available, the mod logs a warning and continues without breaking gameplay.

## Notes

- This is a runtime UI skin replacement, not a UXML/asset bundle replacement pipeline.
- The mod focuses on robust broad-stroke visual replacement across Unity UI and TMP components.
