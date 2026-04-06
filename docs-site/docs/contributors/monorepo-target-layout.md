---
id: monorepo-target-layout
title: Monorepo target layout and migration phases
sidebar_label: Monorepo target layout
description: Planned top-level structure and phased migration without a single big-bang refactor.
---

# Monorepo target layout and migration phases

The repository **stays one Git repo**. The goal is **clear boundaries** between framework, mods, plugins, templates, docs, and tooling so users, modders, and contributors can navigate predictably.

## Target topology (directional)

| Top-level | Purpose |
|-----------|---------|
| `framework/` or keep root `FrikaMF` | Core MelonLoader framework assembly — exact extraction is optional if root layout is documented |
| `mods/` | Gameplay mods (`FMF.Mod.*`, `FMF.*.dll` style) |
| `plugins/` | FFM plugins (`FFM.Plugin.*`) |
| `templates/` | Already exists — scaffolds only |
| `docs-site/` | Docusaurus — **do not rename** without updating `baseUrl`, redirects, and CI |
| `tools/` | Repo maintenance: hook catalog generator, codegen stubs |
| `scripts/` | Release automation (existing) |

**Binaries**: prefer **GitHub Releases** (and pre-releases for beta) over committing DLLs. See [Release channels](../reference/release-channels.md).

## Phased migration (no big-bang)

```mermaid
flowchart LR
  p1[Phase1_DocsAndTools]
  p2[Phase2_MoveModsPlugins]
  p3[Phase3_FrameworkExtract]
  p4[Phase4_CIRedirects]
  p1 --> p2 --> p3 --> p4
```

| Phase | Scope | Exit criteria |
|-------|--------|---------------|
| **1** | Docs, `tools/`, naming wiki, hook catalog script | Docusaurus build green; script generates catalog |
| **2** | `git mv` former `ModsAndPlugins/` → `mods/` / `plugins/` | Done — `.csproj` relative paths unchanged (depth preserved); CI/docs updated |
| **3** | Optional: isolate framework sources under `framework/` | `dotnet build` for solution subset green |
| **4** | CI matrix: docs + dotnet; `plugin-client-redirects` for old URLs | PR checks match local workflow |

## Path updates checklist (Phase 2 applied)

- [x] `FrikaMF.sln` project paths (`plugins\FFM.Plugin.*`)
- [x] `.github/workflows` (CodeQL, release assets, Discord feed)
- [x] Contributor docs and mod/plugin wiki pages (`Project Path` lines)
- [ ] [`docs-site/docusaurus.config.js`](https://github.com/mleem97/gregFramework/blob/master/docs-site/docusaurus.config.js) redirects (only if public URLs must map old paths)
- [ ] Historical wiki-import pages may still mention `StandaloneMods/` — update when editing those files

## Related

- [Repo inventory](./repo-inventory.md)
- [FMF hook naming](../reference/fmf-hook-naming.md)
