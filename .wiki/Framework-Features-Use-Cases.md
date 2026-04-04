# Framework Features & Use Cases

Last updated: 2026-04-03

This page is the complete capability map for `FrikaModdingFramework` and includes practical setup instructions for each major use case.

## 1) Runtime architecture and ownership

Core runtime modules under `FrikaMF/`:

- `Core.cs`: MelonLoader entrypoint and lifecycle orchestration.
- `FfiBridge.cs`: native Rust DLL loading and export dispatch.
- `GameApi.cs`: `GameAPITable` export surface for native mods (`API_VERSION = 7`).
- `EventDispatcher.cs`: C# → Rust event marshalling.
- `HarmonyPatches.cs`: gameplay and UI hook points.
- `GameHooks.cs`: safe game-state read/write wrappers.
- `CustomEmployeeManager.cs`: HR integration and custom employee lifecycle.
- `TechnicianHiring.cs`: technician integration for custom employees.
- `Hooker.cs`: runtime hook auto-installation.
- `AssemblyHookDumpService.cs`: Assembly-CSharp trigger catalog export.
- `MultiplayerBridge.cs`: multiplayer bridge and injected UI.
- `UiModernizer.cs`: Unity UI modernization pass.
- `DC2WebBridge.cs`: web-style bridge (`HTML/CSS/Tailwind/SASS/JS/TS/React` adapters).
- `ModSettingsMenuBridge.cs`: in-game settings chooser (`Game Settings` vs `Mod Settings`).
- `FrameworkLog.cs`: unified framework logging.
- `ReleaseVersion.cs`: framework release source of truth.

## 2) Capability matrix (what the framework enables)

### A. Native Rust mod hosting (FFI)

- Loads native mods from `Data Center/Mods/RustMods/**/*.dll`.
- Resolves exports dynamically (`mod_info`, `mod_init`, optional update/scene/shutdown/event exports).
- Calls optional update callbacks every frame/fixed frame.
- Dispatches typed gameplay events to `mod_on_event`.

Reference: [`FFI-Bridge-Reference`](FFI-Bridge-Reference)

### B. Stable Game API table for native mods

`GameAPITable` capabilities include:

- Economy/progression (`money`, `xp`, `reputation`).
- Time/day control and speed.
- Device/customer/technician metrics.
- Repair/replace dispatch actions.
- Custom employee registration and salary hooks.
- Notification/rate/pause/save controls.
- Steam/P2P function surface and player position.

Reference: [`FFI-Bridge-Reference`](FFI-Bridge-Reference)

### C. Event contract and hook bridge

- Harmony patches map game actions to stable event IDs.
- Typed payload marshalling (`struct`-based) for selected events.
- Duplicate suppression window to prevent patch double-fires.

Reference files: `HarmonyPatches.cs`, `EventDispatcher.cs`, `EventIds.cs`

### D. Runtime hook automation

- Install hooks by scan (`Hooker.InstallByScan(...)`).
- Install hooks from catalog (`Hooker.InstallFromCatalog(...)`).
- Emits hook bridge install/trigger events.

### E. Custom employee and HR extension

- Register custom employees at runtime.
- Inject custom cards/actions into HR UI.
- Hire/fire flow with reputation checks and optional confirmation.
- Save/load hired state and salary re-registration.

### F. Technician helper integration

- Auto-register additional technician definitions.
- Restore technician state after load.
- Connect employee hire/fire to technician workflows.

### G. UI modernization and Web UI bridge

- Non-invasive style modernization (`UiModernizer`).
- DC2WEB bridge for style translation and replacement overlays.
- Supports `HTML/CSS`, `TailwindCSS`, `SASS/SCSS`, `JavaScript`, `TypeScript`, `React JSX/TSX` adapter flow.
- Supports image assets including SVG-first workflow (`SVG`, `PNG`, `JPG/JPEG`, `BMP`, `GIF`, `TGA`).
- Runtime `Mod Settings` panel for toggles and replacement mode.

Reference: [`Web-UI-Bridge`](Web-UI-Bridge)

### H. Multiplayer bridge

- Native multiplayer export bindings.
- Scene-aware menu button injection.
- Runtime multiplayer panel rendering.

### I. Diagnostics and logging

- File crash log (`dc_modloader_debug.log`) via `CrashLog`.
- Structured category logger via `FrameworkLog`.
- Optional switch diagnostics in `GameHooks.DumpSwitchDiagnostics()`.

### J. Hook catalog export for research

- Exports Assembly-CSharp method trigger lines for analysis.
- Produces deterministic sorted output for downstream tooling.

### K. Managed SDK facade (`ModigAPIs`)

High-level managed API helpers are available under `FrikaMF/ModigAPIs/`:

- `ModigGame`: access to raw singleton-backed game surfaces.
- `PlayerApi`: economy/progression helpers (`money`, `xp`, `reputation`).
- `NetworkApi`: network snapshots and break/repair helpers.
- `TimeApi`: day/hour/time-multiplier helpers.
- `UiApi`: notifications and message helpers.
- `WorldApi`: world object discovery helpers.
- `LocalisationApi`: language and text lookup helpers.

### M. Runtime Mod Config System

The framework supports a runtime mod configuration surface (C# wrappers + low-level bridge-facing API) for:

- option registration (`bool`, `int`, `float`)
- runtime panel editing
- immediate persistence to JSON under `UserData/ModConfigs`

Reference: [`Mod Config System`](Mod-Config-System)

### L. Lua/Python/Web FFI integration status

Current implementation status in this repository:

- Rust native FFI host: **implemented**.
- Lua runtime host inside framework: **not implemented**.
- Python runtime host inside framework: **not implemented**.
- Generic HTTP/WebSocket FFI transport to external processes: **not implemented in core bridge**.

What this means in practice:

- Lua and Python are supported through a sidecar architecture (external runtime process) plus the existing C#/Rust bridge contracts.
- Web-facing integration is currently implemented at UI level via `DC2WebBridge` (style/app translation), not as a generic web transport for FFI calls.

## 3) Use case guides (how to build each application flow)

## Use Case 1: Build a Rust event listener mod

1. Place native DLL into `Data Center/Mods/RustMods`.
2. Export at minimum `mod_info` and `mod_init`.
3. Implement optional `mod_on_event(event_id, data_ptr, data_len)`.
4. Parse payloads based on known event IDs and payload shapes.
5. Start game, validate in `MelonLoader/Latest.log` and `dc_modloader_debug.log`.

Minimal skeleton:

```rust
#[no_mangle]
pub extern "C" fn mod_init(_api_table: *mut core::ffi::c_void) -> bool {
    true
}

#[no_mangle]
pub extern "C" fn mod_on_event(event_id: u32, _ptr: *const u8, _len: u32) {
    if event_id == 200 {
        // server powered event
    }
}
```

## Use Case 2: Add a new gameplay event to the framework

1. Add a new ID in `FrikaMF/EventIds.cs`.
2. Add payload struct in `EventDispatcher.cs` if needed.
3. Add a `Fire...` helper in `EventDispatcher.cs`.
4. Patch target method in `HarmonyPatches.cs` and call the helper.
5. Document hook contract in `HOOKS.md` and relevant wiki page.
6. Build and run integration check.

## Use Case 3: Add custom employees to HR

1. Register an employee through `CustomEmployeeManager.Register(...)`.
2. Open HR panel (`HRSystem.OnEnable`) to trigger injection.
3. Use built-in confirm hooks (`ButtonConfirmHire` / `ButtonConfirmFireEmployee`).
4. Validate persistence with save/load cycles.

## Use Case 4: Dispatch automatic repair/replace actions

1. Query available technicians via `GameHooks.GetFreeTechnicianCount()`.
2. Use one of:
   - `GameHooks.DispatchRepairServer()`
   - `GameHooks.DispatchRepairSwitch()`
   - `GameHooks.DispatchReplaceServer()`
   - `GameHooks.DispatchReplaceSwitch()`
3. Handle return values (`1`, `0`, `-1`).

## Use Case 5: Enable Web-based UI styling for an existing screen

1. Register a bundle:

```csharp
DC2WebBridge.RegisterBundle("HRSystem", new Dc2WebBundle
{
    BundleId = "my-hr-style",
    ReplaceExistingUi = false,
    Sources =
    {
        new Dc2WebSource { Type = Dc2WebSourceType.Html, Content = "<div><h1>HR</h1><p>Styled</p></div>" },
        new Dc2WebSource { Type = Dc2WebSourceType.Css, Content = ":root{--panel-color:#0f172acc;--text-color:#f1f5f9;}" },
    }
});
```

1. Apply on target UI root:

```csharp
DC2WebBridge.TryApplyOrReplace(rootGameObject, "HRSystem");
```

1. Toggle runtime behavior from `Mod Settings`.

## Use Case 6: Register a React/TS-style app descriptor

1. Build descriptor with `Framework = "react-ts"`.
2. Provide HTML shell, token CSS, and React-like script snippet.
3. Register with `DC2WebBridge.RegisterWebApp(...)`.
4. Optionally set replacement mode via `DC2WebBridge.SetProfileReplaceMode(...)`.

## Use Case 7: Attach images with SVG preference

1. Build `Dc2WebImageAsset` with `Type = Dc2WebImageType.Svg` and markup/path.
2. Create sprite via `DC2WebBridge.TryCreateSprite(...)` or apply directly with `TryAssignImage(...)`.
3. For complex SVGs, pre-bake externally and provide raster fallback.

## Use Case 8: Offer in-game player-facing mod settings

1. Ensure `MainMenu.Settings` hook is active (`HarmonyPatches.cs`).
2. `ModSettingsMenuBridge` opens chooser:
   - `Game Settings`
   - `Mod Settings`
3. Use toggles to enable/disable DC2WEB and fallback modernizer.

## Use Case 9: Auto-install runtime hooks for discovery

1. Prepare catalog file ([`docs/references/assembly-hooks.txt`](../docs/references/assembly-hooks.txt) or exported dump).
2. Run with hooker options from launch flags (see `Modding-Guide`).
3. Inspect emitted hook bridge events (`HookBridgeInstalled`, `HookBridgeTriggered`).

## Use Case 10: Export trigger catalog from Assembly-CSharp

1. Call `AssemblyHookDumpService.ExportAssemblyCSharpDump(outputPath)`.
2. Review sorted output lines:
   - `runtimetrigger asm[Assembly-CSharp] type[...] method[...]`
3. Feed catalog into Hooker install pipeline.

## Use Case 11: Build a Lua sidecar integration

1. Keep `FrikaMF` as the in-game host (C# + optional Rust DLL).
2. Run Lua in an external process (for example LuaJIT/Lua runtime executable).
3. Bridge data using one of these patterns:
    - C# plugin/host wrappers in your mod DLL, or
    - Rust mod that forwards normalized events/commands to Lua process.
4. Use framework events (`mod_on_event` contract) as the stable inbound stream.
5. Return commands to the game through existing API table operations (money/time/dispatch/etc.).

Recommended scope:

- Use Lua for rule engines, balancing scripts, and fast iteration logic.
- Keep Unity object access and safety-critical calls in C# or Rust.

## Use Case 12: Build a Python sidecar integration

1. Keep game hooks in C# (`HarmonyPatches`) and normalized event forwarding in bridge.
2. Start a Python worker process from your mod runtime or externally.
3. Serialize event payloads as compact structs/JSON messages.
4. Execute analytics/AI/business logic in Python.
5. Send back compact command messages; apply them via `GameAPITable` or `GameHooks` wrappers.

Recommended scope:

- Use Python for analytics, policy evaluation, and model-backed decisions.
- Keep frame-critical hot paths inside C#/Rust to avoid per-frame IPC overhead.

## Use Case 13: Web FFI pattern (external control plane)

1. Expose a local HTTP/WebSocket endpoint from your own mod component.
2. Map incoming web requests to framework-safe actions.
3. Enforce validation/rate limiting before mutating game state.
4. Emit telemetry/events back to clients from `ModFramework.Events` and/or Rust callback stream.

Important distinction:

- `DC2WebBridge` is for UI adaptation/styling in Unity.
- A generic Web FFI transport is a separate integration layer you add on top.

## 4) Player/Developer/Contributor quick routes

- Players: start at [`End-User-Release`](End-User-Release)
- Mod developers: start at [`Mod-Developer-Debug`](Mod-Developer-Debug)
- Contributors: start at [`Contributors-Debug`](Contributors-Debug)

## 5) Known constraints (important)

- React/TS/JS support is adapter-driven; no full browser DOM runtime is embedded.
- No built-in Lua or Python runtime host exists in core framework at this time.
- No built-in generic HTTP/WebSocket FFI transport is shipped in core bridge.
- Steam lobby/event queue parts in API v7 are present but partly stubbed.
- Event ABI is contract-driven; no live schema negotiation yet.

## 6) Maintenance rule for this page

When adding/removing framework capability:

1. Update this page in the same PR.
2. Update audience pages if behavior changes user-facing workflows.
3. Update `HOOKS.md` for hook-surface changes.

## 7) Related links

- [Home](Home)
- [Home EN](Home-en)
- [Modding Guide](Modding-Guide)
- [Architecture](Architecture)
- [Setup](Setup)
- [FFI Bridge Reference](FFI-Bridge-Reference)
- [Mod Config System](Mod-Config-System)
- [Web UI Bridge (DC2WEB)](Web-UI-Bridge)
- [Web UI Bridge (DC2WEB) EN](Web-UI-Bridge-en)
