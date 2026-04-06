---
title: Glossar
description: Begriffe rund um FrikaMF, Unity IL2CPP, Hooking und Rust/C#-Interoperabilität.
sidebar_position: 70
tags:
  - audience:enduser
  - audience:moddev
  - audience:contributor
  - audience:sponsor
  - audience:gamedev
---

## Glossar

## IL2CPP

Unity-Backend, das C#-Code nach C++/nativem Code transformiert. Viele Analysen basieren auf Interop-Metadaten statt auf originalem Managed Code.

## Interop Assembly

Von MelonLoader/Il2CppInterop erzeugte Assemblies mit Typ-/Signaturinformationen für `Il2Cpp.*`-Klassen.

## `Il2Cpp.`-Prefix

Namensraumkonvention für interop-gebundene Spieltypen (z. B. `Il2Cpp.Server`).

## HarmonyX

Patch-Framework (Prefix/Postfix/Transpiler), um Methoden zur Laufzeit zu instrumentieren.

## C-ABI

Stabile binäre Aufrufkonvention für Sprachgrenzen hinweg (hier C# ↔ Rust).

## P/Invoke

Mechanismus in .NET, um native Funktionen aufzurufen (`DllImport`, Delegate-Mapping, etc.).

## RID/Token

Metadata-Identifikatoren aus .NET/IL-Welt; bei IL2CPP-Kontext nur bedingt direkt nutzbar, aber zur Orientierung hilfreich.

## Blittable Typen

Datentypen, die ohne Transformation zwischen managed/unmanaged Speicher kopiert werden können.

## `GameContext`

Begriff für den gemeinsam genutzten Laufzeitkontext/API-Zugriff eines Mods; konkret in FrikaMF über API-Tabellen/Bridge-Objekte abgebildet.

## Prefix/Postfix

- **Prefix:** Läuft vor Originalmethode, kann diese optional unterdrücken.
- **Postfix:** Läuft nach Originalmethode, geeignet für Beobachtung/Erweiterung.

## Beispiel (beide Sprachen)

### 🦀 Rust

```rust
#[repr(C)]
pub struct TickInfo {
    pub dt: f32,
}
```

### 🔷 C\#

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct TickInfo
{
    public float Dt;
}
```
