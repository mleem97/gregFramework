# Mod Config System (C# + Rust FFI)

Last updated: 2026-04-03

This guide describes the `ModConfigSystem` API surface for MelonLoader mods.

Status note:

- The API described here is based on the provided implementation spec from your runtime branch/discussion.
- In the current repository snapshot, direct source references for `ModConfigSystem` were not found.
- Treat this page as the target contract and verify runtime availability in your active game build.

## Overview

The Mod Config System allows mods to register, read, and write config entries that are:

- Persisted under `UserData/ModConfigs/<modId>.json`
- Editable at runtime through an IMGUI panel
- Saved immediately when changed

Supported logical types:

- `bool`
- `int`
- `float`

## Audience

- C# MelonLoader mod developers
- Rust native mod developers (through FFI wrappers exposed by the bridge)

## Accessing the panel

The settings panel can be opened by:

- Hotkey `F8`
- Main menu `Settings` -> `Mod Settings`

Expected behavior while panel is open:

- `Esc` closes panel or popup
- panel is draggable
- entries are scrollable
- game click-through should be blocked while focused

## C# quick start

```csharp
using DataCenterModLoader;

const string MOD_ID = "my_cool_mod";

ModConfigSystem.SetModInfo(MOD_ID, "YourName", "1.0.0");

ModConfigSystem.RegisterBoolOption(MOD_ID, "god_mode", "God Mode", false, "Prevents all damage");
ModConfigSystem.RegisterIntOption(MOD_ID, "move_speed", "Move Speed", 5, 1, 20, "Player move speed multiplier");
ModConfigSystem.RegisterFloatOption(MOD_ID, "gravity", "Gravity Scale", 1.0f, 0.0f, 3.0f, "World gravity multiplier");

bool godMode = ModConfigSystem.GetBoolValue(MOD_ID, "god_mode");
int speed = ModConfigSystem.GetIntValue(MOD_ID, "move_speed", 5);
float gravity = ModConfigSystem.GetFloatValue(MOD_ID, "gravity", 1.0f);
```

## API reference (contract)

All methods are expected on `DataCenterModLoader.ModConfigSystem`.

### Registration

- `RegisterBoolOption(modId, key, displayName, defaultValue, description = "")`
- `RegisterIntOption(modId, key, displayName, defaultValue, min, max, description = "")`
- `RegisterFloatOption(modId, key, displayName, defaultValue, min, max, description = "")`

Expected return:

- `true` when new entry was registered
- `false` on duplicate key/type mismatch/error

### Getters

- `GetBoolValue(modId, key, defaultValue = false)`
- `GetIntValue(modId, key, defaultValue = 0)`
- `GetFloatValue(modId, key, defaultValue = 0f)`

### Setters

- `SetBoolValue(modId, key, value)`
- `SetIntValue(modId, key, value)`
- `SetFloatValue(modId, key, value)`

Expected behavior:

- values are clamped to registration bounds where applicable
- successful writes persist immediately

### Metadata

- `SetModInfo(modId, author, version)`

### Panel control

- `OpenPanel()`
- `ClosePanel()`

### Queries

- `HasOption(modId, key)`
- `IsPanelVisible` (property)

## Low-level FFI-oriented API surface

For Rust/native bridge integration, the documented low-level functions are:

- `RegisterBool(modId, key, displayName, defaultValue, description) -> uint`
- `RegisterInt(modId, key, displayName, defaultValue, min, max, description) -> uint`
- `RegisterFloat(modId, key, displayName, defaultValue, min, max, description) -> uint`
- `GetBool(modId, key) -> uint`
- `GetInt(modId, key) -> int`
- `GetFloat(modId, key) -> float`

Sentinel behavior (as provided):

- register methods return `0` on failure, `1` on success
- `GetBool` returns `0xFFFFFFFF` if not found
- `GetInt` returns `0` if not found
- `GetFloat` returns `0f` if not found

## Expected file format

Path:

```text
<GameRoot>/UserData/ModConfigs/<modId>.json
```

Example shape:

```json
{
  "modId": "my_cool_mod",
  "author": "YourName",
  "version": "1.0.0",
  "entries": {
    "god_mode": {
      "type": "Bool",
      "displayName": "God Mode",
      "description": "Prevents all damage",
      "value": true,
      "default": false
    }
  }
}
```

## Full C# example mod

```csharp
using MelonLoader;
using DataCenterModLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ExampleConfigMod.ExampleMod), "Example Config Mod", "1.0.0", "AuthorName")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace ExampleConfigMod
{
    public class ExampleMod : MelonMod
    {
        private const string MOD_ID = "example_config_mod";

        public override void OnInitializeMelon()
        {
            ModConfigSystem.SetModInfo(MOD_ID, "AuthorName", "1.0.0");

            ModConfigSystem.RegisterBoolOption(MOD_ID, "enable_debug_overlay", "Debug Overlay", false, "Show FPS and debug info");
            ModConfigSystem.RegisterIntOption(MOD_ID, "max_servers", "Max Servers", 10, 1, 100, "Maximum servers to auto-manage");
            ModConfigSystem.RegisterFloatOption(MOD_ID, "time_scale", "Time Scale", 1.0f, 0.1f, 5.0f, "Game speed multiplier");
        }

        public override void OnUpdate()
        {
            bool showDebug = ModConfigSystem.GetBoolValue(MOD_ID, "enable_debug_overlay");
            int maxServers = ModConfigSystem.GetIntValue(MOD_ID, "max_servers", 10);
            float timeScale = ModConfigSystem.GetFloatValue(MOD_ID, "time_scale", 1.0f);

            Time.timeScale = timeScale;
            _ = showDebug;
            _ = maxServers;
        }
    }
}
```

## Design notes

- Register options early (`OnInitializeMelon`) so the panel reflects entries immediately.
- Read values live in update loops when behavior should react instantly.
- Keep `modId` stable to preserve file continuity.
- Prefer high-level wrappers in C# mods; use low-level API for cross-language bridge work.

## Related links

- [Framework Features & Use Cases](/wiki/wiki-import/Framework-Features-Use-Cases)
- [FFI Bridge Reference](/wiki/wiki-import/FFI-Bridge-Reference)
- [Mod-Developer (Debug)](Mod-Developer-Debug)
- [Mod-Developer (Debug) EN](Mod-Developer-Debug-en)
- [Contributors (Debug)](Contributors-Debug)
- [Contributors (Debug) EN](Contributors-Debug-en)
