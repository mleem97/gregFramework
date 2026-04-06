---
id: release-channels
title: Release channels — Steam vs GitHub
slug: /reference/release-channels
description: Where to download stable builds, beta builds, and how this relates to the Steam Workshop.
---

# Release channels — Steam vs GitHub

This project uses **two complementary channels** for distributing mods and plugins. The wiki and [`/mods`](/mods) catalog are **not** a replacement for the Steam Workshop; they are an **overview**, documentation hub, and **second official source** for files that should not flood the Workshop.

## Steam Workshop (discovery & stable)

- **Game**: [Data Center on Steam](https://store.steampowered.com/app/4170200/) (AppID `4170200`).
- **Workshop**: [Browse items](https://steamcommunity.com/workshop/browse/?appid=4170200) — best for **player discovery**, ratings, and stable “subscribe and play” flows.
- Use Workshop for **production-ready** releases you want every player to see.

## GitHub Releases (beta and alternate downloads)

- **Organization**: [github.com/mleem97/gregFramework](https://github.com/mleem97/gregFramework) (see repo for actual org if renamed).
- **Stable**: tagged releases — same DLLs you may also ship via Workshop.
- **Pre-release / beta**: GitHub **pre-releases** — ideal for testers without publishing unfinished items to Workshop.
- **Why**: avoids “polluting” the Workshop with experimental builds while still offering a **single official URL** for power users and CI.

## This documentation site

| Need | Where |
|------|--------|
| Wiki entry for a module | `/wiki/mods/...` |
| Download link (when configured) | GitHub Releases asset or redirect from [`moduleCatalog`](https://github.com/mleem97/gregFramework/blob/master/wiki/src/data/moduleCatalog.ts) |
| Catalog overview | [`/mods`](/mods) |

## Maintainer checklist

- [ ] Stable: tag + Release notes + optional Workshop update.
- [ ] Beta: pre-release + link from module wiki page + `releaseReady: false` in catalog until promoted.
- [ ] Do not commit large binaries to `main` without a documented policy.

## Related

- [Repo inventory](../contributors/repo-inventory.md)
- [Mod store vision](./mod-store-vision.md)
