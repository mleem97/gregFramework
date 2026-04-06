---
title: Known Incompatibilities EN
description: Known breakages after game updates and recommended mitigation steps.
sidebar_position: 190
tags:
  - audience:enduser
  - audience:moddev
  - audience:contributor
  - audience:sponsor
  - audience:gamedev
---

## Known Incompatibilities

Game updates may change signatures and hook points, breaking framework-level patches.

## Common symptoms

- `MissingMethodException`
- `TypeLoadException`
- Harmony patch not firing
- Runtime regressions in modded flows

## Immediate checks

1. Capture `MelonLoader/Latest.log`.
2. Confirm game/framework/mod versions.
3. Review [`HOOKS.md`](/wiki/wiki-import/HOOKS) and compatibility notes.

## Compatibility matrix template

| Game Version | FrikaMF Version | Status | Notes |
| :--- | :--- | :--- | :--- |
| TBD | TBD | Unverified | Update after smoke validation |

## Example diagnostics

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_on_scene_loaded(scene_name: *const i8) {
    if scene_name.is_null() {
        return;
    }
}
```

### 🔷 C\#

```csharp
public override void OnSceneWasLoaded(int buildIndex, string sceneName)
{
    LoggerInstance.Msg($"Scene={sceneName}, Build={buildIndex}");
}
```
