---
title: Mod-Developer (Debug)
description: Entscheidungshilfe Rust vs. C#, Getting Started für beide Tracks, Hook-Recherche, Architektur und API-Orientierung.
sidebar_position: 30
tags:
  - audience:moddev
---

## Mod-Developer (Debug)

Diese Seite richtet sich an Entwickler, die eigene Mods auf Basis von FrikaMF bauen.

## Kurz gesagt

- Du musst **nicht** beide Sprachen lernen.
- Wähle **einen** Track: Rust **oder** C#.
- FrikaMF übernimmt die Brücke zwischen Spiel, Hooks, Events und ggf. FFI.

## Rust oder C#? (Entscheidungshilfe)

| Kriterium | 🔷 C# Track (MelonLoader/HarmonyX) | 🦀 Rust Track (C-ABI/FFI) |
| --- | --- | --- |
| Einstiegsgeschwindigkeit | Sehr schnell | Mittel |
| Unity/Il2Cpp Zugriff | Direkt und komfortabel | Indirekt über API/Events |
| Performance-kritische Logik | Gut | Sehr gut |
| Memory-Safety | Mittel | Hoch |
| Tooling-Komplexität | Niedrig | Mittel bis hoch |
| Empfehlung | Für die meisten Mod-Ideen | Für komplexe Systeme/Engine-nahe Logik |

**Empfehlung:** Starte mit C#, wenn du neu im Stack bist. Nutze Rust, wenn du bewusst FFI-Kontrolle, klare ABI-Verträge oder native Performance brauchst.

## Architekturüberblick

```text
Data Center (IL2CPP)
  ↓ HarmonyX Patch
FrikaMF C# Bridge (Il2Cpp-Objekte -> C-ABI-Structs)
  ↓ P/Invoke / C-ABI                    ↓ MelonLoader API
Rust Mod (.dll)                         C# Mod (.dll)
```

## Quelle der Wahrheit für Hooks

- Verifizierte Hook-Ziele: [`HOOKS.md`](HOOKS)
- Runtime-Patches: `FrikaMF/JoniMF/HarmonyPatches.cs`

## Getting Started: 🔷 C# Track

### Voraussetzungen (Rust)

- .NET SDK 6+
- MelonLoader IL2CPP Setup
- Spiel einmal mit MelonLoader gestartet

### Build (Rust)

```powershell
dotnet build .\FrikaMF.csproj /p:GameDir="C:\Pfad\zu\Data Center"
```

Alternative mit Umgebungsvariable:

```powershell
$env:DATA_CENTER_GAME_DIR = "C:\Pfad\zu\Data Center"
dotnet build .\FrikaMF.csproj
```

### Minimaler C# Mod

```csharp
using HarmonyLib;
using MelonLoader;
using Il2Cpp;

public sealed class HelloCSharpMod : MelonMod
{
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Hello from C# track");
    }
}

[HarmonyPatch(typeof(Server), nameof(Server.PowerButton))]
public static class Patch_Server_PowerButton
{
    public static void Prefix(Server __instance)
    {
        MelonLogger.Msg($"Server power toggle: {__instance.name}");
    }
}
```

## Getting Started: 🦀 Rust Track

### Voraussetzungen

- Rust stable toolchain
- `cargo`
- DLL-Export per C-ABI (`#[no_mangle] extern "C"`)

### Build

```powershell
cargo build --release
```

### Minimaler Rust Mod

```rust
#[repr(C)]
pub struct ModInfoFFI {
    pub id: *const i8,
    pub name: *const i8,
    pub version: *const i8,
    pub author: *const i8,
    pub description: *const i8,
}

#[no_mangle]
pub extern "C" fn mod_info() -> ModInfoFFI {
    ModInfoFFI {
        id: b"hello_rust\0".as_ptr() as *const i8,
        name: b"Hello Rust Mod\0".as_ptr() as *const i8,
        version: b"0.1.0\0".as_ptr() as *const i8,
        author: b"you\0".as_ptr() as *const i8,
        description: b"minimal rust example\0".as_ptr() as *const i8,
    }
}

#[no_mangle]
pub extern "C" fn mod_init(_api_table: *mut core::ffi::c_void) -> bool {
    true
}
```

## Hook-Recherche mit dnSpy / dotPeek

1. Öffne `Assembly-CSharp.dll` aus den IL2CPP-Interop-Ausgaben.
2. Suche Klassen/Methoden nach Gameplay-Relevanz.
3. Prüfe Signaturen, Parameter und Seiteneffekte.
4. Lege Kandidaten in `HOOKS.md` ab (mit Verifikationsstatus).
5. Implementiere Patch in `HarmonyPatches.cs`.

### Warum sind Methodenbodies oft leer?

Bei IL2CPP-Interop-Assemblies sind viele Methoden nur **Stubs/Brücken**. Der echte Code liegt im nativen IL2CPP-Binary. Daher:

- Bodies wirken leer oder minimal.
- Signaturen sind trotzdem nützlich für Hooking/Binding.
- Für verlässliche Analyse: Runtime-Verhalten + dekompilierte Metadaten kombinieren.

## API-Orienteirung (FrikaMF intern)

- `FrikaMF/JoniMF/HarmonyPatches.cs`: Hook-Einstieg
- `FrikaMF/JoniMF/EventIds.cs`: stabile Event-IDs
- `FrikaMF/JoniMF/EventDispatcher.cs`: Marshalling Richtung Rust
- `FrikaMF/JoniMF/GameApi.cs`: API-Tabelle Richtung Rust
- `FrikaMF/JoniMF/GameHooks.cs`: sichere Wrapper für Spielsicht

## Web UI im Framework (DC2WEB)

- Kern: `FrikaMF/JoniMF/DC2WebBridge.cs`
- Menü-Integration: `FrikaMF/JoniMF/ModSettingsMenuBridge.cs`
- Settings-Hook: `MainMenu.Settings` → Auswahl `Game Settings` / `Mod Settings`

Unterstützte Quellen:

- Basic: `HTML`, `CSS`
- Styling-Frameworks: `TailwindCSS`, `SASS`/`SCSS`
- Script-Styles: `JS`, `TS`
- React-orientierte Quellen: `React JSX/TSX` (Adapter-Übersetzung)

Unterstützte Bildtypen:

- SVG (bevorzugt, zur Laufzeit rasterisiert)
- PNG, JPG/JPEG, BMP, GIF, TGA

Details: [`Web UI Bridge (DC2WEB)`](Web-UI-Bridge)

## Praxisregel: Prefix oder Postfix?

- **Prefix**: wenn du Verhalten blockieren/überschreiben musst (`return false`).
- **Postfix**: wenn du nur beobachten/ergänzen willst.
- Bei riskanten State-Methoden immer erst mit Postfix anfangen.

## Cross-Track Beispiel (gleiches Ziel, beide Sprachen)

### 🦀 Rust (Event-getrieben)

```rust
#[no_mangle]
pub extern "C" fn mod_on_event(event_id: u32, _data_ptr: *const u8, _data_len: u32) {
    if event_id == 1001 {
        // handle event in Rust
    }
}
```

### 🔷 C# (Patch-getrieben)

```csharp
[HarmonyPatch(typeof(CustomerBase), nameof(CustomerBase.AreAllAppRequirementsMet))]
public static class Patch_Customer_Requirements
{
    public static void Postfix(bool __result)
    {
        MelonLogger.Msg($"Requirements met: {__result}");
    }
}
```
