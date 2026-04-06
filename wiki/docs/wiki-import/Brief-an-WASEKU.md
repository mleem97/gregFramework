---
title: Brief an WASEKU (Data Center)
description: Offene, technische Einordnung von FrikaMF für den Spielentwickler inkl. Kooperationsangebot und Rechtsrahmen.
sidebar_position: 60
tags:
  - audience:gamedev
---

## Brief an WASEKU (Data Center)

Sehr geehrtes Data-Center-Team,

`FrikaModdingFramework` (FrikaMF) ist ein **inoffizielles**, community-getriebenes Framework, das Spielerinnen und Spielern hilft, Mods für `Data Center` konsistenter und sicherer zu nutzen.

## Unser Ziel

- Verbesserung der Spielerfahrung im Rahmen von Singleplayer-/Offline-Modding
- Saubere technische Schnittstellen statt unkoordinierter Einzelpatches
- Reduzierung von Supportlast durch standardisierte Hook-/Event-Pfade

## Was FrikaMF ausdrücklich nicht tut

- Kein Umgehen von Anti-Cheat
- Kein Fokus auf kompetitive oder monetäre Exploits
- Keine Veränderung von Online-/Mehrspieler-Fairness als Projektziel

## Patch-Scope

FrikaMF fokussiert in erster Linie gameplaynahe, lokale Klassen/Flows und arbeitet transparent mit dokumentierten Hook-Zielen.

## Interoperabilität und Reverse Engineering

FrikaMF nutzt technische Analyse zur Interoperabilität. In der EU/DE ist das im vorgesehenen Rahmen zulässig, u. a. unter:

- Art. 6 EU Software Directive (Interoperabilität)
- § 69e UrhG (Dekompilierung zur Herstellung von Interoperabilität)

## Angebot zur Zusammenarbeit

Wir sind offen für:

- Abstimmung über stabile Modding-Schnittstellen
- Hinweise auf besonders sensible Klassen/Bereiche
- Dialog über eine mögliche offizielle Modding-API

## Kontakt

- Repository: `https://github.com/mleem97/gregFramework`
- Maintainer-Kontakt über GitHub Issues/Discussions

## Technischer Anhang (Kurzbeispiel)

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_on_scene_loaded(_scene_name: *const i8) {
    // runtime callback from framework bridge
}
```

### 🔷 C\#

```csharp
using MelonLoader;

public sealed class SceneObserver : MelonMod
{
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        LoggerInstance.Msg($"Scene loaded: {sceneName}");
    }
}
```

