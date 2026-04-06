---
title: End-User (Release)
description: Installation, Updates, Fehlerbehebung und Deinstallation für Nutzer von FrikaModdingFramework als Mod-Abhängigkeit.
sidebar_position: 20
tags:
  - audience:enduser
---

## End-User (Release)

Diese Seite ist für Spieler gedacht, die **keine Mods entwickeln**, sondern FrikaMF nur als Abhängigkeit für andere Mods nutzen.

Für einen vollständigen Überblick aller Framework-Funktionen und Use-Case-Flows siehe [`Framework Features & Use Cases`](/wiki/wiki-import/Framework-Features-Use-Cases).

## Was du installierst

- `FrikaModdingFramework.dll` ist ein Laufzeit-Framework für `Data Center`.
- Es wird von anderen Mods benötigt, damit deren Hooks/Events funktionieren.
- FrikaMF ist **inoffiziell** und **community-driven** (keine Affiliation mit WASEKU).

## Schnellstart (5 Minuten)

1. Installiere MelonLoader (IL2CPP, stabile Version).
2. Starte das Spiel einmal, beende es wieder.
3. Lege `FrikaModdingFramework.dll` in den Ordner `Data Center/Mods`.
4. Lege den eigentlichen Mod (der FrikaMF benötigt) ebenfalls in `Data Center/Mods`.
5. Starte das Spiel und prüfe `MelonLoader/Latest.log`.

## Installationsanleitung (ausführlich)

### 1) Voraussetzungen

- Installiertes Spiel: `Data Center`
- Schreibrechte im Spielverzeichnis
- Aktuelle Visual C++ Runtime (falls vom Mod verlangt)

### 2) MelonLoader installieren

- Nutze den offiziellen MelonLoader-Installer für IL2CPP.
- Nach der Installation das Spiel **einmal starten**, damit Interop-Dateien erzeugt werden.

### 3) FrikaMF platzieren

- Zielordner:

```text
<Data Center>\Mods\FrikaModdingFramework.dll
```

### 4) Mod installieren

- Den gewünschten Mod in denselben `Mods`-Ordner legen.
- Manche Mods benötigen zusätzlich Konfigurationsdateien (README des Mods prüfen).

## Ordnerstruktur prüfen

```text
Data Center/
├─ Mods/
│  ├─ FrikaModdingFramework.dll
│  ├─ <DeinMod>.dll
│  └─ RustMods/
└─ MelonLoader/
   └─ Latest.log
```

## Update-Anleitung

1. Spiel schließen.
2. Alte `FrikaModdingFramework.dll` ersetzen.
3. Falls im Release erwähnt: alte Configs sichern/löschen.
4. Spiel starten und Log prüfen.

## Neues Modsettings-Menü

Im Main Menu öffnet ein Klick auf `Settings` jetzt eine Auswahl:

- `Game Settings` (normale Spiel-Einstellungen)
- `Mod Settings` (FrikaMF Web/UI-Einstellungen)

In `Mod Settings` kannst du steuern, ob die Framework-UI aktiv webbasiert gestylt wird (`DC2WEB`) oder beim bisherigen Modernizer bleibt.

## Deinstallation

1. Spiel schließen.
2. `FrikaModdingFramework.dll` aus `Mods` entfernen.
3. Alle Mods entfernen, die FrikaMF voraussetzen.
4. Optional: `MelonLoader` komplett deinstallieren.

## Troubleshooting

### Spiel startet nicht / stürzt direkt ab

- Prüfe `MelonLoader/Latest.log` auf `MissingMethod`, `TypeLoadException`, `DllNotFound`.
- Entferne zuletzt installierte Mods testweise.
- Prüfe Versionskompatibilität (Spiel-Patch kann Hooks brechen).

### Mod lädt nicht

- Liegt die DLL wirklich in `Data Center/Mods`?
- Blockiert Windows die Datei (Datei > Eigenschaften > „Zulassen“)?
- Ist die Mod-Version für die aktuelle FrikaMF-Version gebaut?

### Nach Spiel-Update funktioniert nichts mehr

- Prüfe `Bekannte Inkompatibilitäten` im Wiki.
- Warte auf aktualisierte Mod-/FrikaMF-Releases.

## FAQ

### „Warum bricht mein Spiel nach Mod-Install?“

Häufigste Ursachen:

- Mod-Version passt nicht zum Spielstand.
- Veraltete Abhängigkeit oder defekte DLL.
- Hook-Ziel im Spiel wurde durch Update geändert.

### „Muss ich Rust und C# verstehen?“

Nein. Als End-User nicht. Du installierst nur die fertigen DLLs.

### „Kann ich FrikaMF alleine nutzen?“

Technisch ja, praktisch bringt es ohne abhängigen Mod kaum Mehrwert.

## Was du bei Supportanfragen mitschicken solltest

- Spielversion
- FrikaMF-Version
- Mod-Version
- Ausschnitt aus `MelonLoader/Latest.log`
- Schritte zur Reproduktion

## Relevante Querverweise

- [Home](../Home)
- [ModDevs](/wiki/wiki-import/ModDevs/)
- [Contributors](/wiki/wiki-import/Contributors/)
- [Framework Features & Use Cases](../Framework-Features-Use-Cases)
- [FFI Bridge Reference](../FFI-Bridge-Reference)
- [Web UI Bridge (DC2WEB)](../Web-UI-Bridge)
- [Bekannte Inkompatibilitäten](../Bekannte-Inkompatibilitaeten)

## Minimalbeispiel (nur zur Einordnung)

> Du musst diesen Code **nicht** schreiben. Er zeigt nur, dass FrikaMF Mods in beiden Sprachen ermöglicht.

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_info() -> *const i8 {
    b"example-rust-mod\0".as_ptr() as *const i8
}
```

### 🔷 C\#

```csharp
using MelonLoader;

public sealed class ExampleMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Example C# mod loaded");
    }
}
```
