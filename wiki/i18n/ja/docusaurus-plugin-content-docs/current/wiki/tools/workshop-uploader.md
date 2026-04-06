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

### 設定
- **ワークスペースのパス** — プロジェクトの保存場所を変更（既定は `<GameRoot>/workshop`）。
- **言語** — EN、DE、RU、ES、IT、JP、PL、CN の切り替え（既定はシステム言語）。再起動ボタンで変更をすぐに反映。
- **Mod Store の切り替え** — Mod Store タブの表示/非表示（既定はオフ）。再起動ボタン付き。
- **コミュニティリンク** — Discord、Modding チャンネル、GregFramework.eu へのクイックアクセス。

### FMF 依存関係の注意
- エディターでプロジェクトに **「FrikaModFramework が必要」** とマークできます。アップロード時に、FMF のインストールを促す注意書きが Steam の説明に自動で追記されます。

## Requirements

- **Windows 10**（バージョン 1809 以降）。
- **Steam** — **Data Center** を所有しているサインイン済みアカウント（アプリ ID **4170200**）。
- **追加の依存関係なし** — リリースは完全に自己完結型（.NET ランタイムと Windows App SDK を同梱）。
- `steam_api64.dll` と `steam_appid.txt` は exe と同じ場所に必要（リリースビルドに同梱）。

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

### 自己完結型リリース

アプリは **自己完結型** の Windows 実行ファイルとして公開されます。対象マシンに .NET ランタイムや Windows App SDK をインストールする必要はありません。

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release -p:SelfContained=true -p:RuntimeIdentifier=win10-x64
```

出力: `WorkshopUploader/bin/Release/net9.0-windows10.0.19041.0/win10-x64/publish/`

### 全モッド + Gregtools Modmanager を Workshop フォルダへデプロイ

```bash
pwsh -File scripts/Deploy-Release-ToWorkshop.ps1
```

フレームワーク/プラグイン/モッド **および** WorkshopUploader 自体をビルドし、それぞれを `<GameRoot>/workshop/` 配下の Steamworks 互換プロジェクトフォルダにパッケージします。WorkshopUploader は **「Gregtools Modmanager」** としてパッケージされます。

### ゲームディレクトリへデプロイ（ローカル検証）

```bash
pwsh -File scripts/Deploy-Release-ToDataCenter.ps1 -IncludeWorkshopUploader
```

## See also

- Repository README: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- [End-User Guide](/wiki/guides/enduser-workshop)
- [Contributor Guide](/wiki/guides/contributor-workshop)
- [Release](/wiki/guides/release)
