---
title: Lizenz & Rechtliches
description: Lizenzmodell, Haftungsausschluss und rechtliche Einordnung von Interoperabilitätsarbeit.
sidebar_position: 100
tags:
  - audience:enduser
  - audience:moddev
  - audience:contributor
  - audience:sponsor
  - audience:gamedev
---

## Lizenz & Rechtliches

## Lizenz

- Es gilt die projektweite Lizenz in [`LICENSE.txt`](https://github.com/mleem97/gregFramework/blob/master/LICENSE.txt).
- FrikaMF ist ein **inoffizielles Community-Projekt**.
- Keine Affiliation mit WASEKU.

## Haftungsausschluss

- Nutzung auf eigenes Risiko.
- Keine Gewähr für dauerhafte Kompatibilität mit zukünftigen Spielversionen.

## Interoperabilität

FrikaMF nutzt Reverse-Engineering-Techniken zur Interoperabilität.
Rechtlicher Bezug (EU/DE):

- EU Software Directive, Art. 6
- § 69e UrhG

## Compliance-Leitlinien für Modder

- Keine Umgehung von Sicherheits-/Schutzmechanismen.
- Keine Verbreitung fremder urheberrechtlich geschützter Inhalte ohne Rechte.
- Keine Nutzung für betrügerische Online-Vorteile.

## Technischer Mini-Contract (beide Sprachen)

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_shutdown() {
    // release native resources
}
```

### 🔷 C\#

```csharp
public override void OnApplicationQuit()
{
    LoggerInstance.Msg("Framework shutdown");
}
```

