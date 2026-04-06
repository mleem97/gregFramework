---
title: WorkshopUploader
sidebar_label: WorkshopUploader
description: Windows desktop app for managing Steam Workshop projects and metadata for Data Center (FrikaMF).
---

# WorkshopUploader

**WorkshopUploader** is a **.NET MAUI** desktop app for **Windows**. It helps you prepare **Workshop content** for *Data Center*: folder layout, `metadata.json`, preview image, and upload via the **Steamworks** API (Steam must be running and the game must be the active App ID context).

## What the app does

- Creates a workspace folder **`DataCenterWS`** under your user profile (path below).
- Lists **project folders** under it; each project may have a **`content/`** subfolder — that folder is what gets uploaded to the Workshop item.
- For each project you edit **title**, **description**, **visibility** (Public / FriendsOnly / Private), and a **preview image**; values are stored in **`metadata.json`**.
- **Publish to Steam** creates a **new** Workshop item or updates an existing one when a **published file ID** is already saved.

## Requirements

- **Windows** (the project targets MAUI for Windows).
- **Steam** with a signed-in account that **owns Data Center** and is allowed to upload Workshop content (App ID **4170200**).
- Optional: a built **`WorkshopUploader.exe`** next to the game install (see [Build and deploy](#build-deploy)).

## Workspace path

The workspace is fixed to **`DataCenterWS`** under your profile, for example:

`%USERPROFILE%\DataCenterWS`

On first launch the app creates the structure and may place a sample **`metadata.sample.json`** under `.templates\`.

## Project layout

For each Workshop project:

1. Create a **folder** under `DataCenterWS` (the folder name appears in the list).
2. Add a **`content\`** subfolder and put the files that should ship in the Workshop item there (your mod data and assets — **your** content only; do not redistribute game binaries).
3. Optionally create **`metadata.json`** yourself or fill it in the app; the app stores title, description, visibility, preview path, and after the first upload the **published file ID**.
4. Optionally add **`preview.png`** at the project root (or another relative path in metadata) — you can pick an image in the app; it is copied into the project as `preview.png`.

Without **`content/`**, the list shows a warning (“Missing content/”); upload is not possible until it exists.

## Using the app

1. **Home:** **Workshop projects** — the **workspace path** is shown at the top. Pull to refresh the list.
2. **Open a project:** tap an entry → **Editor**.
3. **Editor:** title and description (Steam character limits), **visibility**, **choose preview**, then:
   - **Save metadata.json** — save only.
   - **Publish to Steam** — saves and uploads **`content/`**; first run creates a new Workshop item, later runs reuse the stored **file ID**.
4. The **log** on the home page shows messages (Steam init, upload progress, etc.).

If Steam cannot initialize (e.g. Steam not running), the app reports that.

## Build and deploy {#build-deploy}

From the repository:

```bash
dotnet build WorkshopUploader/WorkshopUploader.csproj -c Debug
```

Release (typically a single-folder publish on Windows):

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release
```

Output is typically under `WorkshopUploader\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\WorkshopUploader.exe` (exact path depends on SDK / TFM).

To place the tool **next to the game** (not under `Mods` / `MelonLoader`):

`{GameRoot}\WorkshopUploader\`

## See also

- Repository README: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- Workshop context: [Steam Workshop and Tooling](/wiki/meta/Steam-Workshop-and-Tooling)
- DevServer betas (`gregframework.eu`): [DevServer betas](/wiki/meta/devserver-betas)
