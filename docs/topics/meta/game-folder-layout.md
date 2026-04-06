---
title: Game folder layout (FMF / MelonLoader)
sidebar_label: Game folder layout
description: Canonical paths for mod configs, FMF plugins, and MelonLoader mods under the Data Center game root.
---

# Game folder layout (FMF / MelonLoader)

This page is the **single reference** for where mod-related files live next to the game executable (`{GameRoot}`). MelonLoader sets `{GameRoot}/UserData` via `MelonEnvironment.UserDataDirectory`, `{GameRoot}/Mods` for **MelonMods**, and `{GameRoot}/Plugins` for **MelonPlugins** (e.g. `FMF.ModPathRedirector.dll`).

## Summary

| Inhalt | Pfad | Format / Hinweis |
|--------|------|------------------|
| **Mod-Konfiguration & Sidecars** | `{GameRoot}/UserData/ModCfg/` | **JSON** für Konfigurationsdateien; weitere Sidecar-Dateien (z. B. `custom_employees_hired.txt`) liegen ebenfalls hier, damit alles Mod-Bezogene an einem Ort liegt. |
| **FMF Framework-Plugins** (FFM.Plugin.*) | `{GameRoot}/FMF/Plugins/` | DLLs; **MelonLoader** lädt standardmäßig nur `{GameRoot}/Mods` — siehe unten. |
| **Plugins** (MelonLoader, z. B. ModPathRedirector) | `{GameRoot}/Plugins/` | MelonLoader `Plugins`-Ordner — nur **MelonPlugin**-DLLs. |
| **Mods** (MelonLoader, z. B. FMF.Mod.*) | `{GameRoot}/Mods/` | MelonLoader `Mods`-Ordner — **MelonMod**-DLLs. |

## UserData/ModCfg

- Alle **mod-relevanten** Konfigurationen und JSON-Sidecars werden unter **`UserData/ModCfg`** geführt.
- Beim ersten Start werden fehlende Dateien angelegt; bei bestehenden Installationen werden ältere Dateien aus **`UserData/`** (Root) nach **`ModCfg/`** übernommen, sofern noch vorhanden.
- Beispiele: `multiplayer-sync.config.json`, `pluginsync.config.json`.
- Framework-Metadaten (z. B. Save-Compat-Stamp) liegen unter **`UserData/ModCfg/FrikaFM/`** (Migration von `UserData/FrikaFM`).

## FMF/Plugins und MelonLoader

**FFM-Plugin-DLLs** liegen kanonisch unter **`{GameRoot}/FMF/Plugins`**. MelonLoader enumeriert **standardmäßig** nur **`Mods/`**. Praktische Optionen:

1. **DLLs zusätzlich** (oder verlinkt) **`Mods/`** ablegen — üblicher Weg für automatisches Laden.
2. **Unterordner** von `Mods` nutzen, falls eure MelonLoader-Version Mods in Unterverzeichnissen lädt (Version je nach Release prüfen).
3. **PluginSync**-Downloads des Frameworks landen unter **`FMF/Plugins/PluginSync/...`**.

## Mods (FMF-basiert)

Normale **MelonLoader-Mods** (einschließlich FMF-Mods) werden wie gewohnt in **`{GameRoot}/Mods/`** installiert.

## Siehe auch

- [Meta & operations](/wiki/topics/meta/overview)
- [Legacy: Mod Config System](/wiki/wiki-import/Mod-Config-System) (API-Contract; Pfade auf **ModCfg** beziehen)
