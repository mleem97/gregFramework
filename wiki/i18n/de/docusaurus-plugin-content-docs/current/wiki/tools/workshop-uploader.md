---
title: WorkshopUploader
sidebar_label: WorkshopUploader
description: Windows-Desktop-App zum Verwalten von Steam-Workshop-Projekten und Metadaten für Data Center (FrikaMF).
---

# WorkshopUploader

Der **WorkshopUploader** ist eine **.NET MAUI**-Desktop-Anwendung für **Windows**. Sie hilft dir, **Workshop-Inhalte** für *Data Center* vorzubereiten: Ordnerstruktur, `metadata.json`, Vorschau-Bild und Upload über die **Steamworks**-API (Steam muss laufen und das Spiel sein App-ID-Kontext sein).

## Was die App macht

- Legt einen Arbeitsbereich **`DataCenterWS`** im Benutzerprofil an (Pfad siehe unten).
- Listet darunter **Projektordner** auf; jedes Projekt kann einen Unterordner **`content/`** haben — genau dieser Inhalt wird ins Workshop-Item hochgeladen.
- Pro Projekt bearbeitest du **Titel**, **Beschreibung**, **Sichtbarkeit** (Public / FriendsOnly / Private) und ein **Vorschau-Bild**; die Daten landen in **`metadata.json`**.
- Über **„Publish to Steam“** wird ein **neues** Workshop-Item angelegt oder ein bestehendes (wenn eine **Published File ID** gespeichert ist) aktualisiert.

## Voraussetzungen

- **Windows** (MAUI-Target für das Projekt).
- **Steam** mit angemeldetem Account, der **Data Center** besitzt bzw. für Workshop-Uploads berechtigt ist (App-ID **4170200**).
- Optional: gebaute **`WorkshopUploader.exe`** neben der Spielinstallation (siehe [Bauen und bereitstellen](#build-deploy)).

## Workspace-Pfad

Der Arbeitsbereich ist fest auf den Ordner **`DataCenterWS`** unter deinem Benutzerprofil gelegt, z. B.:

`%USERPROFILE%\DataCenterWS`

Beim ersten Start legt die App die Struktur an und kann eine **Beispieldatei** `metadata.sample.json` unter `.templates\` ablegen.

## Projektstruktur anlegen

Für jedes Workshop-Projekt:

1. Lege unter `DataCenterWS` einen **Ordner** an (Name = Anzeigename in der Liste).
2. Lege darin einen Unterordner **`content\`** an und packe dort die Dateien, die ins Workshop-Item sollen (Mod-Daten, Assets — nur **eigene** Inhalte, keine Spiel-Binaries weiterverbreiten).
3. Optional: **`metadata.json`** selbst anlegen oder über die App ausfüllen; die App speichert u. a. Titel, Beschreibung, Sichtbarkeit, Pfad zur Vorschau und nach dem ersten Upload die **Published File ID**.
4. Optional: **`preview.png`** im Projektroot (oder gewählter relativer Pfad in den Metadaten) — über die App kannst du ein Bild auswählen; es wird als `preview.png` ins Projekt kopiert.

Ohne **`content/`** zeigt die Übersicht einen Hinweis („Missing content/“); ein Upload ist dann nicht möglich.

## Bedienung in der App

1. **Start:** Übersicht **„Workshop projects“** — oben siehst du den **Workspace-Pfad**. Liste per **Pull-to-refresh** aktualisieren.
2. **Projekt öffnen:** Eintrag antippen → **Editor**.
3. **Editor:** Titel und Beschreibung (mit Zeichenlimits wie bei Steam), **Visibility**, **Vorschau wählen**, dann:
   - **„Save metadata.json“** — nur speichern.
   - **„Publish to Steam“** — speichert und lädt **`content/`** hoch; beim ersten Mal entsteht ein neues Workshop-Item, danach wird die gespeicherte **File ID** wiederverwendet.
4. **Log** auf der Startseite protokolliert Meldungen (u. a. Steam-Init, Upload-Fortschritt).

Wenn Steam nicht initialisiert werden kann (z. B. Steam nicht gestartet), weist die App darauf hin.

## Bauen und bereitstellen {#build-deploy}

Aus dem Repository:

```bash
dotnet build WorkshopUploader/WorkshopUploader.csproj -c Debug
```

Release (typisch einzelne EXE unter Windows):

```bash
dotnet publish WorkshopUploader/WorkshopUploader.csproj -c Release
```

Ausgabe u. a. unter `WorkshopUploader\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\WorkshopUploader.exe` (exakte Pfade je nach SDK/TFM).

Zum Ablegen **neben das Spiel** (nicht unter `Mods`/`MelonLoader`):

`{SpielRoot}\WorkshopUploader\`

## Weiterführende Links

- Repo-README: [`WorkshopUploader/README.md`](https://github.com/mleem97/gregFramework/blob/master/WorkshopUploader/README.md)
- Hintergrund Workshop & Tooling: [Steam Workshop and Tooling](/wiki/meta/Steam-Workshop-and-Tooling)
- DevServer-Betas (Kontext `gregframework.eu`): [DevServer betas](/wiki/meta/devserver-betas)
