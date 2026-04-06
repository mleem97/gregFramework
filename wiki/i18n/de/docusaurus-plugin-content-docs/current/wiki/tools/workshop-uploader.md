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

### Einstellungen
- **Arbeitsbereichspfad** — legt fest, wo Projekte gespeichert werden (Standard: `<GameRoot>/workshop`).
- **Sprache** — Umschalten zwischen EN, DE, RU, ES, IT, JP, PL, CN (Standard: Systemsprache). Der Neustart-Button wendet die Änderung sofort an.
- **Mod-Store-Umschaltung** — Mod-Store-Tab ein-/ausblenden (standardmäßig aus). Neustart-Button enthalten.
- **Community-Links** — Schnellzugriff auf Discord, Modding-Kanal und GregFramework.eu.

### Hinweis zur FMF-Abhängigkeit
- Projekte können im Editor als **„Benötigt FrikaModFramework“** markiert werden. Beim Hochladen wird der Steam-Beschreibung automatisch ein Hinweis angehängt, der Nutzer zum Installieren von FMF auffordert.

## Requirements

- **Windows 10** (Version 1809+).
- **Steam** mit angemeldetem Konto, das **Data Center** besitzt (App-ID **4170200**).
- **Keine weiteren Abhängigkeiten** — die Release-Version ist vollständig eigenständig (enthält .NET-Runtime und Windows App SDK).
- `steam_api64.dll` und `steam_appid.txt` müssen neben der exe liegen (im Release-Build enthalten).

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

### Eigenständiges Release

Die App wird als **eigenständige** Windows-Executable veröffentlicht — auf dem Zielrechner ist keine Installation von .NET-Runtime oder Windows App SDK erforderlich.

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release -p:SelfContained=true -p:RuntimeIdentifier=win10-x64
```

Ausgabe: `WorkshopUploader/bin/Release/net9.0-windows10.0.19041.0/win10-x64/publish/`

### Alle Mods + Gregtools Modmanager in Workshop-Ordner bereitstellen

```bash
pwsh -File scripts/Deploy-Release-ToWorkshop.ps1
```

Dies baut alle Frameworks/Plugins/Mods **und** den WorkshopUploader selbst und packt jedes in einen Steamworks-kompatiblen Projektordner unter `<GameRoot>/workshop/`. Der WorkshopUploader wird als **„Gregtools Modmanager“** verpackt.

### Im Spieleverzeichnis bereitstellen (lokales Testen)

```bash
pwsh -File scripts/Deploy-Release-ToDataCenter.ps1 -IncludeWorkshopUploader
```

## See also

- Repository README: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- [End-User Guide](/wiki/guides/enduser-workshop)
- [Contributor Guide](/wiki/guides/contributor-workshop)
- [Release](/wiki/guides/release)
