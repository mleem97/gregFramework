# Copilot Instructions

## GitHub Copilot — FrikaModdingFramework

### Projekt

FrikaModdingFramework (FrikaMF) ist ein inoffizielles Modding-Framework für
"Data Center" (WASEKU, Unity IL2CPP). Unterstützt Mods in **C#** (MelonLoader/HarmonyX)
und **Rust** (C-ABI FFI via P/Invoke).

**Inoffiziell · Community-driven · Keine Zugehörigkeit zu WASEKU**

### Verfügbare Wiki-Agenten

| Command | Datei | Zweck |
|---|---|---|
| `/wiki-init` | `.github/prompts/wiki-init.prompt.md` | Wiki neu anlegen (State + Queue) |
| `/wiki-orchestrator` | `.github/prompts/wiki-orchestrator.prompt.md` | Fortschritt überwachen, Locks aufräumen |
| `/wiki-worker` | `.github/prompts/wiki-worker.prompt.md` | Seite generieren (parallel starten!) |
| `/wiki-reviewer` | `.github/prompts/wiki-reviewer.prompt.md` | Seiten reviewen |
| `/wiki-update` | `.github/prompts/wiki-update.prompt.md` | Sektion re-queuen |

### Schnellstart Wiki

```
1. /wiki-init                          → Einmalig: State & Queue anlegen
2. /wiki-worker  (Panel 1)             ┐
3. /wiki-worker  (Panel 2)             │ Parallel starten für Geschwindigkeit
4. /wiki-worker  (Panel 3)             ┘
5. /wiki-reviewer (eigenes Panel)      → Prüft fertige Seiten
6. /wiki-orchestrator                  → Fortschritt & Cleanup
```

### Technischer Kontext

- Spielklassen: `Il2Cpp.*` (z.B. `Il2Cpp.Server`, `Il2Cpp.NetworkSwitch`)
- Build: `dotnet build /p:GameDir="..."` oder env `DATA_CENTER_GAME_DIR`
- CI: `$(CI)=true` → keine Spielinstallation nötig
- Commits: Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`)
- Wiki-Output: `docs/wiki/`
- Wiki-State: `.wiki-state/`

### Allgemeine Regeln

- Wiki-Seiten: Deutsch (Code-Kommentare Englisch)
- Code-Beispiele IMMER in beiden Sprachen: 🦀 Rust + 🔷 C#
- Kein "TODO", kein "Details folgen", keine Platzhalter
- YAML-Frontmatter auf jeder Seite (title, description, sidebar_position, tags)

## General Guidelines
- Use English language for all Markdown documentation files that are not excluded by .gitignore.
- Always use Conventional Commits with atomic commit messages (e.g., feat:, fix:, docs:, chore:) for this repository.
- Use `FrikaModFramework` in repository URLs and metadata.
- Use `https://github.com/mleem97/FrikaModFramework` for repository links.
- Use `https://github.com/mleem97/FrikaModFramework.wiki.git` as the wiki remote target.

## Release and Versioning Rules (Mandatory)
- Use release version format `XX.XX.XXXX` for framework releases.
- Treat `FrikaMF/ReleaseVersion.cs` as the single source of truth for framework release version.
- For automated release prep, use:
	- `pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump major`
	- `pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump medium`
	- `pwsh -ExecutionPolicy Bypass -File .\scripts\Update-ReleaseMetadata.ps1 -Bump minor`
- Every release version update must prepend a new `CHANGELOG.md` entry automatically via `scripts/Update-ReleaseMetadata.ps1`.
- Local release upload must publish the framework artifact as `FrikaModdingFramework-v{XX.XX.XXXX}.dll`.
- Prefer `pwsh -ExecutionPolicy Bypass -File .\scripts\Publish-LocalRelease.ps1` for local release upload flow.

## Wiki Sync Rules (Mandatory)
- Keep editable wiki source files under `.wiki/` in this repository.
- Sync `.wiki/` to the GitHub wiki repo with `pwsh -ExecutionPolicy Bypass -File .\scripts\Sync-Wiki.ps1`.
- Use `scripts/Sync-Wiki.ps1` for wiki synchronization and do not manually copy wiki files.

## Project-Specific Rules
- Focus on game events and hook candidates within `Assembly-CSharp`, as this is the relevant game assembly.
- Keep runtime bridge code under `FrikaMF`.
- Keep Rust native mods in `Data Center/Mods/RustMods`.
- Keep game object/content packs in `Data Center_Data/StreamingAssets/Mods`.

## Hook Naming Convention (Mandatory)
- Canonical hook format is `FFM.[Category].[Entity].[Action]`.
- Always use 4 segments: `FFM`, `Category`, `Entity`, `Action`.
- `Action` must start with `On` for event hooks.
- Use PascalCase for all segments and avoid abbreviations.
- Suppressible hooks must expose a `.Suppress` variant (e.g. `FFM.Store.Cart.OnCheckedOut.Suppress`).
- When mapping numeric event IDs to names, use `FrikaMF/HookNames.cs` as the canonical source.

## Mac Compatibility Policy (Mandatory)
- No native macOS runtime target is planned for FrikaMF.
- Supported approach is a documented Wine/CrossOver compatibility path.
- Stage 1 deliverable: setup documentation (`MAC_SETUP.md`).
- Stage 2 deliverable: optional setup automation script (`tools/install-mac.sh`).
- Not planned: custom Wine/Proton fork or native reimplementation.

## Tooling Language Boundaries (Mandatory)
- Do not introduce language-foreign build or automation scripts unless they provide exceptional development value.
- Allowed script/automation shells in this repo are `PowerShell`, `shell`, `bash`, and `cmd` by default.

## Repository Inventory (Code-Grounded)

### `FrikaMF/` file roles
- `Core.cs`: MelonLoader entrypoint that initializes crash logging, Harmony patches, event dispatch, runtime hooker command handling, and Rust mod loading.
- `CustomEmployeeManager.cs`: Manages custom employee registration/state persistence and injects custom employee cards/actions into the HR UI.
- `EventDispatcher.cs`: Marshals C# event payload structs into unmanaged buffers and forwards them to Rust mods via `FFIBridge.DispatchEvent`.
- `EventIds.cs`: Declares numeric event IDs used as the stable event contract between Harmony patches and Rust listeners.
- `FfiBridge.cs`: Loads native Rust DLLs, resolves required/optional exports, invokes lifecycle callbacks, and unloads libraries.
- `GameApi.cs`: Defines `GameAPITable` and exposes game capability function pointers to native mods through `mod_init`.
- `GameHooks.cs`: Implements safe wrappers over `Il2Cpp` singletons/collections for gameplay state access and mutating operations.
- `HarmonyPatches.cs`: Contains all framework Harmony patch classes that convert game actions into bridge events.
- `Hooker.cs`: Implements command-driven runtime hook auto-installation (scan/catalog) and emits hook bridge install/trigger events.
- `ReleaseVersion.cs`: Defines the framework release version constant used as the single source of truth.

### Rust crates under `Data Center/Mods/RustMods/`
- `[UNVERIFIED — repository path not present in this workspace]`
- No `Cargo.toml` files were found in the current repository snapshot, therefore no crate-type (`cdylib`/`lib`/`bin`) could be verified.

### Asset/content packs under `Data Center_Data/StreamingAssets/Mods/`
- `[UNVERIFIED — repository path not present in this workspace]`
- No in-repo content pack directory entries were found under this path in the current snapshot.

### Harmony patch attributes grouped by target
- `Player`: `UpdateCoin`, `UpdateXP`, `UpdateReputation` (`FrikaMF/HarmonyPatches.cs`)
- `Server`: `PowerButton`, `ItIsBroken`, `RepairDevice`, `ServerInsertedInRack`, `RegisterLink`, `UnregisterLink`, `UpdateCustomer`, `UpdateAppID` (`FrikaMF/HarmonyPatches.cs`)
- `TimeController`: `Update` (`FrikaMF/HarmonyPatches.cs`)
- `MainGameManager`: `ButtonCustomerChosen`, `ButtonBuyWall` (`FrikaMF/HarmonyPatches.cs`)
- `ComputerShop`: `ButtonCheckOut`, `ButtonBuyShopItem`, `ButtonClear`, `RemoveSpawnedItem` (`FrikaMF/HarmonyPatches.cs`)
- `HRSystem`: `ButtonConfirmHire`, `ButtonConfirmFireEmployee`, `ButtonCancelBuying`, `OnEnable` (`FrikaMF/HarmonyPatches.cs`)
- `SaveSystem`: `SaveGame`, `Load`, `AutoSave` (`FrikaMF/HarmonyPatches.cs`)
- `CustomerBase`: `AreAllAppRequirementsMet` (`FrikaMF/HarmonyPatches.cs`)
- `Rack`: `ButtonUnmountRack` (`FrikaMF/HarmonyPatches.cs`)
- `NetworkMap`: `AddBrokenSwitch`, `RemoveBrokenSwitch` (`FrikaMF/HarmonyPatches.cs`)
- `BalanceSheet`: `SaveSnapshot` (`FrikaMF/HarmonyPatches.cs`)
- Additional module patch: `CableSpinner.Start` (`HexLabelMod/HexLabelMod.cs`)

### Native loading / P/Invoke declarations and Rust export mapping
- `LoadLibrary` (`kernel32.dll`): used by `FFIBridge` to load each Rust mod DLL; not a Rust export.
- `GetProcAddress` (`kernel32.dll`): used by `FFIBridge` to resolve Rust exports; not a Rust export.
- `FreeLibrary` (`kernel32.dll`): used by `FFIBridge.Dispose()` for native unload; not a Rust export.
- Expected Rust exports resolved by name in `FFIBridge`:
	- `mod_info`
	- `mod_init`
	- `mod_update` (optional)
	- `mod_fixed_update` (optional)
	- `mod_on_scene_loaded` (optional)
	- `mod_shutdown` (optional)
	- `mod_on_event` (optional)
- Corresponding Rust `extern "C"` implementations are `[UNVERIFIED — Rust crate sources not present in this workspace]`.

### Current version and release metadata
- Current version source: `FrikaMF/ReleaseVersion.cs` → `00.01.0009`.
- Most recent CHANGELOG entries:
	- `00.01.0006` (2026-04-03): automated release versioning/changelog/artifact naming.
	- `0.1.5` (2026-04-02): runtime bridge/hook-event migration and docs/release-flow updates.
- `.wiki/HOOKS.md`: verified hook table location.

## Game Assembly Hooks (Assembly-CSharp)

### Hook table with verification status
| Class | Confirmed wrapper type | Key members seen in repo evidence | IL2CPP RID status | Verification |
|---|---|---|---|---|
| `Server` | `Il2Cpp.Server` | `PowerButton`, `ItIsBroken`, `RepairDevice`, `ServerInsertedInRack`, `RegisterLink`, `UnregisterLink`, `UpdateCustomer`, `UpdateAppID`, `get_isOn`, `get_rackPositionUID` | `[UNVERIFIED — requires dnSpy confirmation]` | ⚠️ unverified |
| `NetworkSwitch` | `Il2Cpp.NetworkSwitch` | `PowerButton`, `RepairDevice`, `ItIsBroken`, `get_isOn`, `get_rackPositionUID`, `ButtonShowNetworkSwitchConfig` | `[UNVERIFIED — requires dnSpy confirmation]` | ⚠️ unverified |
| `CustomerBase` | `Il2Cpp.CustomerBase` | `AreAllAppRequirementsMet`, `UpdateMoney`, `UpdateSpeedOnCustomerBaseApp`, `get_appConnections` | `[UNVERIFIED — requires dnSpy confirmation]` | ⚠️ unverified |
| `SaveData` | `Il2Cpp.SaveData` | `get_version`, `get_lastUsedRackPositionGlobalUID`, `get_trolleyPosition`, `get_trolleyRotation` | `[UNVERIFIED — requires dnSpy confirmation]` | ⚠️ unverified |
| `AssetManagement` | `Il2Cpp.AssetManagement` | `ButtonFilterAll`, `ButtonFilterSwitches`, `ButtonFilterServers`, `ButtonConfirmSendingTechnician` | `[UNVERIFIED — requires dnSpy confirmation]` | ⚠️ unverified |
| `ComputerShop` | `Il2Cpp.ComputerShop` | `ButtonCheckOut`, `ButtonBuyShopItem`, `ButtonClear`, `RemoveSpawnedItem`, `ButtonAssetManagementScreen` | `[UNVERIFIED — requires dnSpy confirmation]` | ⚠️ unverified |

### Verified Hook Targets
- Scope note: current repository includes runtime hook catalog lines and Harmony patch declarations, but no decompiled field-token metadata files; therefore all RIDs/tokens remain unverified.

#### `Server`
- Fully-qualified game class: `[UNVERIFIED — requires dnSpy confirmation]`.
- Confirmed interop wrapper type: `Il2Cpp.Server`.
- Relevant fields observed in code:
	- `isOn` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `isBroken` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `eolTime` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
- Relevant methods observed in code/hooks:
	- `PowerButton(...)` (`signature [UNVERIFIED — requires dnSpy confirmation]`)
	- `ItIsBroken()`
	- `RepairDevice()`
	- `ServerInsertedInRack()`
	- `RegisterLink(...)`
	- `UnregisterLink(...)`
	- `UpdateCustomer(...)`
	- `UpdateAppID(...)`
- IL2CPP tokens (RID): `[UNVERIFIED — requires dnSpy confirmation]`.
- Harmony patch exists: `Yes` (`FrikaMF/HarmonyPatches.cs`).
- Rust FFI hook status: `Implemented via EventDispatcher forwarding`.

#### `NetworkSwitch`
- Fully-qualified game class: `[UNVERIFIED — requires dnSpy confirmation]`.
- Confirmed interop wrapper type: `Il2Cpp.NetworkSwitch`.
- Relevant fields observed in code/hooks:
	- `isOn` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `rackPositionUID` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `temporarilyDisconnectedCables` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
- Relevant methods observed in code/hooks:
	- `PowerButton(...)`
	- `ItIsBroken()`
	- `RepairDevice()`
	- `ClearWarningSign(...)`
	- `ClearErrorSign()`
	- `GetConnectedDevices()`
	- `DisconnectCables()`
	- `ReconnectCables()`
- IL2CPP tokens (RID): `[UNVERIFIED — requires dnSpy confirmation]`.
- Harmony patch exists: `No class-specific patch currently`.
- Rust FFI hook status: `Partial (switch events forwarded through NetworkMap patches only)`.

#### `CustomerBase`
- Fully-qualified game class: `[UNVERIFIED — requires dnSpy confirmation]`.
- Confirmed interop wrapper type: `Il2Cpp.CustomerBase`.
- Relevant fields observed in code/hooks:
	- `customerBaseID` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `satisfiedCustomerCount` (`static`, `type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `appConnections` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
- Relevant methods observed in code/hooks:
	- `AreAllAppRequirementsMet(...)`
	- `UpdateMoney(...)`
	- `UpdateSpeedOnCustomerBaseApp(...)`
	- `AddAppPerformance(...)` (`[UNVERIFIED — requires dnSpy confirmation]`)
- IL2CPP tokens (RID): `[UNVERIFIED — requires dnSpy confirmation]`.
- Harmony patch exists: `Yes` (`FrikaMF/HarmonyPatches.cs`, `AreAllAppRequirementsMet`).
- Rust FFI hook status: `Partial (satisfaction events are forwarded; flow-pause target not yet patched)`.

#### `SaveData`
- Fully-qualified game class: `[UNVERIFIED — requires dnSpy confirmation]`.
- Confirmed interop wrapper type: `Il2Cpp.SaveData`.
- Relevant fields observed in hooks (getter/setter members):
	- `version` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `lastUsedRackPositionGlobalUID` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `trolleyPosition` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `trolleyRotation` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `networkData` (`[UNVERIFIED — requires dnSpy confirmation]`)
	- `shopItemUnlockStates` (`[UNVERIFIED — requires dnSpy confirmation]`)
- Relevant methods observed in hooks:
	- `get_version/set_version`
	- `get_lastUsedRackPositionGlobalUID/set_lastUsedRackPositionGlobalUID`
	- `get_trolleyPosition/set_trolleyPosition`
	- `get_trolleyRotation/set_trolleyRotation`
- IL2CPP tokens (RID): `[UNVERIFIED — requires dnSpy confirmation]`.
- Harmony patch exists: `No`.
- Rust FFI hook status: `Planned only`.

#### `AssetManagement`
- Fully-qualified game class: `[UNVERIFIED — requires dnSpy confirmation]`.
- Confirmed interop wrapper type: `Il2Cpp.AssetManagement`.
- Relevant fields observed in hooks (getter/setter members):
	- `buttonReturn` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `overlayConfirmTechnician` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `technicianInformation` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `serverList` (`[UNVERIFIED — requires dnSpy confirmation]`)
	- `switchList` (`[UNVERIFIED — requires dnSpy confirmation]`)
- Relevant methods observed in hooks:
	- `OnEnable`
	- `ButtonFilterAll`, `ButtonFilterSwitches`, `ButtonFilterServers`, `ButtonFilterBroken`, `ButtonFilterEOL`, `ButtonFilterOff`
	- `ButtonConfirmSendingTechnician`, `ButtonCancelSendingTechnician`, `UpdateTechnicianInformation`
- IL2CPP tokens (RID): `[UNVERIFIED — requires dnSpy confirmation]`.
- Harmony patch exists: `No`.
- Rust FFI hook status: `Planned only`.

#### `ComputerShop`
- Fully-qualified game class: `[UNVERIFIED — requires dnSpy confirmation]`.
- Confirmed interop wrapper type: `Il2Cpp.ComputerShop`.
- Relevant fields observed in hooks (getter/setter members):
	- `buttonCheckOut` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `spawnedItemPositions` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `additionalSpawnPosForPatchpanel` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
	- `additionalSpawnPosForSFPBox` (`type/access [UNVERIFIED — requires dnSpy confirmation]`)
- Relevant methods observed in code/hooks:
	- `ButtonCheckOut()`
	- `ButtonBuyShopItem(...)`
	- `ButtonClear()`
	- `RemoveSpawnedItem(...)`
	- `ButtonAssetManagementScreen()`, `ButtonNetworkMap()`, `ButtonBalanceSheetScreen()`, `ButtonHireScreen()`
- IL2CPP tokens (RID): `[UNVERIFIED — requires dnSpy confirmation]`.
- Harmony patch exists: `Yes` (`FrikaMF/HarmonyPatches.cs`).
- Rust FFI hook status: `Implemented for checkout/cart/item events`.

### Additional required model targets

#### `NetworkSaveData`
- Full field list: `[UNVERIFIED — requires dnSpy confirmation]`.
- Only observed member in hook catalog: `MemberwiseClone`.

#### `PlayerData`
- Observed members related to requested scope:
	- `reputation` via `get_reputation/set_reputation`
	- `position` via `get_position/set_position`
- Fields for `level`/`unlocks`: `[UNVERIFIED — requires dnSpy confirmation]`.

#### `ShopItemSO` / `ShopItem`
- `ShopItemSO` observed members: `MemberwiseClone` and `EnsureRunningOnMainThread` only.
- `ShopItem` observed members include UI-related accessors (`itemIcon`, `unlockButton`, `buttonExtended`) and buy/unlock methods.
- Required fields for injection (`GUID`, `price`, `displayName`, `prefabReference`): `[UNVERIFIED — requires dnSpy confirmation]`.

## Rust FFI Contract

### Current bridge struct status
- `GameContext` struct definition: `[UNVERIFIED — not present in C# bridge]`.
- Current C# bridge contract is `GameAPITable` (`FrikaMF/GameApi.cs`).

### `GameAPITable` C-ABI view
- `ApiVersion`: `u32` — API table version value provided to mods (`API_VERSION = 5`).
- All remaining fields are C function pointers represented as `void*` (stored as `IntPtr` in C#):
	- `LogInfo`, `LogWarning`, `LogError`
	- `GetPlayerMoney`, `SetPlayerMoney`
	- `GetTimeScale`, `SetTimeScale`
	- `GetServerCount`, `GetRackCount`, `GetCurrentScene`
	- `GetPlayerXP`, `SetPlayerXP`
	- `GetPlayerReputation`, `SetPlayerReputation`
	- `GetTimeOfDay`, `GetDay`, `GetSecondsInFullDay`, `SetSecondsInFullDay`
	- `GetSwitchCount`, `GetSatisfiedCustomerCount`
	- `SetNetWatchEnabled`, `IsNetWatchEnabled`, `GetNetWatchStats`
	- `GetBrokenServerCount`, `GetBrokenSwitchCount`, `GetEolServerCount`, `GetEolSwitchCount`
	- `GetFreeTechnicianCount`, `GetTotalTechnicianCount`
	- `DispatchRepairServer`, `DispatchRepairSwitch`, `DispatchReplaceServer`, `DispatchReplaceSwitch`
	- `RegisterCustomEmployee`, `IsCustomEmployeeHired`, `FireCustomEmployee`, `RegisterSalary`
	- `ShowNotification`
	- `GetMoneyPerSecond`, `GetExpensesPerSecond`, `GetXpPerSecond`
	- `IsGamePaused`, `SetGamePaused`, `GetDifficulty`, `TriggerSave`

### Rust exports expected by C# loader
- `mod_info() -> ModInfoFFI` (`struct` of `*const c_char` pointers for id/name/version/author/description).
- `mod_init(api_table: *mut c_void) -> bool`.
- `mod_update(delta_time: f32) -> ()`.
- `mod_fixed_update(delta_time: f32) -> ()`.
- `mod_on_scene_loaded(scene_name: *const c_char) -> ()`.
- `mod_shutdown() -> ()`.
- `mod_on_event(event_id: u32, data_ptr: *const u8, data_len: u32) -> ()`.
- Export implementation status in repository: `[UNVERIFIED — Rust source files not present]`.

### C#↔Rust ABI consistency audit
- Calling convention: `Cdecl` is explicitly used for all resolved native delegates in `FFIBridge`.
- `mod_init` return marshalling uses `UnmanagedType.U1` (1-byte bool), which matches common C/Rust bool ABI expectations.
- String payloads use ANSI pointers (`Marshal.StringToHGlobalAnsi` and `PtrToStringAnsi`); Rust side encoding handling is `[UNVERIFIED — requires Rust source confirmation]`.
- No direct `[DllImport]` signatures target Rust DLL exports; all export binding is dynamic via `GetProcAddress`.

### DLL loading behavior
- Load path: recursively scans `Mods/RustMods/**/*.dll` from game root (`FrikaMF/Core.cs` + `FFIBridge.LoadAllMods`).
- Error handling: logs `LoadLibrary` and export failures, continues loading remaining DLLs, and records crash context to `dc_modloader_debug.log`.
- Fallback behavior: missing optional exports (`mod_update`, `mod_on_event`, etc.) are allowed and only reduce capability.

### ABI version check status
- Current state: there is no explicit Rust mod ABI handshake export check (`mod_abi_version`) in the loader.
- Existing partial safeguard: `GameAPITable.ApiVersion` is provided to `mod_init`, but acceptance logic is entirely mod-side.
- Minimal required implementation contract:
	- Add optional Rust export `mod_abi_version() -> u32`.
	- Loader resolves and compares against framework ABI constant before calling `mod_init`.
	- Reject load with explicit log if mismatch occurs.

### Events currently forwarded to Rust
- Economy/player: `MoneyChanged`, `XPChanged`, `ReputationChanged`.
- Server lifecycle: `ServerPowered`, `ServerBroken`, `ServerRepaired`, `ServerInstalled`, `ServerCustomerChanged`, `ServerAppChanged`.
- Time/progression: `DayEnded`, `MonthEnded`.
- Customer: `CustomerAccepted`, `CustomerSatisfied`, `CustomerUnsatisfied`.
- Shop: `ShopCheckout`, `ShopItemAdded`, `ShopCartCleared`, `ShopItemRemoved`.
- HR/custom employee: `EmployeeHired`, `EmployeeFired`, `CustomEmployeeHired`, `CustomEmployeeFired`.
- Infra/save: `CableConnected`, `CableDisconnected`, `RackUnmounted`, `SwitchBroken`, `SwitchRepaired`, `WallPurchased`, `GameSaved`, `GameLoaded`, `GameAutoSaved`.
- Hook automation: `HookBridgeInstalled`, `HookBridgeTriggered`.

### Events required by planned features but not currently forwarded
- DHCP lease assignment/release/renew events (`DHCPLeaseAssigned`, `DHCPLeaseReleased`, `DHCPPoolExhausted`) — not present.
- IPAM snapshot/query events for subnet/address inventory — not present.
- Flow-pause specific interception event for `CustomerBase.AddAppPerformance` — not present.
- VLAN lifecycle/port assignment events (`VlanCreated`, `VlanAssigned`, `PortModeChanged`) — not present.

## Planned Features — Implementation Contracts

### Feature A — DHCP Auto-Assign
- [ ] **Implementation surface**: Rust for lease engine + C# for hook capture/marshalling.
- [ ] **Required hooks**: `Server`/network IP assignment methods (`SetIP` and related) `[UNVERIFIED — requires dnSpy confirmation]`.
- [ ] **Required context fields**: stable access to server identifier, current IP, subnet, and switch/rack linkage (`[UNVERIFIED]`).
- [ ] **Required Rust exports / P/Invoke**: keep `mod_on_event` and add DHCP-specific event payload contracts (no new dependency requirement).
- [ ] **Persistence fields**: extend `NetworkSaveData` with lease table/pool metadata (`[UNVERIFIED schema]`).
- [ ] **Current status**: `not started`.

### Feature B — IPAM Dashboard (IMGUI Overlay)
- [ ] **Implementation surface**: C# preferred for Unity UI/input interoperability; Rust may provide analytics logic only.
- [ ] **Required game class data**: `NetworkMap.servers`, `NetworkMap.switches`, plus `AssetManagement` lists (`serverList`, `switchList`) `[UNVERIFIED exact members]`.
- [ ] **Input handling rule**: use `Keyboard.current` (`UnityEngine.InputSystem`) and do not use legacy `UnityEngine.Input`.
- [ ] **ClassInjector requirement**: no `ClassInjector.RegisterTypeInIl2Cpp` usage exists yet; add registrations before any new IL2CPP `MonoBehaviour` injection.
- [ ] **Current status**: `partial` (input pattern is correct; dashboard implementation absent).

### Feature C — Flow-Pause (CustomerBase intercept)
- [ ] **Target signature**: `CustomerBase.AddAppPerformance(...)` is `[UNVERIFIED — requires dnSpy confirmation]`.
- [ ] **Patch semantics**: Harmony Prefix must return `false` to skip original method execution.
- [ ] **System impact analysis**: verify coupling with server degradation/repair loops before suppressing performance updates.
- [ ] **Bridge contract**: add dedicated flow-pause event forwarding to Rust if policy/logic is native.
- [ ] **Current status**: `not started` (current patch covers `AreAllAppRequirementsMet` only).

### Feature D — VLAN / Switch Port Management
- [ ] **Save schema**: add `NetworkSaveData` fields for VLAN IDs and switch-port assignments (`[UNVERIFIED concrete schema]`).
- [ ] **Shop integration**: identify/create managed-switch license entries in `ShopItemSO` (`[UNVERIFIED identifiers/fields]`).
- [ ] **UI strategy decision**: choose `NetworkSwitch` canvas extension vs standalone IMGUI panel; document chosen path.
- [ ] **State owner**: Rust for VLAN state/validation logic, C# for in-game UI binding and persistence bridge.
- [ ] **Current status**: `not started`.

### Feature E — Educational Tooltips / Subnetting Helper
- [ ] **UI placement**: IPAM panel first; optional projections to server/switch canvases and shop explanations.
- [ ] **Content source**: static data under `Data Center_Data/StreamingAssets/Mods/` preferred over hardcoded strings.
- [ ] **Localization**: scaffold multi-language keys with initial EN content.
- [ ] **Current status**: `not started`.

## Known Unknowns
- IL2CPP tokens (RID) for all mod-critical classes/members are not present in repository metadata and must be confirmed in dnSpy.
- Fully-qualified game namespaces for `Server`, `NetworkSwitch`, `CustomerBase`, `SaveData`, `AssetManagement`, `ComputerShop` are not confirmed from direct decompile files in this workspace.
- `CustomerBase.AddAppPerformance(...)` signature is not confirmed.
- `NetworkSaveData` complete field schema is not confirmed.
- `PlayerData` fields for level/unlocks are not confirmed.
- `ShopItemSO` fields for GUID/price/displayName/prefabReference are not confirmed.
- Rust crate layout, crate-type declarations, and actual `#[no_mangle] extern "C"` implementations are not present in this repository snapshot.

## Verification Workflow Requirements
- Before implementing hook-sensitive features, confirm member names/signatures/tokens in dnSpy and record results in `.wiki/HOOKS.md`.
- Keep `.wiki/HOOKS.md` entries version-scoped (game build + date + verifier).
- Any instruction section using `[UNVERIFIED — requires dnSpy confirmation]` must be resolved before shipping gameplay-affecting patches.