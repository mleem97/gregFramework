---
title: Glossary EN
description: FrikaMF and IL2CPP terminology for all audiences.
sidebar_position: 170
tags:
  - audience:enduser
  - audience:moddev
  - audience:contributor
  - audience:sponsor
  - audience:gamedev
---

## Glossary

### IL2CPP

Unity backend converting managed code to native binaries.

### Interop Assembly

Generated assemblies exposing metadata/signatures for `Il2Cpp.*` access.

### HarmonyX

Runtime patching framework for Prefix/Postfix/Transpiler patterns.

### C-ABI

Stable binary boundary for C# ↔ Rust interoperability.

### Blittable Types

Types copied between managed/unmanaged memory without transformation.

### RID/Token

Metadata identifiers useful for traceability but limited in IL2CPP runtime mapping.

### Example pair

#### 🦀 Rust

```rust
#[repr(C)]
pub struct TickInfo {
    pub dt: f32,
}
```

#### 🔷 C\#

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct TickInfo
{
    public float Dt;
}
```
