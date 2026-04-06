---
title: WorkshopManager
sidebar_label: WorkshopManager
description: Windows desktop app for managing Steam Workshop content, browsing mods, and publishing for Data Center (FrikaMF).
---

# WorkshopManager

**WorkshopManager** is a **.NET MAUI** desktop app for **Windows** that serves as a full-featured Steam Workshop client for *Data Center*. It combines content authoring (publish/update), a **Mod Store** for browsing and installing community mods, and a **Mod Manager** for dependency health checks.

## Features

### Author tools (Projects / Editor / My Uploads)
- Create workshop projects from templates (vanilla assets, MelonLoader mods, FMF plugins).
- Edit **title**, **description**, **visibility**, **tags**, and **preview image**; stored in `metadata.json`.
- **Save and upload to Steam** — saves metadata, publishes `content/` to Steam Workshop, then **syncs** your local folder with Steam's version (like a `git pull` after push).
- **Change notes** — when updating an existing item, describe what changed; shown on the Workshop changelog tab.
- **View on Steam** — open your published item in the browser.
- **Pagination** across all list views (My Uploads, Browse, Subscribed, Favorites).
- **Per-item stats** — subscriptions, votes, score, size.

### Mod Store (integrated in Mod Manager)
- **Browse** all Workshop items for Data Center with tag filtering and sort options (last updated, newest, top rated, trending, most subscribed, title A-Z).
- **Search** mods by text.
- **Subscribe / Unsubscribe** to mods directly from the store.
- **Favorites** — bookmark mods for quick access.
- **Vote** (up/down) on workshop items.
- **Item detail view** — full statistics, description, tags, action buttons, and links to changelog/comments.

### Mod Manager / Health
- **Dependency health** checks: game installed, MelonLoader, Il2Cpp assemblies, FMF core, FMF plugins directory, ModCfg directory.
- **MelonLoader** download page link and game folder access.
- **FMF Plugin channels** — stable (local scan) and beta (server, TODO).

### Параметры
- **Путь к рабочей области** — где хранятся проекты (по умолчанию `<GameRoot>/workshop`).
- **Язык** — переключение между EN, DE, RU, ES, IT, JP, PL, CN (по умолчанию язык системы). Кнопка перезапуска применяет изменение сразу.
- **Переключатель Mod Store** — включает/выключает вкладку Mod Store (по умолчанию выключено). Есть кнопка перезапуска.
- **Ссылки сообщества** — быстрый доступ к Discord, каналу моддинга и GregFramework.eu.

### Уведомление о зависимости FMF
- В редакторе проекты можно пометить как **«Требуется FrikaModFramework»**. При загрузке к описанию в Steam автоматически добавляется уведомление с просьбой установить FMF.

## Requirements

- **Windows 10** (версия 1809+).
- **Steam** с учётной записью, в которой **есть Data Center** (App ID **4170200**).
- **Без дополнительных зависимостей** — релиз полностью автономен (включает .NET runtime и Windows App SDK).
- `steam_api64.dll` и `steam_appid.txt` должны лежать рядом с exe (входят в релизную сборку).

## Workspace path

The workspace is resolved from Steam: `<Data Center install>/workshop`.

Fallback (no Steam): `%USERPROFILE%\DataCenterWS`.

On first launch the app creates the structure and places a sample `metadata.sample.json` under `.templates/`.

## Project layout

For each Workshop project:

1. Create a **folder** under the workspace (the folder name appears in the list).
2. Add a `content/` subfolder with files to ship:
   - `content/Mods/` for MelonLoader mods
   - `content/FMF/Plugins/` for FMF plugins
   - `content/Object/` and `content/Decoration/` for vanilla assets
3. Optionally create `metadata.json` yourself or fill it in the app.
4. Optionally add `preview.png` at the project root.

## Using the app

### Tabs

| Tab | Purpose |
|-----|---------|
| **Projects** | Local workshop projects; search, open editor |
| **New** | Create from template (vanilla, MelonLoader, FMF) |
| **My Uploads** | Paginated list of your published items with stats |
| **Mod Store** | Browse, search, subscribe, vote, and manage mods |

### Publish workflow

1. Open a project from the **Projects** tab.
2. Fill in title, description, tags, visibility, preview image.
3. Optionally write **change notes** (visible on Steam's changelog).
4. Click **Save and upload to Steam**.
5. The app saves `metadata.json`, uploads `content/` to Steam, then **syncs** by re-downloading Steam's version into your local `content/` folder.

### Headless / CI publish

```bash
WorkshopUploader.exe --mode publish --path <project-folder>
```

## Build and deploy {#build-deploy}

### Автономный релиз

Приложение публикуется как **автономный** исполняемый файл Windows — на целевой машине не требуется устанавливать .NET runtime или Windows App SDK.

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release -p:SelfContained=true -p:RuntimeIdentifier=win10-x64
```

Вывод: `WorkshopUploader/bin/Release/net9.0-windows10.0.19041.0/win10-x64/publish/`

### Развернуть все моды и Gregtools Modmanager в папки Workshop

```bash
pwsh -File scripts/Deploy-Release-ToWorkshop.ps1
```

Собирает все framework/plugins/mods **и** сам WorkshopUploader, затем упаковывает каждый в папку проекта, совместимую со Steamworks, в `<GameRoot>/workshop/`. WorkshopUploader упаковывается как **«Gregtools Modmanager»**.

### Развернуть в каталог игры (локальное тестирование)

```bash
pwsh -File scripts/Deploy-Release-ToDataCenter.ps1 -IncludeWorkshopUploader
```

## See also

- Repository README: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- [End-User Guide](/wiki/guides/enduser-workshop)
- [Contributor Guide](/wiki/guides/contributor-workshop)
- [Release](/wiki/guides/release)
