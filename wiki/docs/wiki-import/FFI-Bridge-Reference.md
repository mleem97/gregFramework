# FFI Bridge Reference

Last updated: 2026-04-03

This page documents the currently implemented FFI surface between the C# framework runtime and native Rust DLL mods.

## Source of truth

- `FrikaMF/FfiBridge.cs`
- `FrikaMF/GameApi.cs`
- `FrikaMF/EventDispatcher.cs`
- `FrikaMF/EventIds.cs`
- `FrikaMF/Core.cs`

## Language/runtime support status

- Native Rust DLL integration: supported and implemented.
- Embedded Lua runtime in framework core: not implemented.
- Embedded Python runtime in framework core: not implemented.
- Embedded HTTP/WebSocket transport layer for generic FFI: not implemented.

Recommended integration model for Lua/Python today:

- Run language runtime out-of-process (sidecar).
- Use C# or Rust mod component as boundary adapter.
- Convert incoming framework events to sidecar messages.
- Apply sidecar decisions through `GameAPITable` and existing safe wrapper calls.

Detailed bilingual Lua onboarding tutorials:

- [Lua FFI Start Developing (DE+EN)](/wiki/wiki-import/Lua-FFI-Start-Developing)

## Callability quick answer

- Can you call everything? **No**.
- Most v1-v6 game API functions are callable.
- Lua is currently sidecar-based (no embedded in-process Lua host in core).
- Known limited/stubbed functions remain in v3/v7 (`GetNetWatchStats`, lobby APIs, `SteamPollEvent`).

## Runtime loading model

- Native mods are discovered under `Data Center/Mods/RustMods/**/*.dll`.
- Each DLL is loaded dynamically with `LoadLibrary`.
- Exports are resolved dynamically with `GetProcAddress`.
- On shutdown, libraries are released with `FreeLibrary`.
- Missing optional exports do not prevent loading.

## Rust exports currently consumed

Required-at-runtime behavior is best-effort, but these exports are actively probed by name:

- `mod_info` (metadata)
- `mod_init` (initialization with API table pointer)
- `mod_update` (optional)
- `mod_fixed_update` (optional)
- `mod_on_scene_loaded` (optional)
- `mod_shutdown` (optional)
- `mod_on_event` (optional)

Calling convention for native delegates is `Cdecl`.

## Lifecycle wiring

The MelonLoader entrypoint calls the bridge hooks in this order:

- `OnInitializeMelon`: create bridge, initialize dispatcher, load mods.
- `OnUpdate`: call `mod_update(deltaTime)` for loaded mods.
- `OnFixedUpdate`: call `mod_fixed_update(deltaTime)` for loaded mods.
- `OnSceneWasLoaded`: call `mod_on_scene_loaded(sceneName)`.
- `OnApplicationQuit`: call `mod_shutdown()` and unload native modules.

## Game API table (`GameAPITable`)

Current exposed API version is `7` (`GameAPIManager.API_VERSION = 7`).

### v1 core functions

- Logging: `LogInfo`, `LogWarning`, `LogError`
- Economy/time: `GetPlayerMoney`, `SetPlayerMoney`, `GetTimeScale`, `SetTimeScale`
- Counts/context: `GetServerCount`, `GetRackCount`, `GetCurrentScene`

### v2 progression and world metrics

- XP/Reputation: `GetPlayerXP`, `SetPlayerXP`, `GetPlayerReputation`, `SetPlayerReputation`
- Time/day: `GetTimeOfDay`, `GetDay`, `GetSecondsInFullDay`, `SetSecondsInFullDay`
- Additional counts: `GetSwitchCount`, `GetSatisfiedCustomerCount`

### v3 net-watch controls

- `SetNetWatchEnabled`, `IsNetWatchEnabled`, `GetNetWatchStats`

Current implementation note: net-watch state is local bridge state; `GetNetWatchStats` currently returns `0`.

### v4 repair and technician primitives

- Fault/EOL counters: `GetBrokenServerCount`, `GetBrokenSwitchCount`, `GetEolServerCount`, `GetEolSwitchCount`
- Technician counters: `GetFreeTechnicianCount`, `GetTotalTechnicianCount`
- Dispatch actions: `DispatchRepairServer`, `DispatchRepairSwitch`, `DispatchReplaceServer`, `DispatchReplaceSwitch`

### v5 custom employee surface

- `RegisterCustomEmployee`
- `IsCustomEmployeeHired`
- `FireCustomEmployee`
- `RegisterSalary`

### v6 notifications and simulation state

- UI + rates: `ShowNotification`, `GetMoneyPerSecond`, `GetExpensesPerSecond`, `GetXpPerSecond`
- Simulation control: `IsGamePaused`, `SetGamePaused`, `GetDifficulty`, `TriggerSave`

### v7 Steam/P2P + position

- Steam identity/friends: `SteamGetMyId`, `SteamGetFriendName`
- Lobby API surface: `SteamCreateLobby`, `SteamJoinLobby`, `SteamLeaveLobby`, `SteamGetLobbyId`, `SteamGetLobbyOwner`, `SteamGetLobbyMemberCount`, `SteamGetLobbyMemberByIndex`, `SteamSetLobbyData`, `SteamGetLobbyData`
- P2P networking: `SteamSendP2P`, `SteamIsP2PAvailable`, `SteamReadP2P`, `SteamAcceptP2P`
- Event polling and player transform: `SteamPollEvent`, `GetPlayerPosition`

Implementation detail:

- P2P functions are implemented via `steam_api64` (`ISteamNetworking` v006 wrappers).
- Lobby-related functions are currently stubs returning default values.
- `SteamPollEvent` is currently stubbed (`0`, no queue implementation).

## Event bridge surface

`EventDispatcher` marshals structs into unmanaged memory and forwards data through `FFIBridge.DispatchEvent(eventId, ptr, size)`.

### Event IDs in active contracts

- Economy/player: `100` money changed, `101` XP changed, `102` reputation changed
- Server/network: `200` powered, `201` broken, `202` repaired, `203` installed, `204` cable connected, `205` cable disconnected, `206` customer changed, `207` app changed, `208` rack unmounted, `209` switch broken, `210` switch repaired
- Cable extended: `211` cable created, `212` cable removed, `213` cable cleared, `214` cable speed changed, `215` cable SFP inserted, `216` cable SFP removed
- Time/progression: `300` day ended, `301` month ended
- Customer flow: `400` accepted, `401` satisfied, `402` unsatisfied
- Shop flow: `500` checkout, `501` item added, `502` cart cleared, `503` item removed
- HR/save/wall: `600` employee hired, `601` employee fired, `700` game saved, `701` game loaded, `702` autosaved, `800` wall purchased
- Mod systems: `900` net-watch dispatched, `1000` custom employee hired, `1001` custom employee fired
- Hook automation: `1100` hook bridge installed, `1101` hook bridge triggered

### Payload struct types currently marshaled

- `ValueChangedData`
- `ServerPoweredData`
- `DayEndedData`
- `MonthEndedData`
- `CustomerAcceptedData`
- `CustomerSatisfiedData`
- `ServerCustomerChangedData`
- `ServerAppChangedData`
- `ShopItemAddedData`
- `ShopItemRemovedData`
- `NetWatchDispatchedData`
- `CustomEmployeeEventData` (fixed 64-byte ID buffer)

## Stability and safety behavior

- Mod callback failures are isolated per mod and logged; bridge continues processing other mods.
- Event dispatch uses a short duplicate-suppression window (~50ms) with basic payload hashing.
- Strings passed to Rust callback entrypoints are ANSI (`StringToHGlobalAnsi` / `PtrToStringAnsi`).
- `mod_init` bool return is marshaled as `U1`.

## Current limitations and gaps

- No explicit ABI handshake export is enforced (for example `mod_abi_version`).
- Event ABI validation is contract-based only; no runtime schema negotiation.
- Steam lobby/event queue functions are exposed in the table but mostly unimplemented stubs.
- Event dedup can suppress repeated same-id/same-hash events in the debounce window.

## Minimal native integration checklist

1. Export at least `mod_info` and `mod_init`.
2. Accept and parse `GameAPITable` according to `ApiVersion`.
3. Implement `mod_on_event` to consume normalized gameplay events.
4. Optionally implement frame and lifecycle callbacks (`mod_update`, `mod_fixed_update`, `mod_on_scene_loaded`, `mod_shutdown`).

## Related links

- [Framework Features & Use Cases](/wiki/wiki-import/Framework-Features-Use-Cases)
- [Mod Config System](/wiki/wiki-import/Mod-Config-System)
- [Modding Guide](Modding-Guide)
- [Lua FFI Start Developing (DE+EN)](/wiki/wiki-import/Lua-FFI-Start-Developing)
- [Architecture](/wiki/wiki-import/Architecture)
- [Web UI Bridge (DC2WEB)](/wiki/wiki-import/Web-UI-Bridge)
- [Web UI Bridge (DC2WEB) EN](Web-UI-Bridge-en)
