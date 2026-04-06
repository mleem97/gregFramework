---
title: Bekannte Inkompatibilitäten
description: Übersicht typischer Brüche nach Spielupdates und empfohlene Gegenmaßnahmen.
sidebar_position: 90
tags:
  - audience:enduser
  - audience:moddev
  - audience:contributor
  - audience:sponsor
  - audience:gamedev
---

## Bekannte Inkompatibilitäten

## Warum Inkompatibilitäten auftreten

`Data Center`-Updates können Signaturen, Aufrufreihenfolgen oder interne Klassen verändern. Dadurch können Hooks in FrikaMF und abhängigen Mods brechen.

## Häufige Symptome

- `TypeLoadException`
- `MissingMethodException`
- Harmony-Patches greifen nicht mehr
- UI-/Gameplay-Regressionen in modifizierten Flows

## Schnellmaßnahmen

1. Spiel und Mod-Versionen dokumentieren.
2. `MelonLoader/Latest.log` sichern.
3. `HOOKS.md` auf bekannte Änderungen prüfen.
4. FrikaMF/Mod auf neueste kompatible Version aktualisieren.

## Maintainer-Checkliste nach Spielpatch

- [ ] Kernhooks in `HarmonyPatches.cs` smoke-testen
- [ ] `HOOKS.md` Verifikationsstatus aktualisieren
- [ ] Event-IDs/Dispatcher-Pfade prüfen
- [ ] Rust ABI-Kompatibilität validieren

## Kompatibilitätstabelle (Vorlage)

| Spielversion | FrikaMF-Version | Status | Hinweise |
| :--- | :--- | :--- | :--- |
| TBD | TBD | Ungeprüft | Nach erstem Smoke-Test aktualisieren |

## Technische Diagnosebeispiele (beide Sprachen)

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_on_scene_loaded(scene_name: *const i8) {
    if scene_name.is_null() {
        return;
    }
}
```

### 🔷 C\#

```csharp
public override void OnSceneWasLoaded(int buildIndex, string sceneName)
{
    LoggerInstance.Msg($"Scene: {sceneName}, Build: {buildIndex}");
}
```
