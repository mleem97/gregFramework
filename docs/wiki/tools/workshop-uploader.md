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
- Edit **title**, **description** (with BBCode toolbar), **visibility**, **tags**, and **preview image**; stored in `metadata.json`.
- **BBCode formatting toolbar** — insert Steam-compatible formatting (bold, italic, headings, URL, image, list, code, quote, spoiler, horizontal rule, table) directly in the description editor.
- **Save and upload to Steam** — saves metadata, publishes `content/` to Steam Workshop, then **syncs** your local folder with Steam's version (like a `git pull` after push).
- **Image sync** — after publishing, the preview image and gallery screenshots are re-downloaded from Steam so your local project always matches what's live.
- **Additional screenshots** — attach up to 9 gallery images that appear on the Steam Workshop page alongside the main preview.
- **Change notes** — required for the first publish, recommended for updates; shown on the Workshop changelog tab.
- **Upload readiness check** — validates title, description, content folder, preview image, tags, content size, and changelog before allowing upload.
- **View on Steam** — open your published item in the browser.
- **Export content as ZIP** — create a distributable archive of the content folder.
- **Pagination** across all list views (My Uploads, Browse, Subscribed, Favorites).
- **Per-item stats** — subscriptions, votes, score, size.

### Mod Store (integrated in Mod Manager)
- **Browse** all Workshop items for Data Center with tag filtering and sort options (last updated, newest, top rated, trending, most subscribed, title A-Z).
- **Search** mods by text.
- **Subscribe / Unsubscribe** to mods directly from the store.
- **Favorites** — bookmark mods for quick access.
- **Vote** (up/down) on workshop items.
- **Item detail view** — full statistics, description, tags, gallery images, action buttons, and links to changelog/comments.

### Mod Manager / Health
- **Dependency health** checks: game installed, MelonLoader, Il2Cpp assemblies, FMF core, FMF plugins directory, ModCfg directory.
- **MelonLoader** download page link and game folder access.
- **FMF Plugin channels** — stable (local scan) and beta (server, TODO).

### Settings
- **Workspace path** — change where projects are stored (defaults to `<GameRoot>/workshop`).
- **Language** — switch between EN, DE, RU, ES, IT, JP, PL, CN (defaults to system language). Restart button applies the change instantly.
- **Mod Store toggle** — enable/disable the Mod Store tab (disabled by default). Restart button included.
- **Community links** — quick access to Discord, Modding Channel, and GregFramework.eu.

### FMF dependency notice
- Projects can be marked as **"Needs FrikaModFramework"** in the editor. When uploading, a notice is automatically appended to the Steam description telling users to install FMF.

## Requirements

- **Windows 10** (version 1809+).
- **Steam** with a signed-in account that **owns Data Center** (App ID **4170200**).
- **No additional dependencies** — the release is fully self-contained (includes .NET runtime and Windows App SDK).
- `steam_appid.txt` must be next to the executable (included in the release build). The app loads **`steam_api64.dll`** from **`Data Center_Data/Plugins/x86_64/`** in the Data Center install when possible; otherwise it uses the copy shipped next to the uploader.

## Paths and directories {#paths}

### Workspace path (your authoring projects)

The workspace is resolved from Steam: `<Data Center install>/workshop`.

Fallback (no Steam): `%USERPROFILE%\DataCenterWS`.

You can change this in **Settings → Workspace path**.

On first launch the app creates the structure and places a sample `metadata.sample.json` under `.templates/`.

### Where Steam installs subscribed Workshop mods {#steam-install-path}

When a user **subscribes** to a Workshop item, Steam downloads it to:

```
<SteamLibrary>\steamapps\workshop\content\4170200\<PublishedFileId>\
```

For example, if Steam is installed in the default location and the item ID is `123456789`:

```
C:\Program Files (x86)\Steam\steamapps\workshop\content\4170200\123456789\
```

If you have multiple Steam library folders (e.g. on a different drive), the path follows that library's location:

```
D:\SteamLibrary\steamapps\workshop\content\4170200\123456789\
```

The game reads from this folder at startup. The exact path per item can be inspected in the Steamworks API via `ugc.Directory` after calling `DownloadAsync`.

:::info Finding your Steam library folder
Open Steam → **Settings → Storage** to see all configured library folders and which drive each uses.
:::

### Where the game is installed

The game install directory is typically:

```
C:\Program Files (x86)\Steam\steamapps\common\Data Center\
```

The app resolves this automatically via `SteamApps.AppInstallDir(4170200)`.

## Native mod format (config.json) {#native-mod-format}

Data Center has a **built-in mod system** (`ModLoader`) that loads mods from `config.json` files. This is the "vanilla" modding approach — **no MelonLoader or FMF required**.

### How the ModLoader works (internal flow) {#modloader-internals}

The game's `ModLoader` (a Unity `MonoBehaviour`) runs the following on startup:

1. **`SyncWorkshopThenLoadAll()`** — a coroutine that:
   - Queries all subscribed Steam Workshop items (`PublishedFileId_t[]`)
   - Waits for downloads to complete (with timeout)
   - Copies each Workshop item folder into `StreamingAssets/Mods/workshop_<ID>` via `CopyDirectory()`
2. **`LoadAllMods()`** — scans all subdirectories in the mods path:
   - Calls `LoadModPack(folderPath)` for each mod folder
   - Reads `config.json`, then for each entry calls:
     - `LoadShopItem()` → `CreateShopTemplate()` + `CreateShopButton()` for purchasable items
     - `LoadStaticItem()` → `CreateStaticInstance()` for fixed-position decorations

:::warning DLL loading timing
The `LoadDll()` method exists in the `ModLoader` but **object assets (shopItems, staticItems) are loaded at game startup**. DLL-based mods (`dlls[]` in config.json) are loaded later at **SaveLoad runtime**, not during the initial `LoadAllMods` pass. This means DLL plugins are not available until a save is loaded.
:::

Internal state tracked by `ModLoader`:
- `modTemplates` — `Dictionary<int, GameObject>` of loaded shop item prefabs by mod ID
- `modTemplatesByFolder` — `Dictionary<string, GameObject>` of prefabs by folder name
- `staticInstances` — `List<GameObject>` of instantiated static decorations
- `loadedPlugins` — `List<IModPlugin>` of loaded DLL plugins
- `nextModID` — auto-incrementing ID counter

### Mod location (manual install)

Mods are placed under the game's streaming assets:

```
Data Center_Data\StreamingAssets\Mods\<mod_folder>\
```

Each mod has its own folder with a `config.json` and its asset files.

### Mod location (Workshop subscribers)

When delivered via Steam Workshop, the game's `ModLoader.SyncWorkshopThenLoadAll()` automatically **copies** subscribed item content from the Workshop download folder into `StreamingAssets/Mods/workshop_<ID>`:

```
steamapps\workshop\content\4170200\<PublishedFileId>\
  →  copied to  →
Data Center_Data\StreamingAssets\Mods\workshop_<PublishedFileId>\
```

The `workshop_` prefix distinguishes Workshop-downloaded mods from manually installed ones.

### config.json reference {#config-json}

```json
{
  "modName": "My Cool Mod",
  "shopItems": [
    {
      "itemName": "Custom Server",
      "price": 500,
      "xpToUnlock": 0,
      "sizeInU": 2,
      "mass": 5.0,
      "modelScale": 1.0,
      "colliderSize": [0.5, 0.5, 0.5],
      "colliderCenter": [0.0, 0.0, 0.0],
      "modelFile": "server.obj",
      "textureFile": "server.png",
      "iconFile": "server_icon.png",
      "objectType": 0
    }
  ],
  "staticItems": [
    {
      "itemName": "Wall Poster",
      "modelFile": "poster.obj",
      "textureFile": "poster.png",
      "modelScale": 1.0,
      "colliderSize": [0.5, 0.5, 0.05],
      "colliderCenter": [0.0, 0.0, 0.0],
      "position": [2.0, 1.5, -3.0],
      "rotation": [0.0, 90.0, 0.0]
    }
  ],
  "dlls": [
    {
      "fileName": "MyModPlugin.dll",
      "entryClass": "MyModPlugin.Plugin"
    }
  ]
}
```

### config.json fields

| Field | Description |
|-------|-------------|
| `modName` | Display name of the mod |
| `shopItems[]` | Purchasable objects that appear in the in-game shop |
| `staticItems[]` | Decorations placed at fixed world positions |
| `dlls[]` | Native plugin DLLs (see [DLL mods without MelonLoader](#native-dll-mods)) |

### shopItems fields

| Field | Type | Description |
|-------|------|-------------|
| `itemName` | `string` | Display name in the shop |
| `price` | `int` | Purchase price |
| `xpToUnlock` | `int` | XP threshold to unlock (0 = immediately available) |
| `sizeInU` | `int` | Rack unit size of the object |
| `mass` | `float` | Object mass |
| `modelScale` | `float` | Scale multiplier for the 3D model |
| `colliderSize` | `[x, y, z]` | Physics collider dimensions |
| `colliderCenter` | `[x, y, z]` | Physics collider offset |
| `modelFile` | `string` | Path to `.obj` model file (loaded via `LoadMesh`) |
| `textureFile` | `string` | Path to `.png` texture file (loaded via `CreateMaterial`) |
| `iconFile` | `string` | Path to `.png` shop icon (loaded via `LoadIcon`) |
| `objectType` | `int` | `0` = passive object |

The `ModShopItem` UI component displays each shop item with `itemIcon`, `txtName`, and `txtPrice`. Purchasing calls `ButtonBuyItem()`.

### staticItems fields

| Field | Type | Description |
|-------|------|-------------|
| `itemName` | `string` | Display name |
| `modelFile` | `string` | Path to `.obj` model file |
| `textureFile` | `string` | Path to `.png` texture file |
| `modelScale` | `float` | Scale multiplier |
| `colliderSize` | `[x, y, z]` | Physics collider dimensions |
| `colliderCenter` | `[x, y, z]` | Physics collider offset |
| `position` | `[x, y, z]` | World position |
| `rotation` | `[x, y, z]` | Euler rotation in degrees |

### Asset loading pipeline

| Method | Input | Output |
|--------|-------|--------|
| `LoadMesh(folderPath, modelFile)` | `.obj` file | Unity `Mesh` |
| `CreateMaterial(folderPath, textureFile)` | `.png` file | Unity `Material` |
| `LoadIcon(folderPath, iconFile)` | `.png` file | Unity `Sprite` |
| `LoadTexture(path)` | `.png` file | Unity `Texture2D` |

- **3D models**: Wavefront OBJ format (`.obj`)
- **Textures**: PNG format (`.png`)
- **Icons**: PNG format (`.png`) — shown in the shop UI
- All asset file paths in `config.json` are relative to the mod folder

### DLL mods without MelonLoader {#native-dll-mods}

The game's `ModLoader` has **built-in DLL loading** — no MelonLoader required. This is done via the `dlls` array in `config.json`:

```json
{
  "modName": "My Plugin Mod",
  "shopItems": [],
  "staticItems": [],
  "dlls": [
    {
      "fileName": "MyPlugin.dll",
      "entryClass": "MyPlugin.Plugin"
    }
  ]
}
```

**How it works internally:**
1. `ModLoader.LoadDll(folderPath, DllEntry)` loads the DLL from the mod folder
2. The `entryClass` must reference a class that implements the **`IModPlugin`** interface
3. Loaded plugins are stored in `ModLoader.loadedPlugins` (`List<IModPlugin>`)

:::warning Loading timing
Unlike asset objects (`shopItems`, `staticItems`) which are loaded at game startup, **DLL mods are loaded at SaveLoad runtime** — i.e., when a save file is loaded, not when the game first starts. Your `IModPlugin` will not be instantiated until the player enters a save.
:::

**DLL entry requirements:**

| Field | Description |
|-------|-------------|
| `fileName` | The `.dll` file in the mod folder |
| `entryClass` | Fully qualified class name (e.g. `MyNamespace.MyPlugin`) that implements `IModPlugin` |

:::caution Experimental feature
The developer has marked DLL loading as **untested**. The `LoadDll` method and `IModPlugin` interface exist in the game code, but stability is not guaranteed. A mod that uses only `shopItems` and `staticItems` (asset-only) is the safest approach.
:::

**Advantages over MelonLoader:**
- No external framework needed — works with the vanilla game
- Distributed via Workshop like any other mod (just include the DLL in the mod folder)
- The game manages loading/unloading through its own lifecycle

**Limitations:**
- The `IModPlugin` interface contract is not publicly documented — you need to reference the game's IL2CPP assemblies to implement it
- Less flexibility than MelonLoader's Harmony patching
- Only the entry class specified in `entryClass` is instantiated
- DLLs are loaded at SaveLoad time, not at game startup — no early initialization possible

:::note Current limitations
For now, only **passive objects** are supported for shop items (`objectType: 0`). Active/functional item types may be added in future game updates.
:::

### ModPathRedirector — Workshop download at MelonLoader init {#mod-path-redirector}

**Release & download:** [FMF.ModPathRedirector Release](/wiki/releases/plugins/fmf-modpathredirector-release) — installation steps and GitHub download (`FMF.ModPathRedirector.dll`).

The game loads native mods from `Data Center_Data\StreamingAssets\Mods\`. Workshop items are copied into subfolders named `workshop_<PublishedFileId>` under that path (see [Steam install path](#steam-install-path)).

**ModPathRedirector** is a **MelonLoader plugin** (not a MelonMod): install the DLL under **`{GameRoot}/Plugins/`**. It does **not** change native mod paths. It calls **`steam_api64.dll`** directly (Steam flat API: `SteamAPI_ISteamUGC_*` on the same `ISteamUGC` instance as the game — same process, same Steam session). The game ships this DLL under **`Data Center_Data/Plugins/x86_64/`** (Unity’s native plugin folder); the plugin loads it from there first, then falls back to **`steam_api64.dll`** next to the executable if present. It waits until the Steam client is running, then triggers **`DownloadItem`** for subscribed Workshop content that needs a fetch or update.

**What it does:**
- On each frame (from `OnApplicationStarted`), waits for `SteamAPI_IsSteamRunning` and resolves `SteamAPI_SteamUGC_v0xx` → `ISteamUGC*`, then calls the native UGC APIs (no Il2Cpp Steamworks wrapper)
- Leaves `ModLoader.LoadAllMods` / `CopyDirectory` unchanged — Workshop sync still uses `Data Center_Data/StreamingAssets/Mods/workshop_<ID>/`

**Installation:**
1. Copy `FMF.ModPathRedirector.dll` into `<GameRoot>/Plugins/` (MelonLoader **Plugins** folder next to the executable)
2. Start the game — subscribed Workshop content is requested early; manual native mods still go under `Data Center_Data/StreamingAssets/Mods/<folder>/` (each folder with a `config.json`)

**Layout (unchanged from vanilla):**

```
Data Center/
  Data Center_Data/
    Plugins/
      x86_64/
        steam_api64.dll          ← shipped with the game (Steam API; ModPathRedirector loads this)
    StreamingAssets/
      Mods/
        my_cool_mod/             ← manual native mod
          config.json
        workshop_12345678/       ← Workshop item (game copies here after download)
          ...
  Plugins/                       ← MelonLoader plugins (FMF.ModPathRedirector.dll)
  Mods/                          ← MelonLoader mods (other FMF.* MelonMods)
  Data Center.exe
```

## Project layout

For each Workshop project:

1. Create a **folder** under the workspace (the folder name appears in the list).
2. Add a `content/` subfolder with files to ship:
   - For **vanilla mods**: `config.json` + `.obj` models + `.png` textures directly in `content/`
   - For **MelonLoader mods**: `content/Mods/` with `.dll` files
   - For **FMF plugins**: `content/FMF/Plugins/` with `.dll` files
3. Optionally create `metadata.json` yourself or fill it in the app.
4. Optionally add `preview.png` (or `.jpg`, `.jpeg`, `.gif`, `.webp`) at the project root.
5. Optionally add gallery screenshots under `screenshots/` — these are uploaded as additional preview images.

### Example: Vanilla mod (native config.json)

```
workshop/
└── MyCustomServers/
    ├── metadata.json              # Workshop metadata (title, desc, tags)
    ├── preview.png                # Main preview image
    └── content/                   # Shipped to subscribers
        ├── config.json            # Native mod definition
        ├── server.obj             # 3D model
        ├── server.png             # Texture
        └── server_icon.png        # Shop icon
```

### Example: MelonLoader mod

```
workshop/
└── MyMelonMod/
    ├── metadata.json
    ├── preview.png
    └── content/
        └── Mods/
            └── MyMod.dll
```

### Example: FMF plugin

```
workshop/
└── MyFmfPlugin/
    ├── metadata.json
    ├── preview.png
    └── content/
        └── FMF/
            └── Plugins/
                └── FFM.Plugin.MyPlugin.dll
```

### metadata.json schema

| Field | Type | Description |
|-------|------|-------------|
| `publishedFileId` | `number` | Steam file ID (0 if unpublished) |
| `title` | `string` | Workshop item title (max 128 chars) |
| `description` | `string` | Description, supports [Steam BBCode](#bbcode) (max 8000 chars) |
| `visibility` | `string` | `"Public"`, `"FriendsOnly"`, or `"Private"` |
| `previewImageRelativePath` | `string` | Relative path to preview image (default `"preview.png"`) |
| `tags` | `string[]` | Up to 20 tags for discoverability |
| `needsFmf` | `boolean` | Appends FMF dependency notice on upload |
| `additionalPreviews` | `string[]` | Relative paths to gallery screenshot images |

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
3. Optionally add **gallery screenshots** (up to 9 additional images).
4. Write a **version changelog** (required for first publish).
5. Click **Save and upload to Steam**.
6. The app:
   - Validates all required fields (upload readiness check).
   - Saves `metadata.json`.
   - Uploads `content/` and preview to Steam.
   - Uploads additional gallery screenshots (if any).
   - **Syncs back** — re-downloads Steam's version of content, preview image, and gallery screenshots into your local project folder.

### Import from Steam

You can import an existing published Workshop item into your local workspace:

1. Go to **My Uploads** → click an item → **Import**.
2. The app downloads the content, preview image, gallery screenshots, and creates a local project with the correct `metadata.json`.

### Headless / CI publish

```bash
WorkshopUploader.exe --mode publish --path <project-folder>
```

Optional flags:
- `--autocommit` — writes a status JSON for external tools.

## Steam BBCode reference {#bbcode}

Steam Workshop descriptions support **BBCode** formatting. The editor includes a toolbar with all supported tags. Here is the full reference:

### Text formatting

```bbcode
[b]Bold text[/b]
[i]Italic text[/i]
[u]Underlined text[/u]
[strike]Strikethrough text[/strike]
```

### Headings

```bbcode
[h1]Heading 1[/h1]
[h2]Heading 2[/h2]
[h3]Heading 3[/h3]
```

### Links and images

```bbcode
[url=https://example.com]Link text[/url]
[img]https://example.com/image.png[/img]
```

### Lists

```bbcode
[list]
[*] Item 1
[*] Item 2
[*] Item 3
[/list]
```

### Code blocks

```bbcode
[code]
console.log("Hello World");
[/code]
```

### Quotes and spoilers

```bbcode
[quote]Quoted text here[/quote]
[spoiler]Hidden text revealed on click[/spoiler]
```

### Horizontal rule

```bbcode
[hr][/hr]
```

### Tables

```bbcode
[table]
[tr]
[th]Header 1[/th]
[th]Header 2[/th]
[/tr]
[tr]
[td]Cell 1[/td]
[td]Cell 2[/td]
[/tr]
[/table]
```

### Example workshop description

```bbcode
[h1]My Awesome Mod[/h1]

Custom decorations and objects for Data Center.

[h2]Features[/h2]
[list]
[*] 10 new decoration items
[*] 5 custom server rack skins
[*] Compatible with FrikaModFramework
[/list]

[h2]Installation[/h2]
Subscribe to this Workshop item — the game loads it automatically.

[h2]Requirements[/h2]
[list]
[*] Data Center (Steam)
[*] [url=https://melonwiki.xyz]MelonLoader[/url] (for modded items)
[/list]

[h3]Changelog[/h3]
[b]v1.0[/b] — Initial release

[hr][/hr]
[i]Made by YourName — [url=https://discord.gg/your-server]Join the Discord[/url][/i]
```

## Upload readiness checks {#checks}

Before uploading, the editor runs automated checks:

| Check | Error condition | Warning condition |
|-------|----------------|-------------------|
| **Content folder** | `content/` missing or empty | — |
| **Title** | Empty or > 128 characters | — |
| **Description** | > 8000 characters | Empty (recommended) |
| **Visibility** | — | Unknown value |
| **Preview image** | — | Missing or > 1 MB |
| **Tags** | — | No tags set |
| **Content size** | — | > 100 MB |
| **Changelog** | Empty on first publish | Empty on update |

Items with **errors** cannot be uploaded. **Warnings** are informational — upload is still possible.

## Preview images {#preview-images}

### Main preview image
- Supports **PNG**, **JPG**, **JPEG**, **GIF**, and **WebP**.
- Auto-detected by filename: `preview.png`, `preview.jpg`, etc.
- Recommended size: under **1 MB**.
- The app auto-detects the format from the file's magic bytes when syncing from Steam.

### Gallery screenshots
- Up to **9** additional images per item.
- Stored in `screenshots/` under the project root.
- Uploaded in a separate pass after the main content publish.
- **Synced back** from Steam on publish and import — if screenshots exist on Steam, they are downloaded to your local `screenshots/` folder.

## Build and deploy {#build-deploy}

### Self-contained release

The app is published as a **self-contained** Windows executable — no .NET runtime or Windows App SDK install required on the target machine.

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release -p:SelfContained=true -p:RuntimeIdentifier=win10-x64
```

Output: `WorkshopUploader/bin/Release/net9.0-windows10.0.19041.0/win10-x64/publish/`

### Deploy all mods + Gregtools Modmanager to Workshop folders

```bash
pwsh -File scripts/Deploy-Release-ToWorkshop.ps1
```

This builds all framework/plugins/mods **and** the WorkshopUploader itself, then packages each into a Steamworks-compatible project folder under `<GameRoot>/workshop/`. The WorkshopUploader is packaged as **"Gregtools Modmanager"**.

### Deploy to game directory (local testing)

```bash
pwsh -File scripts/Deploy-Release-ToDataCenter.ps1 -IncludeWorkshopUploader
```

## FAQ {#faq}

### General

**Q: Do I need to install anything besides the app?**
No. The release is fully self-contained — .NET runtime and Windows App SDK are bundled. You only need Steam running with a Data Center license.

**Q: What Steam App ID does this use?**
`4170200` (Data Center). The file `steam_appid.txt` must contain this ID next to the executable.

**Q: Can I use this on Linux / macOS?**
Not currently. The app targets Windows 10+ via .NET MAUI with WinUI.

### Projects and workspace

**Q: Where are my projects stored?**
By default under `<Data Center install>/workshop/`. You can change this in Settings. Fallback if Steam is unavailable: `%USERPROFILE%\DataCenterWS`.

**Q: What is the difference between the workspace and Steam's download folder?**
The **workspace** (`<GameRoot>/workshop/`) is where you *author* content. The **Steam download folder** (`steamapps/workshop/content/4170200/<id>/`) is where Steam installs *subscribed* items for the game. They are separate locations.

**Q: Can I edit a project that was already published?**
Yes. Open it from **Projects**, make changes, and click **Save and upload to Steam**. The app updates the existing Workshop item (using the `publishedFileId` stored in `metadata.json`).

**Q: Can I import a Workshop item I published from another machine?**
Yes. Use **My Uploads** → select the item → **Import**. The app downloads everything (content, preview, gallery screenshots) and creates a local project.

### Publishing

**Q: Why can't I upload? The button is greyed out / shows errors.**
The upload readiness checker found issues. Common blockers:
- `content/` folder is missing or empty.
- Title is empty.
- No changelog provided on first publish.
Check the readiness panel at the bottom of the editor for details.

**Q: Is the changelog required?**
Yes for the **first publish** (error if empty). For updates it's recommended but not enforced (warning only).

**Q: What happens after I click "Save and upload to Steam"?**
1. Metadata is saved locally.
2. Content and preview are uploaded to Steam.
3. Gallery screenshots are uploaded (if any).
4. The app re-downloads everything from Steam to sync your local project (content, preview image, gallery images).

**Q: My upload says "Workshop legal agreement must be accepted."**
You need to accept the Steam Workshop legal agreement for Data Center in your browser. Visit the Workshop page for the game on Steam and accept the agreement.

**Q: Can I upload from the command line?**
Yes: `WorkshopUploader.exe --mode publish --path <project-folder>`. Useful for CI/CD pipelines.

### Descriptions and formatting

**Q: Does the description support formatting?**
Yes, Steam Workshop uses **BBCode**. The editor includes a toolbar with all supported tags (bold, italic, headings, URLs, images, lists, code, quotes, spoilers, tables). See the [BBCode reference](#bbcode) above.

**Q: What is the maximum description length?**
8000 characters.

**Q: Can I use Markdown in descriptions?**
No. Steam Workshop only supports BBCode. Markdown will be displayed as plain text.

### Images

**Q: What image formats are supported?**
PNG, JPG/JPEG, GIF, and WebP for both the main preview and gallery screenshots.

**Q: How large can the preview image be?**
Steam recommends under **1 MB**. The app warns if it's larger but doesn't block upload.

**Q: How many gallery screenshots can I add?**
Up to **9** additional images. They appear in the image gallery on the Steam Workshop page.

**Q: Are my screenshots synced from Steam?**
Yes. After publishing and during import, gallery screenshots are downloaded from Steam and saved in your project's `screenshots/` folder.

### Mods and installation

**Q: Where does Steam install Workshop mods?**
`<SteamLibrary>/steamapps/workshop/content/4170200/<PublishedFileId>/`. See [Steam install path](#steam-install-path) for details.

**Q: Where does the game look for native mods (manual install)?**
`Data Center_Data/StreamingAssets/Mods/<mod_folder>/`. Each subfolder must contain a `config.json`. See the [native mod format](#native-mod-format) reference.

**Q: How does the game load Workshop mods?**
At startup, the game's `ModLoader.SyncWorkshopThenLoadAll()` queries all subscribed items from Steam, waits for downloads to complete, and copies each into `Data Center_Data/StreamingAssets/Mods/workshop_<ID>/`. Then `LoadAllMods()` scans that folder. Asset objects (shopItems, staticItems) load immediately; DLL mods load later at SaveLoad time. The content structure determines what kind of mod it is:
- **Native mods** (vanilla): `config.json` + OBJ/PNG assets directly in `content/` — no extra tools needed.
- **MelonLoader mods**: `content/Mods/` with `.dll` files — requires MelonLoader.
- **FMF plugins**: `content/FMF/Plugins/` with `.dll` files — requires MelonLoader + FMF.

**Q: What are the three mod types?**

| Type | Requirements | Content structure | Use case |
|------|-------------|-------------------|----------|
| **Vanilla (native)** | None | `config.json` + `.obj` + `.png` | Custom objects, decorations, shop items |
| **MelonLoader** | MelonLoader | `Mods/*.dll` | Code mods (IL2CPP) |
| **FMF plugin** | MelonLoader + FMF | `FMF/Plugins/*.dll` | FrikaModFramework plugins |

**Q: What model format does the native mod system use?**
Wavefront OBJ (`.obj`) for 3D models and PNG (`.png`) for textures and icons.

**Q: What is `objectType` in config.json?**
Currently only `0` (passive object) is supported. Active/functional object types may be added in future game updates.

**Q: Can I load custom DLLs via the native mod system (without MelonLoader)?**
Yes. The game's `ModLoader` has built-in DLL loading via the `dlls` array in `config.json`. Your entry class must implement the game's `IModPlugin` interface. See [DLL mods without MelonLoader](#native-dll-mods) for details. Note that this feature is marked as **experimental** by the developer — asset-only mods (`shopItems`/`staticItems`) are the safest approach.

**Q: Do users need MelonLoader to use Workshop items?**
Only for MelonLoader and FMF mods. **Vanilla/native mods** (using `config.json`) work without any additional tools — just subscribe and play. Mark your project as "Needs FrikaModFramework" if applicable — the app will add an installation notice to the description automatically.

**Q: Can I change where native mods are loaded from?**
Yes. Install the [ModPathRedirector](#mod-path-redirector) MelonLoader plugin into `{GameRoot}/Plugins/`. It uses `steam_api64.dll` to trigger Workshop downloads once the Steam client is up; files still land under `Data Center_Data/StreamingAssets/Mods/workshop_<ID>/`.

### Mod Store

**Q: How do I enable the Mod Store?**
Go to **Settings** → toggle **Mod Store** on → restart the app.

**Q: Can I subscribe to mods from the app?**
Yes. Browse or search for mods, then click **Subscribe**. Steam will download the mod automatically.

### Troubleshooting

**Q: The app won't start / crashes on launch.**
- Ensure `steam_appid.txt` is next to the executable; `steam_api64.dll` should resolve from the Data Center install (`Data Center_Data/Plugins/x86_64/`) or from the uploader folder.
- Steam must be running and logged in.
- Check the Windows Event Viewer (Application log) for .NET crash details.
- On Windows 10 < 1809, the app may not work due to WinUI requirements.

**Q: "Steam init failed" error in the app.**
- Make sure Steam is running and you are logged in.
- Verify you own Data Center (App ID 4170200).
- Check that `steam_appid.txt` contains `4170200`.

**Q: My content doesn't show up after subscribing.**
- Restart the game — Workshop content is loaded at startup.
- Check `steamapps/workshop/content/4170200/` to verify the download exists.
- If the folder is empty, try unsubscribing and re-subscribing.

**Q: The preview image doesn't load in the Mod Store.**
- The image is loaded from Steam's CDN. Check your internet connection.
- Very large images may take a moment to appear.

## See also

- Repository README: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- [End-User Guide](/wiki/guides/enduser-workshop)
- [Contributor Guide](/wiki/guides/contributor-workshop)
- [Release](/wiki/guides/release)
