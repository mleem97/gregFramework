# FrikaMF Core Split Migration

## Migration Checklist

1. Identify core-only framework code and confirm the final `FrikaMF` surface.
2. Move optional runtime features into dedicated standalone plugins under `StandalonePlugins/`.
3. Introduce shared abstractions for plugin registration, hook binding, and reference loading.
4. Add compatibility shims for any moved public types to preserve existing consumers.
5. Update the solution and project references for the new source layout.
6. Add `.gitignore` entries for generated hooks, build outputs, and compressed references.
7. Compress large reference dumps and update any scripts that consume them.
8. Validate the framework build and each extracted plugin independently.
9. Remove any now-redundant core code only after compatibility shims are in place.
10. Commit the refactor in small atomic steps to keep history easy to review.

## Target Layout

```text
src/
  FrikaMF/
    References/
      ReferenceScanner.cs
    Hooks/
      HookBinder.cs
      Generated/
    FFMPluginBase.cs
StandalonePlugins/
  FFM.Plugin.Multiplayer/
  FFM.Plugin.Sysadmin/
  FFM.Plugin.AssetExporter/
  FFM.Plugin.WebUIBridge/
  FFM.Plugin.PlayerModels/
```

## What Stays in Core

- Bootstrap and lifecycle wiring
- Shared framework event dispatch
- Core game access helpers
- Stable public API surface for plugin authors
- Compatibility shims for moved types

## What Moves Out

- Multiplayer bridge and networking UI
- Sysadmin and admin-style utilities
- Export-only diagnostics and asset tooling
- Web UI bridge and other presentation-only helpers
- Optional gameplay extensions that are not required to run the framework

## Notes for Contributors

- Keep public API changes additive when possible.
- Prefer new plugin entry points over expanding the core module.
- Keep generated files out of version control unless explicitly required.
- Update documentation for any renamed namespaces, paths, or project files.
