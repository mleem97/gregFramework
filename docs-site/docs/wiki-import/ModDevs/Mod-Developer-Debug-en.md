---
title: Mod-Developer (Debug) EN
description: Rust vs C# decision guide, getting started for both tracks, hook discovery, architecture, and API orientation.
sidebar_position: 130
tags:
  - audience:moddev
---

## Mod-Developer (Debug)

You only need one track: **Rust** or **C#**. FrikaMF bridges runtime communication.

Full framework capability catalog with setup guides: [`Framework Features & Use Cases`](/wiki-import/Framework-Features-Use-Cases).

## Rust vs C# decision guide

| Criteria | 🔷 C# Track | 🦀 Rust Track |
| --- | --- | --- |
| Onboarding speed | Fast | Medium |
| Direct Unity/Il2Cpp access | Strong | Indirect |
| Native-level control | Medium | High |
| Safety model | Medium | High |
| Recommended for | Most gameplay mods | Performance/ABI-heavy systems |

## Lua/Python/Web FFI status

- Rust FFI in framework core: **implemented**.
- Built-in Lua runtime host: **not implemented**.
- Built-in Python runtime host: **not implemented**.
- Built-in generic HTTP/WebSocket FFI transport: **not implemented**.

Recommended approach:

- Run Lua/Python as a sidecar process and connect through your C# or Rust mod boundary.
- Use framework events as stable inputs and framework APIs as safe outputs.
- Keep Unity/IL2CPP object access in C# or Rust layers.

For full DE/EN step-by-step tutorials per FFI entrypoint, see:

- [Lua FFI — How to Start Developing (DE + EN)](/wiki-import/Lua-FFI-Start-Developing)

## Architecture

```text
Data Center (IL2CPP)
  ↓ HarmonyX Patch
FrikaMF C# Bridge (Il2Cpp objects -> C-ABI structs)
  ↓ P/Invoke / C-ABI                    ↓ MelonLoader API
Rust Mod (.dll)                         C# Mod (.dll)
```

## Source of truth for hooks

- [`HOOKS.md`](/wiki-import/HOOKS)

## C# track quick start

```powershell
dotnet build .\FrikaMF.csproj /p:GameDir="C:\Path\To\Data Center"
```

```csharp
using HarmonyLib;
using MelonLoader;
using Il2Cpp;

[HarmonyPatch(typeof(Server), nameof(Server.PowerButton))]
public static class Patch_Server_PowerButton
{
    public static void Prefix(Server __instance)
    {
        MelonLogger.Msg($"Server power toggle: {__instance.name}");
    }
}
```

## Rust track quick start

```powershell
cargo build --release
```

```rust
#[no_mangle]
pub extern "C" fn mod_init(_api_table: *mut core::ffi::c_void) -> bool {
    true
}
```

## dnSpy / dotPeek guidance

- Open generated `Assembly-CSharp.dll` interop output.
- Validate signatures and call context.
- Document candidates in `HOOKS.md`.
- Implement Harmony patch and event dispatch.

## Why many IL2CPP interop methods look empty

Interop assemblies often contain metadata-facing stubs; real implementation lives in native IL2CPP binaries.

## Web FFI vs Web UI (important)

- `DC2WebBridge` provides Unity-side UI adaptation/styling.
- It is not a generic network FFI transport bus.
- For Web FFI, implement your own HTTP/WebSocket gateway with validation and rate limits.

Config API reference: [`Mod Config System`](/wiki-import/Mod-Config-System)

## Cross-track example

### 🦀 Rust

```rust
#[no_mangle]
pub extern "C" fn mod_on_event(event_id: u32, _ptr: *const u8, _len: u32) {
    if event_id == 1001 {
    }
}
```

### 🔷 C\#

```csharp
[HarmonyPatch(typeof(CustomerBase), nameof(CustomerBase.AreAllAppRequirementsMet))]
public static class Patch_Requirements
{
    public static void Postfix(bool __result)
    {
        MelonLogger.Msg($"Requirements met: {__result}");
    }
}
```
