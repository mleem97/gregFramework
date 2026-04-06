---
title: Sponsoren
description: Warum FrikaMF unterstützt werden sollte, wie Sponsoring funktioniert und wofür Mittel eingesetzt werden.
sidebar_position: 50
tags:
  - audience:sponsor
---

## Sponsoren

## Was ist FrikaMF?

`FrikaModdingFramework` ist ein community-getriebenes, inoffizielles Modding-Framework für `Data Center`.
Es hilft, dass Mods stabiler, kompatibler und leichter wartbar werden.

## Warum ist das nützlich?

- Weniger Frust nach Spielupdates
- Klarere Schnittstellen für Mod-Autoren
- Einheitlicher Unterbau für C#- und Rust-Mods
- Schnellere Fehlerbehebung dank gemeinsamer Standards

## Sponsoring-Optionen

- GitHub Sponsors (primär)
- Ko-fi / ähnliche Plattformen (optional)
- Einmalige oder wiederkehrende Unterstützung

> Konkrete Links können im Repo-Profil gepflegt werden.

## Wofür werden Mittel genutzt?

- Wartung nach Spielupdates
- Testaufwand und Tooling
- Dokumentation/Onboarding
- Infrastruktur (CI/CD, Artefaktbereitstellung)

## Transparenz

- Kein offizielles Produkt von WASEKU
- Keine Paywall für Kernfunktionen
- Fokus auf Stabilität, Sicherheit und Dokumentation

## Hall of Fame (freiwillig)

- Sponsor-Nennung auf Wunsch in dieser Seite
- Optional mit Link/Handle

## Technisches Mini-Beispiel (zur Einordnung)

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_update(_delta_time: f32) {
    // framework-backed periodic update
}
```

### 🔷 C\#

```csharp
using MelonLoader;

public sealed class SponsorDemoMod : MelonMod
{
    public override void OnUpdate()
    {
        // framework-backed periodic update
    }
}
```
