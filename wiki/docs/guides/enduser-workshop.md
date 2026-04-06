---
id: enduser-workshop
title: End-User Guide — WorkshopManager
sidebar_label: End-User Guide
description: How to browse, install, and manage Data Center mods using the WorkshopManager.
sidebar_position: 10
tags:
  - audience:enduser
  - workshop
---

# End-User Guide — WorkshopManager

This guide is for players who want to **install and manage mods** for Data Center using the WorkshopManager desktop app.

## What you need

- **Data Center** installed via Steam.
- **Steam** running and logged in.
- **WorkshopManager** (`WorkshopUploader.exe`) — either built from source or provided as a release.

## Installation

1. Download or build the WorkshopManager.
2. Place the app folder at `<Data Center install>/WorkshopUploader/`.
3. Launch `WorkshopUploader.exe`. The Steam status indicator in the top-right should turn green.

## Browsing mods (Mod Store)

1. Open the **Mod Store** tab.
2. Use the **Store** sub-tab to browse all available mods.
3. Filter by tag (vanilla, modded, melonloader, fmf, framework) or sort by popularity, date, score, etc.
4. Use the **Search** bar to find specific mods by name.
5. Click on any mod to see its **detail page** with full stats, description, and action buttons.

## Installing mods (Subscribe)

1. In the Store or on an item's detail page, click **Subscribe**.
2. Steam downloads the mod automatically.
3. Check the **Installed** sub-tab to see all your subscribed mods.

Subscribed mods are managed by Steam — they update automatically when the author publishes changes.

## Favorites

1. On an item's detail page, click **Favorite** to bookmark it.
2. View all your favorites in the **Favorites** sub-tab.
3. Click **Unfavorite** to remove.

## Voting

On an item's detail page:
- Click **Vote Up** to recommend the mod.
- Click **Vote Down** if there's a quality issue.

Your votes help the community find the best mods.

## Dependency Health (Health tab)

The **Health** sub-tab checks whether your game has:
- MelonLoader installed
- Il2Cpp interop assemblies generated
- FrikaModFramework core DLL
- FMF plugins directory
- Mod config directory

If anything is missing, follow the instructions shown.

### Installing MelonLoader

1. Go to the Health tab and click **Download page**.
2. Download the latest **MelonLoader Installer** from GitHub.
3. Run the installer, select **Data Center** as the game, choose **IL2CPP**.
4. Start the game once and close it (this generates required assemblies).

## Troubleshooting

### "Steam - offline" in the title bar

- Make sure Steam is running and you are logged in.
- Ensure `steam_appid.txt` (containing `4170200`) exists next to `WorkshopUploader.exe`.

### Mod does not load in-game

- Check `MelonLoader/Latest.log` in the game directory.
- Ensure `FrikaModdingFramework.dll` is in `<game>/Mods/`.
- Verify the mod DLL is also in `<game>/Mods/` or `<game>/FMF/Plugins/`.

### Subscription does not appear

- Wait a few seconds and refresh the Installed list.
- Steam may need a moment to process the subscription.

## Uninstalling mods

1. Go to the **Installed** sub-tab.
2. Click **Unsubscribe** on the mod you want to remove.
3. Steam removes the mod files automatically.
