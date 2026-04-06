<!-- markdownlint-disable MD024 -->

# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project uses framework release versions in `XX.XX.XXXX` format.

## [Unreleased]

### Added

- Live-Sync tooling: `tools/refresh_refs.py`, `tools/diff_assembly_metadata.py`, and `lib/references/` layout (vendored MelonLoader interop; gitignored DLLs).
- Discord backlog bridge (`tools/discord_bridge.py`), `docs/IDEA_BACKLOG.md`, and `tools/auto_issue_creator.py` (GitHub CLI).
- `WorkshopUploader` WinForms app (Workshop stub + DevServer betas fetch) and docs (`docs/Steam-Workshop-and-Tooling.md`, `docs/devserver-betas.md`).
- Ralph prompts and AI instructions updated for game-update workflows and `lib/references/` as the type surface.

### Changed

- Repository layout: former `ModsAndPlugins/` split into top-level `mods/` (gameplay mods) and `plugins/` (`FFM.Plugin.*`); `FrikaMF.sln` references `plugins\...`; `FrikaMF.csproj` excludes `WorkshopUploader/**` from the default compile glob.
- `FrikaMF.csproj` prefers `lib/references/MelonLoader` when present; excludes `Templates/**` from compile; ships IL2CPP catalog/hook diagnostics from `Core` (debug snapshot).
- Maintain Keep a Changelog structure and release metadata automation.

## [00.01.0012] - 2026-04-04

### Changed

- Automated release metadata update.

## [00.01.0009] - 2026-04-04

### Added

- Add standalone-mod focused wiki pages for Docusaurus-ready structure (`StandaloneMods`, `Release-Assets-and-Templates`, `Repository-Status-2026-04-04`, `Community-Thanks`).
- Add community contribution templates (`.github/ISSUE_TEMPLATE/*`, `.github/pull_request_template.md`, `.github/DISCUSSION_TEMPLATE/ideas.yml`).
- Add release bundle templates under `Templates/*` for standalone mods, UI workflow, and `FFM.Langserver.Compat` adapters.

### Changed

- Normalize legacy changelog version history to `XX.XX.XXXX` format down to initial release.
- Extend release workflow to upload a full modding bundle ZIP with templates, scripts, docs, and UI runtime assets.
- Update roadmap/tasklist/modding guide to reflect current standalone architecture and compatibility goals.
- Add explicit community thanks in repository and wiki documentation.

## [00.01.0008] - 2026-04-04

### Added

- Add `FFM.Langserver.Compat` runtime discovery for cross-language adapter manifests in `Mods/FFM.Langserver.Compat`.
- Add compatibility diagnostics export to `FrikaFM/ffm-langserver-compat-status.json`.
- Add release runner workflow `.github/workflows/release-assets.yml` to upload built DLLs to GitHub Releases.

### Changed

- Add detection warnings for external FFI/bridge DLL candidates and Harmony hook owner overlaps.
- Align standalone mod assembly versions to `00.01.0008`.
- Extend `README.md` with related project link (`DataCenter_DHCPSwitches`) and Langserver compat model.

## [00.01.0007] - 2026-04-04

### Changed

- Migrate remaining root-level mods to framework-compatible standalone projects under `StandaloneMods`.
- Move `HexLabelMod` sources into `StandaloneMods/FMF.HexLabelMod` and add standalone project metadata.
- Move legacy `JoniML` sources into `StandaloneMods/FMF.JoniMLCompatMod` and provide FMF compatibility entrypoint.

## [00.01.0006] - 2026-04-03

### Changed

- Automate release versioning, changelog updates, and artifact naming.

## [00.02.0000] - 2026-04-03

### Features

- **modding:** add StreamingAssets modpack scaffold and docs ([819c883](https://github.com/mleem97/gregFramework/commit/819c883524c30145d94aabe439acdba01d517cd3))
- **runtime:** migrate JoniMF bridge and automate hook installation ([0dc8f1b](https://github.com/mleem97/gregFramework/commit/0dc8f1b7e94d6f1ba19066829c5773d9a7d3e352))

## [00.01.0005] - 2026-04-02

### Added

- Migrate active runtime bridge and hook/event layer into `JoniMF` + `FrikaMF` structure.
- Restore `EventDispatcher` and `EventIds` integration for runtime patch dispatch.

### Changed

- Expand README, modding guide, and wiki with contribution, build, and hooks/events usage guidance.
- Switch to local DLL upload flow for GitHub releases (game-dependent refs stay local).

## [00.01.0004] - 2026-04-01

### Fixed

- Use compatible MelonInfo signature to restore mod loading.

## [00.01.0003] - 2026-04-01

### Fixed

- Switch repository slug from underscore to hyphen.
- Update melon author/url and repository badge links.

## [00.01.0002] - 2026-04-01

### Fixed

- Use Il2Cpp list for EventSystem raycast results.

## [00.01.0001] - 2026-04-01

### Fixed

- Remove duplicate CI compile include for `Main.CI.cs`.

## [00.01.0000] - 2026-04-01

### Added

- Add initial `FrikaMF` project and core mod code.
- Extend export structure and beta export functionality.

[Unreleased]: https://github.com/mleem97/gregFramework/compare/v00.01.0012...HEAD
[00.01.0009]: https://github.com/mleem97/gregFramework/compare/v00.01.0008...v00.01.0009
[00.01.0008]: https://github.com/mleem97/gregFramework/compare/v00.01.0007...v00.01.0008
[00.01.0007]: https://github.com/mleem97/gregFramework/compare/v00.01.0006...v00.01.0007
[00.01.0006]: https://github.com/mleem97/gregFramework/compare/v00.01.0005...v00.01.0006
[00.02.0000]: https://github.com/mleem97/gregFramework/compare/v00.01.0005...v00.02.0000
[00.01.0005]: https://github.com/mleem97/gregFramework/compare/v00.01.0004...v00.01.0005
[00.01.0004]: https://github.com/mleem97/gregFramework/compare/v00.01.0003...v00.01.0004
[00.01.0003]: https://github.com/mleem97/gregFramework/compare/v00.01.0002...v00.01.0003
[00.01.0002]: https://github.com/mleem97/gregFramework/compare/v00.01.0001...v00.01.0002
[00.01.0001]: https://github.com/mleem97/gregFramework/compare/v00.01.0000...v00.01.0001
[00.01.0000]: https://github.com/mleem97/gregFramework/compare/3b2b394e6151a389abb9006c36890f3ec97f6346...v00.01.0000
[00.01.0012]: https://github.com/mleem97/gregFramework/compare/v00.01.0011...v00.01.0012

