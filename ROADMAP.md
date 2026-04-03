# Data Center Mod Support Roadmap

## Purpose

This roadmap defines what is already possible in the framework today and what should be built next now that the game officially opened mod support for adding new store objects.

## Current State (Baseline)

## Already Possible (Verified in Repository)

- Framework migration and modular structure (`FrikaMF`, `FrikaMF/JoniMF`, `HexLabelMod`) are in place.
- Runtime hook/event bridge exists:
  - Harmony patch layer (`FrikaMF/JoniMF/HarmonyPatches.cs`)
  - Event IDs (`FrikaMF/JoniMF/EventIds.cs`)
  - Dispatch transport (`FrikaMF/JoniMF/EventDispatcher.cs`)
- Native plugin bridge exists (`FrikaMF/JoniMF/FfiBridge.cs`) with event forwarding support.
- C# API bridge exists (`FrikaMF/JoniMF/GameApi.cs`, `FrikaMF/JoniMF/GameHooks.cs`) for runtime game access.
- Local release flow exists and is aligned with local game DLL constraints (`scripts/Publish-LocalRelease.ps1`).

## Partially Available / Needs Validation

- Stable event coverage for all gameplay/store actions.
- Data contracts for object definition files (JSON/XML + assets) are not yet formalized in this repo.
- End-to-end insertion pipeline for custom store items is not yet documented as a reproducible flow.

## Newly Opened Opportunity (Game Side)

The game now supports adding custom objects to the store through mod content packs (config + model + texture + icon). This unlocks a new feature stream:

- Data-driven object definitions
- Asset bundle or file-based import pipeline
- Store registration and runtime validation hooks

## Patch Intelligence (v1.0.40)

Confirmed from latest patch notes:

- Modding first draft is now game-native:
  - Content location: `Data Center_Data/StreamingAssets/Mods`
  - Structure: one mod per folder
  - Inputs: `.obj` files + config (see `ExampleMod` in game files)
  - Result: custom objects appear in shop and are now save/load compatible
- QoL change: SFP modules can be inserted directly from SFP box.
- Stats migration:
  - Old stat key `STAT_TOTALCABLELENGTH` is broken in base game
  - Replacement key is `STAT_TOTALCABLELENGTH2`
- Steam stats integrity:
  - Money-per-second is rounded before sending to Steam stats.

### Implications for This Framework

- Prefer integration with native `StreamingAssets/Mods` pipeline before adding heavy Harmony-based store injection.
- Add schema discovery and validation based on `ExampleMod` as P0.
- Treat stat IDs as versioned contracts and migrate framework mappings to `STAT_TOTALCABLELENGTH2`.
- Keep numeric stat/event payload normalization (rounding) in telemetry bridge paths.

### Candidate Modding Domains from Recent Updates

- Device ecosystem is expanding quickly (SFP/QSFP modules, switches, patch panels, rack systems).
- Feature candidates for content packs:
  - Additional switches and server variants
  - Additional rack/rackmount object sets
  - Fiber/network component variants (where supported by game-side schema)
- World/space-related changes exist across updates; expansion support should be treated as capability-gated and validated against current runtime APIs.

## Strategic Goals

1. Deliver a reliable custom-object pipeline from file to in-game store.
2. Keep compatibility stable across game updates (v1.0.40+ and future patches).
3. Provide clear authoring docs so mod creators can build without reverse engineering.
4. Maintain safe, testable extension points in `Assembly-CSharp` hook surface.

## Library Intake Process (Every New Melon Export)

Each time game libraries are re-exported, run this intake gate before feature work:

1. Snapshot incoming libraries (version, export date, source branch/tag).
1. Run compatibility builds:

   - `dotnet build .\\FrikaMF.csproj -c Release -p:TreatWarningsAsErrors=true -nologo`
   - `dotnet build .\\HexLabelMod\\HexLabelMod.csproj -c Release -nologo`

1. Revalidate hook points in `FrikaMF/JoniMF/HarmonyPatches.cs` against `Assembly-CSharp` method signatures.
1. Compare event surface (`FrikaMF/JoniMF/EventIds.cs` + dispatch callsites) for added/removed gameplay events.
1. Update changelog compatibility note and release checklist before tagging.

### Intake Exit Criteria

- Both release builds pass.
- Hook map drift is documented and resolved.
- Event surface changes are documented.
- `TASKLIST.md` intake items are updated with status for this export cycle.

## Phase Plan

## Phase 0 — Capability Audit (Immediate)

### Outcome

A verified matrix of what works now for store object insertion.

### Work Items

- Identify exact game entry points for store item registration/loading.
- Trace config schema usage in `Assembly-CSharp` (JSON/XML parser paths).
- Map required asset files and mandatory fields.
- Record failure modes (missing model, bad collider, bad icon path, etc.).

### Exit Criteria

- Capability matrix completed (supported fields, unsupported fields, limits).
- One reproducible sample object can be loaded manually.

## Phase 1 — MVP Store Object Pipeline

### Outcome

First stable implementation where custom objects can be dropped into mod folder and appear in store.

### Work Items

- Add loader service for custom object manifests.
- Add validation layer (schema + file existence + numeric ranges).
- Add registration hooks to insert objects into store lists.
- Add runtime error reporting to logs and diagnostics output.

### Exit Criteria

- At least 3 sample objects load and appear in store.
- Invalid object packs fail gracefully without crashing gameplay.

## Phase 2 — Authoring Toolkit and Docs

### Outcome

Mod creators can self-serve with templates and clear instructions.

### Work Items

- Publish `config.json` template and field reference.
- Add sample object pack with working assets.
- Add “Common Errors” troubleshooting section.
- Add compatibility/versioning guidance per game version.

### Exit Criteria

- New contributor can create and load one custom object in under 30 minutes.

## Phase 3 — Advanced Features

### Outcome

Support richer content and balancing.

### Work Items

- Category tagging and store filtering for modded items.
- Price/economy balancing hooks.
- Optional localization support for item names/descriptions.
- Optional dependency metadata (required framework/game version).

### Exit Criteria

- Modded objects support categories + localization + optional dependencies.

## Phase 4 — Stability and Ecosystem

### Outcome

Long-term maintainability and creator ecosystem support.

### Work Items

- Regression suite against known game versions.
- Backward compatibility policy for mod pack schema.
- Publish migration guides when schema changes.
- Add release cadence and changelog discipline for mod authors.

### Exit Criteria

- Release process includes compatibility statement and migration notes.

## Priority Matrix

## P0 (Now)

- Capability audit
- MVP insertion pipeline
- Validation + crash-safe error handling
- Recurring game-library intake gate on each new export

## P1 (Next)

- Authoring docs/templates
- Sample packs and troubleshooting

## P2 (Later)

- Advanced balancing/localization/dependencies
- Expanded regression coverage

## Risks and Mitigations

- Game updates break hook points:
  - Mitigation: centralize hook mapping, add fast smoke tests.
- Incomplete asset validation leads to runtime crashes:
  - Mitigation: strict pre-load validation and safe fallback behavior.
- Schema drift confuses mod creators:
  - Mitigation: versioned schema and migration notes.

## Suggested Milestones

- M1: Capability audit report complete
- M2: MVP custom objects appear in store
- M3: Public authoring guide + templates
- M4: Advanced feature rollout
- M5: Stability and compatibility baseline

## Definition of Success

Success means creators can add custom store objects with predictable behavior, clear docs, and no unsafe runtime side effects.
