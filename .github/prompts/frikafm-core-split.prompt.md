# FrikaMF Core Split Migration Prompt

You are working in the `FrikaModFramework` repository.

## Goal
Refactor the repository so that `FrikaMF` only contains the minimum code required to operate the core framework. All optional extensions, gameplay add-ons, UI bridges, multiplayer features, sysadmin utilities, and other non-essential runtime features must move into standalone plugins under `StandalonePlugins/`.

## Core Scope
Keep only the code that is required for the framework to boot, load core APIs, dispatch framework events, and provide the stable modder-facing foundation.

## Required Workstreams
1. **Reference Scanner**
   - Add `ReferenceScanner.cs` under `src/FrikaMF/References/`.
   - Load all `.dll` files recursively from the `References` tree.
   - Skip already-loaded assemblies by name.
   - Ignore native DLLs gracefully.
   - Expose type resolution across loaded assemblies.

2. **Hook Binder and Aliases**
   - Add `HookBinder.cs` under `src/FrikaMF/Hooks/`.
   - Map raw `Assembly-CSharp` methods to `FFM.{Category}.{MethodName}` aliases.
   - Support pre- and post-handler registration.
   - Generate hook aliases from `docs/references/assembly-hooks.txt` or an equivalent build-time source.

3. **Standalone Plugin Base**
   - Add `FFMPluginBase` for all standalone plugins.
   - Register plugins through a central registry instead of hard-coding optional subsystems in the core.

4. **Plugin Extraction**
   - Move multiplayer functionality into `StandalonePlugins/FFM.Plugin.Multiplayer`.
   - Move sysadmin functionality into `StandalonePlugins/FFM.Plugin.Sysadmin`.
   - Move exporter and web UI bridge features into separate standalone plugins.
   - Preserve backwards compatibility with `[Obsolete]` forwarding shims where needed.

5. **Packaging and Build Hygiene**
   - Update `.gitignore` for generated files, build output, and compressed reference artifacts.
   - Compress large reference dumps and document decompression steps.
   - Update `FrikaMF.sln` and project references for the new layout.

6. **Documentation and Migration**
   - Add `MIGRATION.md` with the ordered migration checklist.
   - Document what moved, what stayed in core, and what plugin authors must change.

## Constraints
- Do not change MelonLoader bootstrapping.
- Do not alter changelog format or commitlint rules.
- Do not modify wiki content.
- Keep the refactor backwards compatible for existing consumers.
- If a class moves, add a compatibility shim in the old namespace or path.

## Output Expectations
- Implement the changes in small, reviewable commits.
- Prefer minimal, mechanical moves over behavior changes.
- Preserve existing runtime behavior unless a feature is explicitly being extracted.
- Validate the build after each major step.
