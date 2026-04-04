---
title: Changelog & Versionen
description: Versionshistorie und Release-Hinweise von FrikaModdingFramework.
sidebar_position: 80
tags:
  - audience:enduser
  - audience:moddev
  - audience:contributor
  - audience:sponsor
  - audience:gamedev
---

## Changelog & Versionen

Die verbindliche Versionshistorie liegt in der Repository-Datei:

- [`CHANGELOG.md`](https://github.com/mleem97/FrikaModFramework/blob/master/CHANGELOG.md)

## Versionierung

- Framework-Releaseformat: `XX.XX.XXXX`
- Single Source of Truth: `FrikaMF/ReleaseVersion.cs`

## Release-Prozess (Kurz)

- Metadaten/Changelog-Bump über Script:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump major
pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump medium
pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump minor
```

- Lokales Publish-Artefakt:

```text
FrikaModdingFramework-v{XX.XX.XXXX}.dll
```

## Upgrade-Empfehlung

1. Release Notes prüfen.
2. Kompatibilität mit Spielversion und abhängigen Mods prüfen.
3. Framework aktualisieren.
4. Logs (`MelonLoader/Latest.log`) nach Warnungen scannen.

## Technisches Referenzpaar (beide Sprachen)

### 🦀 Rust

```rust
pub const ABI_VERSION: u32 = 5;
```

### 🔷 C\#

```csharp
public const uint API_VERSION = 5;
```
