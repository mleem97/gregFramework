# Frikadelle Modding Framework (MelonLoader)

[![.NET CI](https://github.com/mleem97/FrikaModFramework/actions/workflows/dotnet-ci.yml/badge.svg?branch=master)](https://github.com/mleem97/FrikaModFramework/actions/workflows/dotnet-ci.yml)
[![Commit Lint](https://github.com/mleem97/FrikaModFramework/actions/workflows/commitlint.yml/badge.svg?branch=master)](https://github.com/mleem97/FrikaModFramework/actions/workflows/commitlint.yml)
[![Last Commit](https://img.shields.io/github/last-commit/mleem97/FrikaModFramework/master)](https://github.com/mleem97/FrikaModFramework/commits/master)
[![License: Custom-NC--NR](https://img.shields.io/badge/license-Custom%20NC--NR-red.svg)](LICENSE.txt)

`FrikaModFramework` is a modding framework for `Data Center` using MelonLoader.

## What end users need

- A compatible `Data Center` installation
- MelonLoader installed for your game version
- Framework DLL in the game `Mods` folder

## Quick install

1. Build or obtain `FrikaModdingFramework.dll`.
2. Copy it to: `Data Center/Mods`
3. Optional: copy `HexLabelMod.dll` to the same folder.
4. Start the game and verify loading in `MelonLoader/Latest.log`.

## Folder layout

- C# mods: `Data Center/Mods`
- Rust/native plugins: `Data Center/Mods/RustMods`
- Content packs: `Data Center/Data Center_Data/StreamingAssets/Mods`

## Important notes

- This project is unofficial and not affiliated with the game developer.
- Do not use this project for piracy, unauthorized reuse, or content theft.
- License is restrictive: redistribution and commercial use are prohibited unless explicitly permitted in writing (see `LICENSE.txt`).

## Troubleshooting (quick)

- Mod not loading: check `MelonLoader/Latest.log`.
- Missing diagnostics/output: verify write permissions in the game directory.

## Wiki (all technical details)

For development, architecture, hooks, workflows, and deep documentation, use the wiki:

- Local wiki source: `./.wiki/`
- Setup: `./.wiki/Setup.md`
- Architecture: `./.wiki/Architecture.md`
- Modding Guide: `./.wiki/Modding-Guide.md`
- Device Reference: `./.wiki/Device-Reference.md`
- Verified Hooks: `./.wiki/HOOKS.md`
- Asset Export Notes: `./.wiki/AssetExport.md`
- UI Reference: `./.wiki/ui.md`
- MelonLoader Notes: `./.wiki/MelonLoader.md`
- Roadmap: `./.wiki/ROADMAP.md`
- Task List: `./.wiki/TASKLIST.md`

## Security and support

- Security policy: `SECURITY.md`
- Support: `SUPPORT.md`
- Contribution process: `CONTRIBUTING.md`

## License

See `LICENSE.txt` for full terms.
