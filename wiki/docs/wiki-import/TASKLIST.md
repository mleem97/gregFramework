# Store Object Mod Support Task List

## How to Use This List

- Status values: `todo`, `in-progress`, `blocked`, `done`
- Priority values: `P0`, `P1`, `P2`
- Each task includes a concrete definition of done.

## Epic S — Standalone Mod System Sync (P0)

## S1 Ensure all standalone mods are framework-dependent

- Status: `in-progress`
- Priority: `P0`
- Area: `StandaloneMods/*/Main.cs`, `StandaloneMods/*/CompatMain.cs`
- Tasks:
  - Verify runtime dependency checks against `FrikaModdingFramework`.
  - Add integration check that dependency-missing mode deactivates safely.
- Definition of Done:
  - All standalone mods fail gracefully when framework DLL is missing.

## S2 Mirror file-by-file status in wiki

- Status: `in-progress`
- Priority: `P0`
- Area: `.wiki/StandaloneMods.md`, `.wiki/Repository-Status-2026-04-04.md`
- Tasks:
  - Keep per-file status table in sync with repository changes.
  - Mark partial features and link next actions to roadmap.
- Definition of Done:
  - Wiki status pages match repository state for standalone mods and core bridge.

## Epic R — Release Bundle Completeness (P0)

## R1 Upload full modding bundle ZIP in release workflow

- Status: `in-progress`
- Priority: `P0`
- Area: `.github/workflows/release-assets.yml`
- Tasks:
  - Package templates, UI templates, runtime UI payload, docs, and scripts into one ZIP.
  - Upload ZIP to GitHub release along with DLL artifacts.
- Definition of Done:
  - Every release contains DLL assets and one complete modding bundle ZIP.

## Epic G — Gregify Employees (P0)

## G1 Replace all employee visuals with Greg baseline

- Status: `in-progress`
- Priority: `P0`
- Area: `StandaloneMods/FMF.GregifyEmployees/Main.cs`
- Tasks:
  - Apply Greg model replacement to all technicians continuously.
  - Replace employee card/portrait images with `image.png`.
  - Validate behavior for employees introduced by other mods.
- Definition of Done:
  - Base and mod-added hires are Gregified in model and portrait/UI image.

## G2 RGB Greg premium purchase

- Status: `in-progress`
- Priority: `P0`
- Area: `StandaloneMods/FMF.GregifyEmployees/Main.cs`
- Tasks:
  - Keep custom hire entry for `RGB Greg`.
  - Enforce 1 Billiarde cost gate.
  - Apply animated RGB/emission overlay effect.
- Definition of Done:
  - RGB Greg is purchasable and visually glows with dynamic color cycling.

## Epic M — Multiplayer + Plugin Sync (P0/P1)

## M1 Start 16-player multiplayer path

- Status: `in-progress`
- Priority: `P0`
- Area: `FrikaMF/MultiplayerBridge.cs`
- Tasks:
  - Raise remote player baseline to 16.
  - Add sync validation metrics and drift checks.
- Definition of Done:
  - Framework baseline supports 16 remote player slots.

## M2 Add plugin sync bootstrap service

- Status: `in-progress`
- Priority: `P1`
- Area: `FrikaMF/PluginSyncService.cs`
- Tasks:
  - Poll central server manifest.
  - Download plugins in temporary/permanent mode.
  - Add integrity verification and rollback plan.
- Definition of Done:
  - Plugin sync flow is live with safety checks and conflict handling.

## Epic C — Community Enablement (P1)

## C0 Add issue and pull request templates

- Status: `in-progress`
- Priority: `P1`
- Area: `.github/ISSUE_TEMPLATE/*`, `.github/pull_request_template.md`
- Tasks:
  - Add bug report template.
  - Add feature request template.
  - Add pull request checklist template.
- Definition of Done:
  - GitHub UI offers structured forms for bug reports, feature requests, and pull requests.

## Epic L0 — Library Intake Gate (P0, Recurring)

## L0.1 Snapshot new export metadata

- Status: `todo`
- Priority: `P0`
- Tasks:
  - Record game version, export date/time, and source branch/tag.
  - Record path(s) for exported libraries used by local build.
- Definition of Done:
  - Intake log entry exists and is linked in the current release notes draft.

## L0.2 Validate compile item boundaries

- Status: `todo`
- Priority: `P0`
- Area: `framework/FrikaMF.csproj`
- Tasks:
  - Ensure generated decompiled sources under `References/il2cpp-unpack` are excluded from `Compile`.
  - Ensure generated files are tracked as `None` only.
- Definition of Done:
  - `FrikaMF` build is not compiling generated unpack `.cs` files.

## L0.3 Run compatibility build gate

- Status: `todo`
- Priority: `P0`
- Tasks:
  - Run `dotnet build .\\framework\\framework/FrikaMF.csproj -c Release -p:TreatWarningsAsErrors=true -nologo`.
- Definition of Done:
  - Both builds pass with current local exports.

## L0.4 Revalidate hook surface after export

- Status: `todo`
- Priority: `P0`
- Area: `FrikaMF/HarmonyPatches.cs`
- Tasks:
  - Verify targeted game methods still exist and signatures match.
  - Log hook drift and required patch updates.
- Definition of Done:
  - Hook compatibility notes are written and actionable changes are listed.

## L0.5 Revalidate event contracts

- Status: `todo`
- Priority: `P0`
- Area: `FrikaMF/EventIds.cs`, `FrikaMF/EventDispatcher.cs`
- Tasks:
  - Confirm existing event IDs still map to valid game behavior.
  - Identify candidate new events from changed gameplay methods.
- Definition of Done:
  - Event contract delta is documented for the release cycle.

## Epic L1 — v1.0.40 Native Modding Intake (P0)

## L1.1 Validate native content root and layout

- Status: `todo`
- Priority: `P0`
- Area: game runtime integration
- Tasks:
  - Validate path `Data Center_Data/StreamingAssets/Mods` on local install.
  - Confirm one-mod-per-folder loading behavior.
  - Confirm framework docs match exact runtime path conventions.
- Definition of Done:
  - Path and folder contract verified in runtime and documented.

## L1.2 Reverse-map `ExampleMod` schema

- Status: `todo`
- Priority: `P0`
- Tasks:
  - Extract all config keys and required file names from `ExampleMod`.
  - Identify required `.obj` naming and any optional assets.
  - Record shop visibility and save/load persistence requirements.
- Definition of Done:
  - Schema table and minimal valid sample contract are documented.

## L1.3 Build content-pack validator against native contract

- Status: `todo`
- Priority: `P0`
- Area: `FrikaMF` tooling/validation
- Tasks:
  - Add validator rules for folder structure + required config keys + `.obj` existence.
  - Emit clear diagnostics per content pack.
- Definition of Done:
  - Invalid packs are rejected early with actionable error output.

## L1.4 Migrate cable stat mapping

- Status: `todo`
- Priority: `P0`
- Area: stats/event bridge
- Tasks:
  - Replace references to `STAT_TOTALCABLELENGTH` with `STAT_TOTALCABLELENGTH2` where applicable.
  - Add compatibility note in release/changelog workflow.
- Definition of Done:
  - No active code path emits old stat key for cable length.

## L1.5 Normalize Steam stat payloads

- Status: `todo`
- Priority: `P0`
- Area: telemetry dispatch
- Tasks:
  - Ensure money-per-second values are rounded before stat transmission.
  - Add regression test/checklist step for numeric normalization.
- Definition of Done:
  - Stat payload rounding behavior is enforced and documented.

## L1.6 Probe new modding domains (racks/switches/servers/world)

- Status: `todo`
- Priority: `P1`
- Tasks:
  - Validate whether native mod schema supports additional racks/switches/servers.
  - Validate world/space expansion capabilities and limits in current version.
  - Capture unsupported areas as explicit non-goals or hook candidates.
- Definition of Done:
  - Capability matrix includes supported/unsupported mod domains with evidence.

## Epic A — Capability Audit (P0)

## A1. Locate store registration pipeline

- Status: `todo`
- Priority: `P0`
- Area: `FrikaMF` / hook discovery
- Tasks:
  - Find `Assembly-CSharp` methods that populate store inventory/items.
  - Identify when mod folder content is scanned/loaded.
- Definition of Done:
  - Method map documented with class + method names + call timing.

## A2. Reverse-map object config schema

- Status: `todo`
- Priority: `P0`
- Tasks:
  - List supported config fields (name, price, model, icon, collider, etc.).
  - Record field types and constraints.
- Definition of Done:
  - Schema table with required/optional fields and defaults.

## A3. Identify mandatory asset set

- Status: `todo`
- Priority: `P0`
- Tasks:
  - Confirm required files (`model.obj`, `texture.png`, `icon.png`, etc.).
  - Confirm path rules and case sensitivity.
- Definition of Done:
  - Minimal valid pack example is documented.

## Epic B — MVP Implementation (P0)

## B1. Add manifest loader service

- Status: `todo`
- Priority: `P0`
- Area: `FrikaMF`
- Tasks:
  - Implement loader to parse mod object manifests.
  - Keep parse errors isolated per object pack.
- Definition of Done:
  - Loader returns valid objects + structured error list.

## B2. Add strict validation layer

- Status: `todo`
- Priority: `P0`
- Tasks:
  - Validate mandatory fields.
  - Validate numeric ranges and file existence.
  - Validate unsupported keys with warnings.
- Definition of Done:
  - Invalid packs are rejected without runtime crash.

## B3. Hook runtime store insertion

- Status: `todo`
- Priority: `P0`
- Area: `FrikaMF/HarmonyPatches.cs`
- Tasks:
  - Patch store init/update phase.
  - Inject validated custom objects into store list.
- Definition of Done:
  - At least 3 test objects appear and are purchasable.

## B4. Add diagnostics and telemetry

- Status: `todo`
- Priority: `P0`
- Tasks:
  - Log load/validation/registration result per object.
  - Export summary report to diagnostics folder.
- Definition of Done:
  - Diagnostics include success/failure reason per object pack.

## Epic C — Authoring Experience (P1)

## C1. Publish template pack

- Status: `todo`
- Priority: `P1`
- Tasks:
  - Add a `Mods` example pack with valid config + assets.
  - Include comments for each config field.
- Definition of Done:
  - Template works in a clean local test setup.

## C2. Add creator-facing docs

- Status: `todo`
- Priority: `P1`
- Files:
  - `.wiki/Modding-Guide.md`
  - `.wiki/Modding-Guide.md`
- Tasks:
  - Add step-by-step object creation tutorial.
  - Add troubleshooting for common errors.
- Definition of Done:
  - New contributor can follow docs and load one object.

## C3. Add compatibility guidance

- Status: `todo`
- Priority: `P1`
- Tasks:
  - Document supported game version(s).
  - Document expected behavior when schema changes.
- Definition of Done:
  - Compatibility section appears in docs and release notes.

## Epic D — Advanced Features (P2)

## D1. Category and filtering support

- Status: `todo`
- Priority: `P2`
- Tasks:
  - Add category metadata for modded objects.
  - Integrate with store filters/sorting.
- Definition of Done:
  - Modded categories are visible and filterable.

## D2. Localization support

- Status: `todo`
- Priority: `P2`
- Tasks:
  - Add localization key support for name/description.
  - Add fallback behavior when translation missing.
- Definition of Done:
  - At least 2 languages tested with fallback.

## D3. Dependency metadata

- Status: `todo`
- Priority: `P2`
- Tasks:
  - Allow packs to specify min framework/game versions.
  - Warn and skip incompatible packs.
- Definition of Done:
  - Incompatible packs are reported and safely ignored.

## QA Checklist (for each release)

- Status: `todo`
- Priority: `P0`
- Checklist:
  - `dotnet build .\framework\framework/FrikaMF.csproj -c Release -p:TreatWarningsAsErrors=true -nologo`
  - Test valid object pack load
  - Test invalid pack (missing model)
  - Test invalid values (bad collider/scale)
  - Verify store insertion and purchase flow
  - Verify diagnostics output

## Release Checklist (Local DLL Upload)

- Status: `todo`
- Priority: `P0`
- Checklist:
  - Build locally with release config
  - Confirm DLL outputs exist
  - Run local release upload:
    - `. .\scripts\Publish-LocalRelease.ps1`
    - `$env:GITHUB_TOKEN = "<token>"`
    - `Publish-LocalRelease -Tag "vX.Y.Z"`
  - Update changelog and compatibility notes
