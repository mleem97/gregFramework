# Store Object Mod Support Task List

## How to Use This List

- Status values: `todo`, `in-progress`, `blocked`, `done`
- Priority values: `P0`, `P1`, `P2`
- Each task includes a concrete definition of done.

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
- Area: `FrikaMF.csproj`
- Tasks:
  - Ensure generated decompiled sources under `References/il2cpp-unpack` are excluded from `Compile`.
  - Ensure generated files are tracked as `None` only.
- Definition of Done:
  - `FrikaMF` build is not compiling generated unpack `.cs` files.

## L0.3 Run compatibility build gate

- Status: `todo`
- Priority: `P0`
- Tasks:
  - Run `dotnet build .\\FrikaMF.csproj -c Release -p:TreatWarningsAsErrors=true -nologo`.
  - Run `dotnet build .\\HexLabelMod\\HexLabelMod.csproj -c Release -nologo`.
- Definition of Done:
  - Both builds pass with current local exports.

## L0.4 Revalidate hook surface after export

- Status: `todo`
- Priority: `P0`
- Area: `FrikaMF/JoniMF/HarmonyPatches.cs`
- Tasks:
  - Verify targeted game methods still exist and signatures match.
  - Log hook drift and required patch updates.
- Definition of Done:
  - Hook compatibility notes are written and actionable changes are listed.

## L0.5 Revalidate event contracts

- Status: `todo`
- Priority: `P0`
- Area: `FrikaMF/JoniMF/EventIds.cs`, `FrikaMF/JoniMF/EventDispatcher.cs`
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
- Area: `FrikaMF/JoniMF` / hook discovery
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
- Area: `FrikaMF` / `FrikaMF/JoniMF`
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
- Area: `FrikaMF/JoniMF/HarmonyPatches.cs`
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
  - `README_MODDING.md`
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
  - `dotnet build .\FrikaMF.csproj -c Release -p:TreatWarningsAsErrors=true -nologo`
  - `dotnet build .\HexLabelMod\HexLabelMod.csproj -c Release -nologo`
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
