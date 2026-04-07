---
id: repo-inventory
title: Repository inventory
sidebar_label: Repo inventory
description: Current monorepo layout, projects, and known solution drift (contributors).
---

# Repository inventory

This page is the **source of truth snapshot** for how the DataCenterExporter / gregFramework monorepo is organized today. Use it before large refactors or when onboarding.

## Top-level areas

| Area | Path | Role |
|------|------|------|
| Framework core | [`framework/FrikaMF.csproj`](https://github.com/mleem97/gregFramework/blob/master/framework/FrikaMF.csproj) | MelonLoader mod hosting runtime hooks, Harmony, bridge, events |
| Target layout / registry | [`FrikaModFramework/`](https://github.com/mleem97/gregFramework/tree/master/FrikaModFramework) | `fmf_hooks.json`, bindings stubs, migration docs |
| Workshop tooling | [`workshopuploader/`](https://github.com/mleem97/gregFramework/tree/master/workshopuploader) (rename from `WorkshopUploader/`; see `WorkshopUploader/MIGRATION_PUBLIC_REPO.md`) | Steam Workshop / workspace uploader — **.NET MAUI** (Windows) |
| MCP (LLM / IDE) | [`mcp-server/`](https://github.com/mleem97/gregFramework/tree/master/mcp-server) | Model Context Protocol over docs + `fmf_hooks.json`; Docker: `docker compose up docs-mcp` |
| Mods (sources) | [`mods/`](https://github.com/mleem97/gregFramework/tree/master/mods) | Gameplay mods (`FMF.*`, `FMF.Mod.*` folders) |
| Plugins (sources) | [`plugins/`](https://github.com/mleem97/gregFramework/tree/master/plugins) | Framework plugins (`FFM.Plugin.*`) |
| Templates | [`Templates/`](https://github.com/mleem97/gregFramework/tree/master/Templates) | Scaffolds for new mods/plugins |
| Documentation content | [`docs/`](https://github.com/mleem97/gregFramework/tree/master/docs) | Markdown/MDX sources for the wiki |
| Documentation site (Docusaurus) | [`wiki/`](https://github.com/mleem97/gregFramework/tree/master/wiki) | App shell, theme, `npm run build` |
| Scripts | [`scripts/`](https://github.com/mleem97/gregFramework/tree/master/scripts) | Release metadata, changelog (e.g. `Update-ReleaseMetadata.ps1`) |
| Wiki import (legacy) | [`docs/wiki-import/`](./../wiki-import/Home.md) | Imported `.wiki` content; still linked from many pages |

## .NET projects on disk (`*.csproj`)

| Project | Location | In `FrikaMF.sln`? |
|---------|----------|-------------------|
| FrikaMF | `framework/FrikaMF.csproj` | Yes |
| WorkshopUploader | `workshopuploader/WorkshopUploader.csproj` (after folder rename) | No — use `WorkshopUploader.sln` in that folder |
| FFM.Plugin.* (x5) | `plugins/FFM.Plugin.*/` | Yes — paths in [`FrikaMF.sln`](https://github.com/mleem97/gregFramework/blob/master/FrikaMF.sln) use `plugins\...` |
| FMF.HexLabelMod | `mods/FMF.Mod.HexLabelMod/` | No (build standalone or add to solution) |
| FMF.ConsoleInputGuard | `mods/FMF.ConsoleInputGuard/` | No |
| FMF.GregifyEmployees | `mods/FMF.Mod.GregifyEmployees/` | No |
| FMF.JoniMLCompatMod | `mods/FMF.Plugin.LangCompatBridge/` | No |
| Templates | `Templates/FMF.*`, `Templates/StandaloneModTemplate/` | No |

## Build status (framework project)

- `framework/FrikaMF.csproj` explicitly **excludes** `workshopuploader/**` from compile (that app builds only via `workshopuploader/WorkshopUploader.csproj` / `WorkshopUploader.sln` in that folder).
- `dotnet build FrikaMF.sln` builds framework and plugin projects under `plugins\` — **not** the MAUI Workshop app (MelonLoader/game refs still required locally unless `CI=true`).

## `FrikaMF.sln` drift (action items)

1. **Mods not in solution**: Standalone mod projects under `mods/` are intentionally omitted from the solution to keep the graph small; add them if you want `dotnet build` for every module in one shot.

2. **Templates in `framework/FrikaMF.csproj`**: Template sources under `Templates/` may fail `dotnet build framework/FrikaMF.csproj` with `CS0122` if `Core` visibility does not match template expectations — treat templates as **samples** until the project graph is cleaned up.

## Documentation (Docusaurus)

- **Entry**: `/wiki` → [`intro`](../intro.md)
- **Sidebar**: [`sidebars.js`](https://github.com/mleem97/gregFramework/blob/master/wiki/sidebars.js)
- **Module catalog** (downloads table): [`wiki/src/data/moduleCatalog.ts`](https://github.com/mleem97/gregFramework/blob/master/wiki/src/data/moduleCatalog.ts)
- **Landing**: `/` → [`src/pages/index.tsx`](https://github.com/mleem97/gregFramework/blob/master/wiki/src/pages/index.tsx)
- **Static catalog page**: `/mods`

## Hook / event sources of truth (code)

- String constants: [`framework/FrikaMF/HookNames.cs`](https://github.com/mleem97/gregFramework/blob/master/framework/FrikaMF/HookNames.cs) (`FFM.*` hook IDs today).
- Numeric IDs: [`framework/FrikaMF/EventIds.cs`](https://github.com/mleem97/gregFramework/blob/master/framework/FrikaMF/EventIds.cs).
- Generated wiki mirror: run [`tools/Generate-FmfHookCatalog.ps1`](https://github.com/mleem97/gregFramework/blob/master/tools/Generate-FmfHookCatalog.ps1) → [`fmf-hooks-catalog`](../reference/fmf-hooks-catalog.md).

## Debugging (MelonLoader, IL2CPP, Unity)

- **Build first:** `dotnet build FrikaMF.sln -c Debug` (requires MelonLoader + IL2CPP interop under `MelonLoader/` for your game install, or `lib/references/MelonLoader` — see `tools/refresh_refs.py`).
- **Attach:** Run **Data Center** with MelonLoader, then attach your IDE’s **.NET / CoreCLR** debugger to the game process (process name usually matches the game executable). Breakpoints hit in **Debug** builds of mods/plugins copied into `Mods/`.
- **`FFM.Plugin.AssetExporter`:** The project links `framework/Main.cs` (and related files) **and** references `FrikaMF.csproj`, which would normally produce **CS0436** duplicate-type warnings. Those are **suppressed** in the plugin `.csproj` (`NoWarn`); do not remove the project reference without linking the rest of the `FrikaMF` sources.

## Related

- [Monorepo target layout](./monorepo-target-layout.md) — phased folder goals
- [FMF hook naming](../reference/fmf-hook-naming.md) — naming convention
- [Release channels](../reference/release-channels.md) — Steam vs GitHub beta
