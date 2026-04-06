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

### Configuración
- **Ruta del espacio de trabajo** — cambia dónde se guardan los proyectos (por defecto `<GameRoot>/workshop`).
- **Idioma** — alternar entre EN, DE, RU, ES, IT, JP, PL, CN (por defecto el idioma del sistema). El botón Reiniciar aplica el cambio al instante.
- **Interruptor Mod Store** — activar/desactivar la pestaña Mod Store (desactivada por defecto). Incluye botón Reiniciar.
- **Enlaces de la comunidad** — acceso rápido a Discord, canal de modding y GregFramework.eu.

### Aviso de dependencia de FMF
- Los proyectos pueden marcarse como **«Necesita FrikaModFramework»** en el editor. Al subir, se añade automáticamente un aviso a la descripción de Steam para indicar a los usuarios que instalen FMF.

## Requirements

- **Windows 10** (versión 1809+).
- **Steam** con una cuenta iniciada sesión que **posea Data Center** (ID de aplicación **4170200**).
- **Sin dependencias adicionales** — la versión publicada es totalmente autocontenida (incluye runtime de .NET y Windows App SDK).
- `steam_api64.dll` y `steam_appid.txt` deben estar junto al exe (incluidos en la compilación Release).

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

### Release autocontenido

La aplicación se publica como ejecutable de Windows **autocontenido** — no hace falta instalar runtime de .NET ni Windows App SDK en el equipo de destino.

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release -p:SelfContained=true -p:RuntimeIdentifier=win10-x64
```

Salida: `WorkshopUploader/bin/Release/net9.0-windows10.0.19041.0/win10-x64/publish/`

### Desplegar todos los mods + Gregtools Modmanager en carpetas Workshop

```bash
pwsh -File scripts/Deploy-Release-ToWorkshop.ps1
```

Compila todos los frameworks/plugins/mods **y** el propio WorkshopUploader, y empaqueta cada uno en una carpeta de proyecto compatible con Steamworks bajo `<GameRoot>/workshop/`. El WorkshopUploader se empaqueta como **«Gregtools Modmanager»**.

### Desplegar en el directorio del juego (pruebas locales)

```bash
pwsh -File scripts/Deploy-Release-ToDataCenter.ps1 -IncludeWorkshopUploader
```

## See also

- Repository README: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- [End-User Guide](/wiki/guides/enduser-workshop)
- [Contributor Guide](/wiki/guides/contributor-workshop)
- [Release](/wiki/guides/release)
