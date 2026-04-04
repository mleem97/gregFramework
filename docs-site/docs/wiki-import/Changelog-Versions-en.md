---
title: Changelog & Versions EN
description: Release history and versioning reference for FrikaMF.
sidebar_position: 180
tags:
  - audience:enduser
  - audience:moddev
  - audience:contributor
  - audience:sponsor
  - audience:gamedev
---

## Changelog & Versions

Primary history:

- [`CHANGELOG.md`](https://github.com/mleem97/FrikaModFramework/blob/master/CHANGELOG.md)

## Versioning

- Framework format: `XX.XX.XXXX`
- Source of truth: `FrikaMF/ReleaseVersion.cs`

## Release metadata commands

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump major
pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump medium
pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump minor
```

## Example constants

### 🦀 Rust

```rust
pub const ABI_VERSION: u32 = 5;
```

### 🔷 C\#

```csharp
public const uint API_VERSION = 5;
```
