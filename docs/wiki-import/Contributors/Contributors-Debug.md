---
title: Contributors (Debug) EN
description: Framework contributor setup, hook workflow, conventions, CI behavior, and PR checklist.
sidebar_position: 140
tags:
  - audience:contributor
---

## Contributors (Debug)

This page is for contributors working on FrikaMF itself.

For the complete and maintainable feature matrix with implementation use cases, see [`Framework Features & Use Cases`](/wiki/wiki-import/Framework-Features-Use-Cases).

## Dev setup

```powershell
dotnet build .\framework\framework/FrikaMF.csproj -c Debug -nologo
cargo build --release
```

Optional game path override:

```powershell
dotnet build .\framework\framework/FrikaMF.csproj /p:GameDir="C:\Path\To\Data Center"
```

## Add a new hook workflow

1. Find target in dnSpy/dotPeek.
2. Update `HOOKS.md`.
3. Add Harmony patch.
4. Add event id + dispatch wiring.
5. Add/update Rust ABI event handling if needed.
6. Test locally.
7. Open PR.

## Conventions

- Keep ABI structs blittable.
- Keep wrappers in framework, gameplay policy in mods.
- Use stable event names and explicit contracts.

## IL2CPP pitfalls

- `b###` compiler-generated members are unstable.
- Coroutine state-machine types (`d##`) are unstable.
- Prefer Postfix first unless behavior blocking is required.

## Lua/Python/Web FFI contributor notes

Current core status:

- Rust native FFI bridge: implemented.
- Built-in Lua runtime host: not implemented.
- Built-in Python runtime host: not implemented.
- Built-in generic HTTP/WebSocket FFI transport: not implemented.

Contribution guidance:

- Treat Lua/Python as sidecar integration work unless core runtime hosting is explicitly added.
- Keep Unity and IL2CPP access constrained to C#/Rust boundaries.
- If adding transport, define strict command schemas, authentication for non-local access, and rate limits.

## CI behavior

- CI runs without game installation.
- `$(CI)=true` skips local game reference validation.
- Local builds require MelonLoader-generated interop files.

## PR checklist

- [ ] `HOOKS.md` updated
- [ ] Build passes locally
- [ ] Docs updated
- [ ] Conventional Commits used

## Example pair

### 🦀 Rust

```rust
#[repr(C)]
pub struct MoneyChanged {
    pub old_value: i32,
    pub new_value: i32,
}
```

### 🔷 C\#

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct MoneyChanged
{
    public int OldValue;
    public int NewValue;
}
```
