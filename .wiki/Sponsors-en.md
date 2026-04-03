---
title: Sponsors EN
description: Why FrikaMF sponsorship matters, support options, and where funding is used.
sidebar_position: 150
tags:
  - audience:sponsor
---

## Sponsors

FrikaMF is an unofficial, community-driven framework that improves modding stability for `Data Center`.

## Why support this project

- Faster compatibility recovery after game updates
- Better documentation and onboarding
- Shared runtime standards for mod authors

## Sponsorship options

- GitHub Sponsors
- Ko-fi (optional)
- One-time or recurring contributions

## Funding usage

- Maintenance and compatibility work
- Tooling/CI and release infrastructure
- Documentation quality

## Technical context sample

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_update(_dt: f32) {}
```

### 🔷 C\#

```csharp
using MelonLoader;

public sealed class SponsorDemoMod : MelonMod
{
    public override void OnUpdate() {}
}
```
