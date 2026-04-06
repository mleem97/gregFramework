<!-- markdownlint-disable MD024 MD032 -->

# Lua FFI — How to Start Developing (DE + EN)

Last updated: 2026-04-04

This page is a practical, bilingual onboarding for Lua-based integrations on top of FrikaMF.

> Tabbed-style note: GitHub Wiki has no native tab component. This page uses markdown "Tab: DE/EN" sections as a tab-like fallback.

## Current status (code-grounded)

- Native FFI host in framework core: **implemented** (`FrikaMF/FfiBridge.cs`).
- Embedded Lua host in framework core: **not implemented**.
- Embedded Python host in framework core: **not implemented**.
- Generic HTTP/WebSocket FFI transport in core: **not implemented**.

## Can we call everything?

Short answer: **No, not everything**.

- You can call all **implemented** `GameAPITable` entries via a native boundary mod.
- Lua cannot call the framework directly in-process today; use a **sidecar** pattern.
- Some API entries are intentionally exposed but currently stubbed/partial:
  - `GetNetWatchStats` currently returns `0`.
  - Steam lobby functions (`SteamCreateLobby`, `SteamJoinLobby`, `SteamLeaveLobby`, `SteamGetLobby*`, `SteamSetLobbyData`, `SteamGetLobbyData`) are currently stubs.
  - `SteamPollEvent` is currently stubbed (`0`).

## Recommended architecture for Lua today

```text
Data Center (IL2CPP)
  -> FrikaMF FFI bridge (C#)
  -> Native adapter mod (Rust or C#)
  -> IPC (named pipe / TCP localhost / UDP localhost)
  -> Lua runtime (LuaJIT/Lua 5.x sidecar)
```

## FFI entrypoint tutorials (each export)

### 1) `mod_info` — plugin metadata

### Tab: 🇩🇪 Deutsch

**Ziel:** Mod-Metadaten liefern, damit FrikaMF die DLL korrekt identifiziert.

**Start-Schritte:**
1. Export `mod_info` in deinem nativen Adapter bereitstellen.
2. Statische null-terminierte Strings zurückgeben.
3. Keine Heap-Freigabe auf C#-Seite erwarten.

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
        id: b"lua_adapter\0".as_ptr() as *const i8,
        name: b"Lua Adapter\0".as_ptr() as *const i8,
        version: b"0.1.0\0".as_ptr() as *const i8,
        author: b"you\0".as_ptr() as *const i8,
        description: b"Routes FrikaMF events to Lua sidecar\0".as_ptr() as *const i8,
    }
}
```

### Tab: 🇬🇧 English

**Goal:** Provide plugin metadata so FrikaMF can identify your native module.

**Start steps:**
1. Export `mod_info` from your native adapter.
2. Return static null-terminated strings.
3. Do not expect C# to free returned string memory.

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
        id: b"lua_adapter\0".as_ptr() as *const i8,
        name: b"Lua Adapter\0".as_ptr() as *const i8,
        version: b"0.1.0\0".as_ptr() as *const i8,
        author: b"you\0".as_ptr() as *const i8,
        description: b"Routes FrikaMF events to Lua sidecar\0".as_ptr() as *const i8,
    }
}
```

### 2) `mod_init(api_table)` — bootstrap and API binding

### Tab: 🇩🇪 Deutsch

**Ziel:** `GameAPITable` binden und Lua-Sidecar starten/verbinden.

```rust
#[repr(C)]
pub struct GameApiTableV7 {
    pub api_version: u32,
    pub log_info: extern "C" fn(*const i8),
    pub log_warning: extern "C" fn(*const i8),
    pub log_error: extern "C" fn(*const i8),
    pub get_player_money: extern "C" fn() -> f64,
}

static mut API: Option<&'static GameApiTableV7> = None;

#[no_mangle]
pub extern "C" fn mod_init(api_table: *mut core::ffi::c_void) -> bool {
    if api_table.is_null() {
        return false;
    }

    unsafe {
        let api = &*(api_table as *const GameApiTableV7);
        if api.api_version < 7 {
            return false;
        }
        API = Some(api);
    }

    // Hier Sidecar starten oder Verbindung aufbauen
    true
}
```

### Tab: 🇬🇧 English

**Goal:** Bind `GameAPITable` and start/connect your Lua sidecar.

```rust
#[repr(C)]
pub struct GameApiTableV7 {
    pub api_version: u32,
    pub log_info: extern "C" fn(*const i8),
    pub log_warning: extern "C" fn(*const i8),
    pub log_error: extern "C" fn(*const i8),
    pub get_player_money: extern "C" fn() -> f64,
}

static mut API: Option<&'static GameApiTableV7> = None;

#[no_mangle]
pub extern "C" fn mod_init(api_table: *mut core::ffi::c_void) -> bool {
    if api_table.is_null() {
        return false;
    }

    unsafe {
        let api = &*(api_table as *const GameApiTableV7);
        if api.api_version < 7 {
            return false;
        }
        API = Some(api);
    }

    // Start sidecar or connect IPC channel here
    true
}
```

### 3) `mod_update(delta_time)` — per-frame tick

### Tab: 🇩🇪 Deutsch

**Ziel:** Nicht-blockierende Tick-Logik und Sidecar-Polling.

```rust
#[no_mangle]
pub extern "C" fn mod_update(delta_time: f32) {
    let _ = delta_time;
    // Non-blocking: eingehende Lua-Befehle lesen und anwenden
}
```

### Tab: 🇬🇧 English

**Goal:** Non-blocking tick loop and sidecar polling.

```rust
#[no_mangle]
pub extern "C" fn mod_update(delta_time: f32) {
    let _ = delta_time;
    // Non-blocking: poll Lua commands and apply them
}
```

### 4) `mod_fixed_update(delta_time)` — fixed-step logic

### Tab: 🇩🇪 Deutsch

**Ziel:** Deterministische/physiknahe Ticks separat halten.

```rust
#[no_mangle]
pub extern "C" fn mod_fixed_update(delta_time: f32) {
    let _ = delta_time;
    // Optional: feste Simulationslogik
}
```

### Tab: 🇬🇧 English

**Goal:** Keep deterministic/fixed-step logic separate.

```rust
#[no_mangle]
pub extern "C" fn mod_fixed_update(delta_time: f32) {
    let _ = delta_time;
    // Optional: fixed-step simulation logic
}
```

### 5) `mod_on_scene_loaded(scene_name)` — scene lifecycle

### Tab: 🇩🇪 Deutsch

```rust
use std::ffi::CStr;

#[no_mangle]
pub extern "C" fn mod_on_scene_loaded(scene_name: *const i8) {
    if scene_name.is_null() {
        return;
    }
    let name = unsafe { CStr::from_ptr(scene_name) }.to_string_lossy();
    // Sidecar informieren: Szene gewechselt
    println!("Scene loaded: {}", name);
}
```

### Tab: 🇬🇧 English

```rust
use std::ffi::CStr;

#[no_mangle]
pub extern "C" fn mod_on_scene_loaded(scene_name: *const i8) {
    if scene_name.is_null() {
        return;
    }
    let name = unsafe { CStr::from_ptr(scene_name) }.to_string_lossy();
    // Notify sidecar: scene changed
    println!("Scene loaded: {}", name);
}
```

### 6) `mod_on_event(event_id, data_ptr, data_len)` — event ingestion

### Tab: 🇩🇪 Deutsch

```rust
#[no_mangle]
pub extern "C" fn mod_on_event(event_id: u32, data_ptr: *const u8, data_len: u32) {
    match event_id {
        200 => {
            // ServerPowered
            let _ = (data_ptr, data_len);
        }
        211 => {
            // CableCreated
        }
        214 => {
            // CableSpeedChanged
        }
        _ => {}
    }
}
```

### Tab: 🇬🇧 English

```rust
#[no_mangle]
pub extern "C" fn mod_on_event(event_id: u32, data_ptr: *const u8, data_len: u32) {
    match event_id {
        200 => {
            // ServerPowered
            let _ = (data_ptr, data_len);
        }
        211 => {
            // CableCreated
        }
        214 => {
            // CableSpeedChanged
        }
        _ => {}
    }
}
```

### 7) `mod_shutdown()` — cleanup

### Tab: 🇩🇪 Deutsch

```rust
#[no_mangle]
pub extern "C" fn mod_shutdown() {
    // Sidecar-Verbindung sauber schließen
}
```

### Tab: 🇬🇧 English

```rust
#[no_mangle]
pub extern "C" fn mod_shutdown() {
    // Close sidecar connection cleanly
}
```

## Lua sidecar starter snippets

### Lua process (receive events, send commands)

### Tab: 🇩🇪 Deutsch

```lua
-- lua_sidecar.lua
local socket = require("socket")
local client = assert(socket.tcp())
assert(client:connect("127.0.0.1", 34567))
client:settimeout(0)

while true do
  local line = client:receive("*l")
  if line then
    -- Beispiel: Event auswerten
    -- print("event", line)
  end

  -- Beispiel-Kommando zurück an Adapter
  -- client:send('{"cmd":"set_time_scale","value":1.0}\n')

  socket.sleep(0.01)
end
```

### Tab: 🇬🇧 English

```lua
-- lua_sidecar.lua
local socket = require("socket")
local client = assert(socket.tcp())
assert(client:connect("127.0.0.1", 34567))
client:settimeout(0)

while true do
  local line = client:receive("*l")
  if line then
    -- Example: handle incoming event
    -- print("event", line)
  end

  -- Example command back to adapter
  -- client:send('{"cmd":"set_time_scale","value":1.0}\n')

  socket.sleep(0.01)
end
```

## Callable API matrix (v7)

### Fully implemented and callable (runtime-state dependent)

- v1: logging, player money, time scale, server/rack count, current scene
- v2: XP/reputation, day/time metrics, switch/customer counters
- v4: broken/eol counters, technician counters, repair/replace dispatch
- v5: custom employee registration and HR actions
- v6: notifications, rates, pause, difficulty, trigger save
- v7 (partial): `SteamGetMyId`, `SteamGetFriendName`, `SteamSendP2P`, `SteamIsP2PAvailable`, `SteamReadP2P`, `SteamAcceptP2P`, `GetPlayerPosition`

### Exposed but currently stubbed/limited

- `GetNetWatchStats` (currently constant `0`)
- `SteamCreateLobby`
- `SteamJoinLobby`
- `SteamLeaveLobby`
- `SteamGetLobbyId`
- `SteamGetLobbyOwner`
- `SteamGetLobbyMemberCount`
- `SteamGetLobbyMemberByIndex`
- `SteamSetLobbyData`
- `SteamGetLobbyData`
- `SteamPollEvent`

## Per-function quickstart (all exported `GameAPITable` symbols)

> Format: `Status` + concise DE/EN start hint.

| Symbol | Status | How to start (DE / EN) |
| --- | --- | --- |
| `LogInfo` | implemented | Adapter-call mit C-String / call with C string |
| `LogWarning` | implemented | Warnungen aus Lua weiterreichen / forward warnings from Lua |
| `LogError` | implemented | Fehler aus Lua weiterreichen / forward Lua errors |
| `GetPlayerMoney` | implemented | Read-only Poll im Tick / read-only polling in tick |
| `SetPlayerMoney` | implemented | Nur host-authoritativ nutzen / host-authoritative only |
| `GetTimeScale` | implemented | Vor Regelentscheidungen lesen / read before rule decisions |
| `SetTimeScale` | implemented | Nur validierte Kommandos / validated commands only |
| `GetServerCount` | implemented | KPI Snapshot / KPI snapshot |
| `GetRackCount` | implemented | KPI Snapshot / KPI snapshot |
| `GetCurrentScene` | implemented | Szenenwechsel im Lua-Flow / route scene changes to Lua |
| `GetPlayerXP` | implemented | Progress Polling / progress polling |
| `SetPlayerXP` | implemented | Nur über sichere Policies / safe policy gating |
| `GetPlayerReputation` | implemented | Reputation als Inputsignal / use as input signal |
| `SetPlayerReputation` | implemented | Nur hostseitig mutieren / mutate host-side only |
| `GetTimeOfDay` | implemented | Tageslogik triggern / drive day-phase logic |
| `GetDay` | implemented | Daily-rotation rules / daily rotation rules |
| `GetSecondsInFullDay` | implemented | Balance-Kalibrierung / pacing calibration |
| `SetSecondsInFullDay` | implemented | Nur in kontrollierten Modi / controlled modes only |
| `GetSwitchCount` | implemented | Netzwerk-Metrik / network metric |
| `GetSatisfiedCustomerCount` | implemented | SLA-/Kundenziele bewerten / evaluate SLA goals |
| `SetNetWatchEnabled` | implemented | Feature-Flag aus Lua setzen / toggle feature flag |
| `IsNetWatchEnabled` | implemented | Flag-Readback / flag readback |
| `GetNetWatchStats` | limited | Aktuell Platzhalter (`0`) / placeholder (`0`) |
| `GetBrokenServerCount` | implemented | Repair-Queue Priorisierung / prioritize repair queue |
| `GetBrokenSwitchCount` | implemented | Repair-Queue Priorisierung / prioritize repair queue |
| `GetEolServerCount` | implemented | EOL-Planung / EOL planning |
| `GetEolSwitchCount` | implemented | EOL-Planung / EOL planning |
| `GetFreeTechnicianCount` | implemented | Dispatch-Checks / dispatch checks |
| `GetTotalTechnicianCount` | implemented | Kapazitätsplanung / capacity planning |
| `DispatchRepairServer` | implemented | Command aus Lua-Policy / Lua policy command |
| `DispatchRepairSwitch` | implemented | Command aus Lua-Policy / Lua policy command |
| `DispatchReplaceServer` | implemented | Nur bei bestätigten Regeln / confirmed policy only |
| `DispatchReplaceSwitch` | implemented | Nur bei bestätigten Regeln / confirmed policy only |
| `RegisterCustomEmployee` | implemented | Bei Init registrieren / register at init |
| `IsCustomEmployeeHired` | implemented | Status-Polling / status polling |
| `FireCustomEmployee` | implemented | Admin-/Policy-Aktion / admin/policy action |
| `RegisterSalary` | implemented | Lohnmodelle im Adapter / salary model in adapter |
| `ShowNotification` | implemented | User-Feedback aus Lua-Entscheidung / user feedback from Lua decision |
| `GetMoneyPerSecond` | implemented | Wirtschaftsmetriken / economy metric |
| `GetExpensesPerSecond` | implemented | Wirtschaftsmetriken / economy metric |
| `GetXpPerSecond` | implemented | Progressionstempo / progression pace |
| `IsGamePaused` | implemented | Guard vor kritischen Writes / guard before critical writes |
| `SetGamePaused` | implemented | Nur hostseitig / host-side only |
| `GetDifficulty` | implemented | Policy-Branching / policy branching |
| `TriggerSave` | implemented | Checkpoint triggern / trigger checkpoint |
| `SteamGetMyId` | implemented | Steam identity read / Steam identity read |
| `SteamGetFriendName` | implemented | Name lookup / name lookup |
| `SteamCreateLobby` | stubbed | Noch nicht produktiv / not production-ready yet |
| `SteamJoinLobby` | stubbed | Noch nicht produktiv / not production-ready yet |
| `SteamLeaveLobby` | stubbed | Noch nicht produktiv / not production-ready yet |
| `SteamGetLobbyId` | stubbed | Noch nicht produktiv / not production-ready yet |
| `SteamGetLobbyOwner` | stubbed | Noch nicht produktiv / not production-ready yet |
| `SteamGetLobbyMemberCount` | stubbed | Noch nicht produktiv / not production-ready yet |
| `SteamGetLobbyMemberByIndex` | stubbed | Noch nicht produktiv / not production-ready yet |
| `SteamSetLobbyData` | stubbed | Noch nicht produktiv / not production-ready yet |
| `SteamGetLobbyData` | stubbed | Noch nicht produktiv / not production-ready yet |
| `SteamSendP2P` | implemented | Packet send via adapter / packet send via adapter |
| `SteamIsP2PAvailable` | implemented | Poll inbound packets / poll inbound packets |
| `SteamReadP2P` | implemented | Read payload buffer / read payload buffer |
| `SteamAcceptP2P` | implemented | Session acceptance call / session acceptance call |
| `SteamPollEvent` | stubbed | Event queue noch offen / event queue pending |
| `GetPlayerPosition` | implemented | Positional telemetry / positional telemetry |

## Practical answer to “Kann man alles callen?”

- **Nein**, nicht vollständig.
- **Ja**, der Großteil der Game- und Event-FFI ist callbar.
- **Lua direkt im Framework:** aktuell nicht — nur über Adapter + Sidecar.

## Related links

- [FFI Bridge Reference](/wiki/wiki-import/FFI-Bridge-Reference)
- [Modding Guide](Modding-Guide)
- [Mod-Developer (Debug)](Mod-Developer-Debug)
- [Framework Features & Use Cases](/wiki/wiki-import/Framework-Features-Use-Cases)
