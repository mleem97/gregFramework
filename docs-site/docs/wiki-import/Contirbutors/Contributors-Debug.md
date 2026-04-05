---
title: Contributors (Debug)
description: "Beitrag zum Framework selbst: Setup, Hook-Workflow, Konventionen, PR-Checks und CI-Verhalten."
sidebar_position: 40
tags:
  - audience:contributor
---

## Contributors (Debug)

Diese Seite ist für Entwickler, die direkt am `FrikaModdingFramework` arbeiten.

Für die vollständige, laufend zu pflegende Feature-Matrix mit Implementierungs-Use-Cases siehe [`Framework Features & Use Cases`](Framework-Features-Use-Cases).

## Dev-Setup

### Voraussetzungen

- Windows mit installiertem `Data Center`
- .NET SDK 6+
- Rust Toolchain (für FFI-/Bridge-Arbeiten)
- MelonLoader installiert und mindestens ein Spielstart

### Build-Kommandos

```powershell
dotnet build .\FrikaMF.csproj -c Debug -nologo
cargo build --release
```

Mit explizitem Spielpfad:

```powershell
dotnet build .\FrikaMF.csproj /p:GameDir="C:\Pfad\zu\Data Center"
```

## Projektstruktur (kurz erklärt)

- `FrikaMF/`: Runtime Bridge, Hooks, Dispatcher, API-Tabelle
- `FrikaMF/ModigAPIs/`: höhere API-Fassaden
- `Events/`: Event-Contracts
- `.wiki/`: editierbare Wiki-Quelle
- `scripts/`: Build-/Release-/Sync-Automation

## Workflow: Neuen Hook hinzufügen (Schritt für Schritt)

1. **dnSpy/dotPeek:** Zielmethode finden und Signatur prüfen.
2. **Dokumentation:** Eintrag in [`HOOKS.md`](HOOKS) hinzufügen/aktualisieren.
3. **Patch:** Harmony Patch in `FrikaMF/HarmonyPatches.cs` ergänzen.
4. **Bridge:** Event-ID in `EventIds.cs` und Dispatch in `EventDispatcher.cs` ergänzen.
5. **Rust-Vertrag:** Falls benötigt, C-ABI-Struktur + `mod_on_event` Vertrag ergänzen.
6. **Test:** Build + Laufzeitprüfung.
7. **PR:** Kleine, atomare Commits + nachvollziehbare Beschreibung.

## Konventionen

### Namensgebung

- Hooks: `Patch_<Klasse>_<Methode>`
- Event IDs: sprechende, stabile Namen in `EventIds.cs`
- Rust Exports: `mod_info`, `mod_init`, `mod_on_event`, etc.

### Wrapper vs. Mod-Logik

- **Wrapper/Bridge (Framework):** Stabilität, Marshalling, Sicherheitschecks
- **Mod (Feature-Logik):** Gameplay-Verhalten, Policies, UI

### Blittable-Typen-Regel

Für C-ABI Datenstrukturen nur blittable Felder verwenden (z. B. `int`, `float`, fixed-size buffers, Pointer). Keine managed Referenztypen in ABI-Structs.

## IL2CPP-Fallstricke

- `b###`-Suffixe: compiler-generierte Member, oft instabil zwischen Spielversionen.
- Coroutine-Compiler-Typen (`d##`/Iterator-State): ebenfalls instabil.
- Prefix bei mutierenden Methoden kann Seiteneffekte blockieren; Postfix oft sicherer.

## DC2WEB Contributor-Hinweise

Neue Komponenten:

- `FrikaMF/DC2WebBridge.cs`
- `FrikaMF/ModSettingsMenuBridge.cs`

Hook-Fluss:

- `MainMenu.Settings` öffnet Settings-Auswahl (Game vs Mod)
- `MainMenu.Start`, `HRSystem.OnEnable`, `ComputerShop.InteractOnClick` triggern Web-/UI-Anwendung

Erweiterungspunkte:

- `IDc2WebFrameworkAdapter` für weitere Frameworks
- `Dc2WebAppDescriptor` für komplexere App-Bundles
- `Dc2WebImageAsset` für Asset-Pipelines (SVG-first)

Aktuelle technische Grenze:

- Kein eingebetteter Browser/DOM/JS-Engine-Laufzeitstack.
- React/TS/JS laufen über Übersetzungsadapter auf Unity-UI-Profile.

Details: [`Web UI Bridge (DC2WEB)`](Web-UI-Bridge)

## Lua/Python/Web FFI Contributor Notes

Current core status:

- Rust native FFI bridge: implemented.
- Built-in Lua runtime host: not implemented.
- Built-in Python runtime host: not implemented.
- Built-in generic HTTP/WebSocket FFI transport: not implemented.

Contribution guidance:

- Treat Lua/Python support as sidecar integration work, not as existing core runtime features.
- Keep Unity and IL2CPP access in C# or Rust boundaries; do not expose raw Unity objects over external transport.
- If adding web transport, define strict command schemas, authentication for non-local access, and rate limits.
- Document every added transport/ABI contract in `Framework-Features-Use-Cases` and `FFI-Bridge-Reference`.

## CI-Pipeline (warum Builds in CI anders sind)

- CI läuft ohne lokale Spielinstallation.
- In `FrikaMF.csproj` wird `$(CI)=true` verwendet, um lokale Referenzvalidierung zu überspringen.
- Lokal gilt: ohne erzeugte MelonLoader-/Interop-Dateien scheitert der Build absichtlich mit klarer Fehlermeldung.

## PR-Checkliste

- [ ] Hook in `HOOKS.md` dokumentiert (inkl. Verifikationsstatus)
- [ ] Event-ID/Dispatch konsistent ergänzt
- [ ] Build lokal erfolgreich (`Debug` mindestens)
- [ ] Wiki/Docs bei API-Änderung aktualisiert
- [ ] Keine irrelevanten Format-/Refactor-Änderungen
- [ ] Commit Messages als Conventional Commits

## Commit-Konvention (Conventional Commits)

Beispiele:

- `feat(hooks): add CustomerBase performance event`
- `fix(ffi): guard null ptr in mod_on_event`
- `docs(wiki): document IL2CPP hook pitfalls`

## Contributor-Referenzbeispiel (beide Sprachen)

### 🦀 Rust

```rust
#[repr(C)]
pub struct MoneyChanged {
    pub old_value: i32,
    pub new_value: i32,
}

#[no_mangle]
pub extern "C" fn mod_on_event(event_id: u32, data_ptr: *const u8, data_len: u32) {
    if event_id == 1 && data_len as usize == core::mem::size_of::<MoneyChanged>() {
        let payload = unsafe { &*(data_ptr as *const MoneyChanged) };
        let _delta = payload.new_value - payload.old_value;
    }
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

public static void FireMoneyChanged(int oldValue, int newValue)
{
    var payload = new MoneyChanged { OldValue = oldValue, NewValue = newValue };
    EventDispatcher.Dispatch(EventIds.MoneyChanged, payload);
}
```
