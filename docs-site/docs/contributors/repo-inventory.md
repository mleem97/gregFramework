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
| Framework core | [`FrikaMF.csproj`](https://github.com/mleem97/gregFramework/blob/master/FrikaMF.csproj) (root) | MelonLoader mod hosting runtime hooks, Harmony, bridge, events |
| Workshop tooling | [`WorkshopUploader/`](https://github.com/mleem97/gregFramework/tree/master/WorkshopUploader) | Steam Workshop upload helper (local/CI) |
| Mods (sources) | [`mods/`](https://github.com/mleem97/gregFramework/tree/master/mods) | Gameplay mods (`FMF.*`, `FMF.Mod.*` folders) |
| Plugins (sources) | [`plugins/`](https://github.com/mleem97/gregFramework/tree/master/plugins) | Framework plugins (`FFM.Plugin.*`) |
| Templates | [`Templates/`](https://github.com/mleem97/gregFramework/tree/master/Templates) | Scaffolds for new mods/plugins |
| Documentation site | [`docs-site/`](https://github.com/mleem97/gregFramework/tree/master/docs-site) | Docusaurus wiki, landing, `/mods` catalog |
| Scripts | [`scripts/`](https://github.com/mleem97/gregFramework/tree/master/scripts) | Release metadata, changelog (e.g. `Update-ReleaseMetadata.ps1`) |
| Wiki import (legacy) | [`docs-site/docs/wiki-import/`](./../wiki-import/Home.md) | Imported `.wiki` content; still linked from many pages |

## .NET projects on disk (`*.csproj`)

| Project | Location | In `FrikaMF.sln`? |
|---------|----------|-------------------|
| FrikaMF | `./FrikaMF.csproj` | Yes |
| WorkshopUploader | `WorkshopUploader/WorkshopUploader.csproj` | Yes |
| FFM.Plugin.* (x5) | `plugins/FFM.Plugin.*/` | Yes — paths in [`FrikaMF.sln`](https://github.com/mleem97/gregFramework/blob/master/FrikaMF.sln) use `plugins\...` |
| FMF.HexLabelMod | `mods/FMF.Mod.HexLabelMod/` | No (build standalone or add to solution) |
| FMF.ConsoleInputGuard | `mods/FMF.ConsoleInputGuard/` | No |
| FMF.GregifyEmployees | `mods/FMF.Mod.GregifyEmployees/` | No |
| FMF.JoniMLCompatMod | `mods/FMF.Plugin.LangCompatBridge/` | No |
| Templates | `Templates/FMF.*`, `Templates/StandaloneModTemplate/` | No |

## Build status (framework project)

- `FrikaMF.csproj` explicitly **excludes** `WorkshopUploader/**` from compile (that app builds only via `WorkshopUploader/WorkshopUploader.csproj` / solution).
- `dotnet build FrikaMF.sln` builds framework, `WorkshopUploader`, and plugin projects under `plugins\` (MelonLoader/game refs still required locally unless `CI=true`).

## `FrikaMF.sln` drift (action items)

1. **Mods not in solution**: Standalone mod projects under `mods/` are intentionally omitted from the solution to keep the graph small; add them if you want `dotnet build` for every module in one shot.

2. **Templates in `FrikaMF.csproj`**: Template sources under `Templates/` may fail `dotnet build FrikaMF.csproj` with `CS0122` if `Core` visibility does not match template expectations — treat templates as **samples** until the project graph is cleaned up.

3. **WorkshopUploader vs `FrikaMF.csproj`**: Building the framework project alone can still pick up adjacent WinForms sources — prefer `dotnet build FrikaMF.sln` or isolate `WorkshopUploader` in CI.

## Documentation (Docusaurus)

- **Entry**: `/wiki` → [`intro`](../intro.md)
- **Sidebar**: [`sidebars.js`](https://github.com/mleem97/gregFramework/blob/master/docs-site/sidebars.js)
- **Module catalog** (downloads table): [`docs-site/src/data/moduleCatalog.ts`](https://github.com/mleem97/gregFramework/blob/master/docs-site/src/data/moduleCatalog.ts)
- **Landing**: `/` → [`src/pages/index.tsx`](https://github.com/mleem97/gregFramework/blob/master/docs-site/src/pages/index.tsx)
- **Static catalog page**: `/mods`

## Hook / event sources of truth (code)

- String constants: [`FrikaMF/HookNames.cs`](https://github.com/mleem97/gregFramework/blob/master/FrikaMF/HookNames.cs) (`FFM.*` hook IDs today).
- Numeric IDs: [`FrikaMF/EventIds.cs`](https://github.com/mleem97/gregFramework/blob/master/FrikaMF/EventIds.cs).
- Generated wiki mirror: run [`tools/Generate-FmfHookCatalog.ps1`](https://github.com/mleem97/gregFramework/blob/master/tools/Generate-FmfHookCatalog.ps1) → [`fmf-hooks-catalog`](../reference/fmf-hooks-catalog.md).

## Related

- [Monorepo target layout](./monorepo-target-layout.md) — phased folder goals
- [FMF hook naming](../reference/fmf-hook-naming.md) — naming convention
- [Release channels](../reference/release-channels.md) — Steam vs GitHub beta
