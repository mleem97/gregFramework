<!-- markdownlint-disable MD024 -->

# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
This project uses framework release versions in `XX.XX.XXXX` format.

## [Unreleased]

### Changed

- Maintain Keep a Changelog structure and release metadata automation.

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

## [0.2.0] - 2026-04-03

### Features

- **modding:** add StreamingAssets modpack scaffold and docs ([819c883](https://github.com/mleem97/FrikaModFramework/commit/819c883524c30145d94aabe439acdba01d517cd3))
- **runtime:** migrate JoniMF bridge and automate hook installation ([0dc8f1b](https://github.com/mleem97/FrikaModFramework/commit/0dc8f1b7e94d6f1ba19066829c5773d9a7d3e352))

## [0.1.5] - 2026-04-02

### Added

- Migrate active runtime bridge and hook/event layer into `JoniMF` + `FrikaMF` structure.
- Restore `EventDispatcher` and `EventIds` integration for runtime patch dispatch.

### Changed

- Expand README, modding guide, and wiki with contribution, build, and hooks/events usage guidance.
- Switch to local DLL upload flow for GitHub releases (game-dependent refs stay local).

## [0.1.4] - 2026-04-01

### Fixed

- Use compatible MelonInfo signature to restore mod loading.

## [0.1.3] - 2026-04-01

### Fixed

- Switch repository slug from underscore to hyphen.
- Update melon author/url and repository badge links.

## [0.1.2] - 2026-04-01

### Fixed

- Use Il2Cpp list for EventSystem raycast results.

## [0.1.1] - 2026-04-01

### Fixed

- Remove duplicate CI compile include for `Main.CI.cs`.

## [0.1.0] - 2026-04-01

### Added

- Add initial `FrikaMF` project and core mod code.
- Extend export structure and beta export functionality.

[Unreleased]: https://github.com/mleem97/FrikaModFramework/compare/v00.01.0008...HEAD
[00.01.0008]: https://github.com/mleem97/FrikaModFramework/compare/v00.01.0007...v00.01.0008
[00.01.0007]: https://github.com/mleem97/FrikaModFramework/compare/v00.01.0006...v00.01.0007
[00.01.0006]: https://github.com/mleem97/FrikaModFramework/compare/v00.01.0005...v00.01.0006
[0.2.0]: https://github.com/mleem97/FrikaModFramework/compare/v0.1.5...v0.2.0
[0.1.5]: https://github.com/mleem97/FrikaModFramework/compare/v0.1.4...v0.1.5
[0.1.4]: https://github.com/mleem97/FrikaModFramework/compare/v0.1.3...v0.1.4
[0.1.3]: https://github.com/mleem97/FrikaModFramework/compare/v0.1.2...v0.1.3
[0.1.2]: https://github.com/mleem97/FrikaModFramework/compare/v0.1.1...v0.1.2
[0.1.1]: https://github.com/mleem97/FrikaModFramework/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/mleem97/FrikaModFramework/compare/3b2b394e6151a389abb9006c36890f3ec97f6346...v0.1.0

