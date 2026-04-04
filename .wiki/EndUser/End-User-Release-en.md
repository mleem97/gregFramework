---
title: End-User (Release) EN
description: Installation, updates, troubleshooting, and uninstall guide for players using FrikaMF as a dependency.
sidebar_position: 120
tags:
  - audience:enduser
---

## End-User (Release)

This page is for players who do **not** develop mods and only install FrikaMF because another mod depends on it.

For a complete overview of framework capabilities and setup flows, see [`Framework Features & Use Cases`](Framework-Features-Use-Cases).

## What you install

- `FrikaModdingFramework.dll` is a runtime framework for `Data Center`.
- It provides stable runtime glue required by other mods.
- FrikaMF is **unofficial** and **community-driven**.

## Quick install

1. Install MelonLoader (IL2CPP, latest stable).
2. Launch the game once, then close it.
3. Put `FrikaModdingFramework.dll` into `Data Center/Mods`.
4. Put your actual mod into `Data Center/Mods`.
5. Start the game and verify `MelonLoader/Latest.log`.

## Troubleshooting

- Game crashes at startup: check `MelonLoader/Latest.log` for missing DLL/type errors.
- Mod not loading: verify correct folder and unblock files in Windows file properties.
- After game update: check compatibility page and wait for updated releases if hooks changed.

## Update

1. Close game.
2. Replace `FrikaModdingFramework.dll`.
3. Start game and verify logs.

## Uninstall

1. Close game.
2. Remove `FrikaModdingFramework.dll` from `Mods`.
3. Remove dependent mods.

## Related pages

- [Home EN](../Home-en)
- [ModDevs EN](../ModDevs/Index-en)
- [Contributors EN](../Contributors/Index-en)
- [FFI Bridge Reference](../FFI-Bridge-Reference)
- [Web UI Bridge EN (DC2WEB)](../Web-UI-Bridge-en)
- [Known Incompatibilities EN](../Known-Incompatibilities-en)

## Minimal context example (both language tracks)

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_info() -> *const i8 {
    b"example-rust\0".as_ptr() as *const i8
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
