---
title: Monorepo — Getting started
sidebar_label: Monorepo getting started
description: Target layout (FrikaModFramework, templates, tools) and how it maps to this repo today.
---

# Monorepo — Getting started

The **goal** is a clean separation between the framework (`FrikaModFramework/`), standalone mods (`HexMod/`, …), templates (`templates/`), and tooling (`tools/`). The live MelonLoader framework is still under [`framework/`](https://github.com/mleem97/gregFramework/tree/master/framework) while we migrate incrementally.

## Clone and build the framework

```text
dotnet build framework/FrikaMF.csproj
```

Or open `FrikaMF.sln` in Visual Studio.

## Hook naming

- **Target convention:** `FMF.<DOMAIN>.<Event>` (see [`CONTRIBUTING.md`](https://github.com/mleem97/gregFramework/blob/master/CONTRIBUTING.md)).
- **Registry:** [`FrikaModFramework/fmf_hooks.json`](https://github.com/mleem97/gregFramework/blob/master/FrikaModFramework/fmf_hooks.json).
- **Legacy runtime strings** may still use `FFM.*` in [`HookNames`](https://github.com/mleem97/gregFramework/blob/master/framework/FrikaMF/HookNames.cs) until migrated.

## Create a mod from the template

1. Copy [`templates/mod/`](https://github.com/mleem97/gregFramework/tree/master/templates/mod) to a new folder.
2. Edit `fmf/hooks.json` and add your sources under `src/`.
3. See [`HexMod/`](https://github.com/mleem97/gregFramework/tree/master/HexMod) for a pilot layout.

## Documentation site

Wiki content: [`docs/`](https://github.com/mleem97/gregFramework/tree/master/docs). Docusaurus app: [`wiki/`](https://github.com/mleem97/gregFramework/tree/master/wiki) (`npm run build` inside `wiki/`).
