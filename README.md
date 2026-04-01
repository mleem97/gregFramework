# DataCenter Asset Exporter Mod (MelonLoader)

> **Wichtiger Hinweis (Ethik & Nutzung):**
> Dieser Mod ruft **nicht** dazu auf, Code, Assets oder sonstige Inhalte von Indie-Entwickler:innen zu stehlen, unrechtmäßig weiterzuverwenden oder zu verbreiten.
> Er wurde entwickelt, um die **Mod-Entwicklung für MelonLoader** auf Basis von **Data Center (Waseku)** zu unterstützen – insbesondere für Mods, die tiefer eingreifen als reiner Code (z. B. eigene Assets/Erweiterungen im legitimen Modding-Kontext).

## Überblick

`DataCenterExporter` ist ein MelonLoader-Mod für `Data Center`, der zur Laufzeit verwendete Inhalte (Meshes/Texturen) für Modding-Workflows exportiert.

Aktueller Fokus:

- Export von **im Spiel verwendeten Assets** (aktive + inaktive Objekte in geladenen Szenen)
- Optionaler Export von geladenen, aber aktuell nicht verwendeten Assets (`NotUsed`)
- UI-Hilfsfunktionen zur schnelleren Referenzsuche
- Erweiterte Metadaten-Exporte (Komponenten, Objekt-Settings, Material-Infos, Summary)

## Features

- `F8`: Startet den Exportlauf
- `F9`: Loggt den UI-Pfad unter dem Mauszeiger
- `F10`: Schaltet Beta-Export (`an/aus`)

Exportziele:

- `Mods/ExportedAssets/CurrentGame`
  - `Models`
  - `Textures`
  - `Sprites`
  - `Materials`
  - `Scripts` (`components.txt`)
  - `Settings` (`objects.txt`, `summary.txt`)
- `Mods/ExportedAssets/CurrentGame/NotUsed`
  - Optional: Nicht verwendete, aber geladene Assets (aufgeteilt in `Models` und `Textures`)

Zusätzlich wird eine `README_NOT_USED.txt` im `CurrentGame`-Ordner erzeugt.

## Technologie-Stack

- `.NET 6`
- `MelonLoader`
- `Unity IL2CPP` Interop
- `Unity Input System`

## Voraussetzungen

- Installiertes Spiel: `Data Center`
- Installierter `MelonLoader` für das Spiel
- `Visual Studio 2022/2026` oder `dotnet SDK` mit .NET 6

## Build

Im Projektordner:

```powershell
dotnet build DataCenterExporter.sln -v:minimal
```

Ausgabe typischerweise in:

- `bin/Debug/net6.0/DataCenterExporter.dll`

## Installation

1. Projekt bauen
2. `DataCenterExporter.dll` in den `Mods`-Ordner von `Data Center` kopieren
3. Spiel starten
4. Im Spiel die Hotkeys (`F8/F9/F10`) verwenden

## Laufzeitverhalten (Kurz)

- Export nutzt Szenen-Hierarchien (inkl. inaktive Objekte), um „wirklich verbaute“ Inhalte zu erfassen.
- UI-Erkennung für `F9` erfolgt robust via Reflection, damit fehlende direkte UI-Assembly-Referenzen im Mod-Build kein Hard-Blocker sind.
- `NotUsed` wird gefiltert, damit primär relevante Asset-Kandidaten exportiert werden (keine offensichtlichen internen/mini-Assets).

## Projektstruktur

- `Main.cs` – zentrale Mod-Logik (Hotkeys, Export, UI-Logging)
- `AssetExport.md` – Anforderungs-/Notizdatei zum Exportverhalten
- `ui.md` – UI-Referenzkontext

## Contribution

Beiträge sind willkommen.

### Ablauf

1. Repository forken
2. Branch erstellen (`feature/...`, `fix/...`)
3. Änderungen klein und fokussiert halten
4. Build lokal prüfen
5. Pull Request mit klarer Beschreibung erstellen

### Beitrag-Richtlinien

- Keine Änderungen einbringen, die auf Urheberrechtsverletzungen oder Asset-Diebstahl abzielen.
- Exporte/Features müssen den legitimen Modding-Zweck unterstützen.
- Bestehenden Stil und Architektur beibehalten.
- Keine unnötigen Abhängigkeiten hinzufügen.

## Issues & Bug Reports

Bitte bei Issues angeben:

- Spielversion
- MelonLoader-Version
- Mod-Version / Commit
- Reproduktionsschritte
- Relevante Logs / Fehlermeldungen

## Security Policy

Bitte keine Sicherheitslücken öffentlich als Issue posten.
Melde sie verantwortungsvoll über einen privaten Kanal (Maintainer kontaktieren).

## Code of Conduct

- Respektvoller Umgang
- Konstruktives Feedback
- Keine diskriminierenden oder beleidigenden Inhalte

Bei wiederholten Verstößen können Beiträge/Interaktionen eingeschränkt werden.

## Lizenz

Dieses Projekt steht unter der **MIT-Lizenz**.
Siehe `LICENSE.txt`.

## Haftungsausschluss

Dieses Projekt ist ein Community-Modding-Werkzeug.
Es besteht keine Verbindung zu oder offizielle Unterstützung durch die Entwickler von `Data Center`.
