<!-- markdownlint-disable MD010 MD022 MD029 MD031 MD032 MD033 -->

# Frikadelle Modding Framework (MelonLoader)

[![.NET CI](https://github.com/mleem97/FrikaModFramework/actions/workflows/dotnet-ci.yml/badge.svg?branch=master)](https://github.com/mleem97/FrikaModFramework/actions/workflows/dotnet-ci.yml)
[![Commit Lint](https://github.com/mleem97/FrikaModFramework/actions/workflows/commitlint.yml/badge.svg?branch=master)](https://github.com/mleem97/FrikaModFramework/actions/workflows/commitlint.yml)
[![Last Commit](https://img.shields.io/github/last-commit/mleem97/FrikaModFramework/master)](https://github.com/mleem97/FrikaModFramework/commits/master)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.txt)

> ⚠️ **Important Notice (Ethics & Usage)**  
> This mod **does not** encourage or endorse the theft, unauthorized reuse, or distribution of code, assets, or any other content created by indie developers. It was developed strictly to support the **modding workflow** for the game *Data Center* (by Waseku) via MelonLoader. It is intended for modders who need a deeper understanding of the game's structure to create legitimate, transformative mods (e.g., custom assets, structural extensions) within a fair modding context.

---

## ✅ Quick Start

### Build the framework and label mod

```powershell
dotnet build .\FrikaMF.csproj -c Release
dotnet build .\HexLabelMod\HexLabelMod.csproj -c Release
```

### Install into the game

1. Copy `bin/Release/net6.0/DataCenterModLoader.dll` to `Data Center/Mods`.
2. Copy `HexLabelMod/bin/Release/net6.0/HexLabelMod.dll` to `Data Center/Mods`.
3. Start the game and verify load in `MelonLoader/Latest.log`.

Before first build/use: run the game once with MelonLoader so required generated assemblies and runtime metadata exist.

### Folder separation (important)

- C# mods: `Data Center/Mods`
- Rust/native plugins: `Data Center/Mods/RustMods`
- Game-object content packs: `Data Center/Data Center_Data/StreamingAssets/Mods`

### New game objects without extra helper mods

You can place object packs directly in `StreamingAssets/Mods` and keep all related data together in one folder.

Example:

```text
Data Center_Data/StreamingAssets/Mods/ExampleServerPack/
	config.json
	model.obj
	model.mtl
	texture.png
	icon.png
```

Scaffold command:

```powershell
pwsh -ExecutionPolicy Bypass -File .\scripts\New-StreamingAssetModPack.ps1 -GamePath "C:\Program Files (x86)\Steam\steamapps\common\Data Center" -ModName "ExampleServerPack"
```

This aligns with the game's native `ExampleMod` approach and your framework goal: object data stays together in `StreamingAssets/Mods/<PackName>`.

### Deploy helper command

```powershell
. .\scripts\Invoke-DataCenterModDeploy.ps1
Invoke-Deploy --all
```

### Local release upload (important)

GitHub Actions **must not** build release DLLs for this repo because game-specific references are local by design.
Release DLLs are uploaded from your local machine after local build:

```powershell
. .\scripts\Publish-LocalRelease.ps1
$env:GITHUB_TOKEN = "<github_token_with_repo_scope>"
Publish-LocalRelease -Tag "v0.1.5"
```

This uploads:

- `bin/Release/net6.0/DataCenterModLoader.dll`
- `HexLabelMod/bin/Release/net6.0/HexLabelMod.dll`

---

## ✅ C# Quality Checks (no pnpm)

```powershell
dotnet build .\FrikaMF.csproj -c Release -p:TreatWarningsAsErrors=true -nologo
dotnet build .\HexLabelMod\HexLabelMod.csproj -c Release -nologo
```

---

## 📖 Overview

**Frikadelle Modding Framework** is a MelonLoader-based framework mod for the game *Data Center*. It allows modders to export in-game content (meshes, textures, etc.) at runtime and provides a central event/hook foundation for custom modding workflows.

**Current Focus:**
- **In-Use Asset Export:** Captures assets actively utilized in the game (both active and inactive objects within loaded scenes).
- **Unused Asset Export (Optional):** Extracts loaded but currently unreferenced assets.
- **UI Helper Functions:** Accelerates UI reference searches and structure mapping.
- **Extended Metadata Export:** Generates detailed text logs regarding components, object settings, material configurations, and general summaries.

---

## ✨ Features

### Hotkeys
- <kbd>F8</kbd> : Start the asset export process.
- <kbd>F9</kbd> : Log the exact UI hierarchy path under the mouse cursor.
- <kbd>F10</kbd> : Toggle Beta Export features (On/Off).

> Dev hotkeys are enabled in `Debug` builds only.
- <kbd>F11</kbd> : Export IL2CPP diagnostics (`il2cpp-event-catalog.txt` + `il2cpp-gameplay-index.txt`).
- <kbd>F12</kbd> : Discover and install runtime hook candidates (framework mode).

At startup, the mod now automatically exports a full consolidated snapshot to:
- `Mods/ExportedAssets/Diagnostics/game-signals-full.txt`

### Export Destinations
Assets are exported to the `Mods/ExportedAssets/CurrentGame` directory, organized as follows:
- `Models/`
- `Textures/`
- `Sprites/`
- `Materials/`
- `Scripts/` (outputs `components.txt`)
- `Settings/` (outputs `objects.txt`, `summary.txt`)

### Unused Assets (Optional)
Exported to `Mods/ExportedAssets/CurrentGame/NotUsed`.
- Separated into `Models/` and `Textures/`.
- Automatically generates a `README_NOT_USED.txt` file in the main `CurrentGame` directory explaining its contents.

---

## 🛠 Technology Stack

- **.NET 6**
- **MelonLoader** (Modding Framework)
- **Unity IL2CPP Interop**
- **Unity Input System**

### Rust Plugin Development

If you want to write plugins in Rust, use the Rust bridge project:

- `https://github.com/Joniii11/DataCenter-RustBridge`

---

## 📋 Prerequisites

To build and use this mod, you need:
1. The game **Data Center** installed.
2. **MelonLoader** installed and configured for the game.
3. **Visual Studio 2022/2026** OR the **.NET 6 SDK** installed on your machine.

---

## 🏗 Build Instructions

Navigate to the project directory in your terminal and run:

```sh
dotnet build FrikaMF.sln -v:minimal

```
The compiled mod will typically be output to:  
`bin/Debug/net6.0/FrikadelleModdingFramework.dll`

---

## 🚀 Installation & Usage

1. Build the project following the instructions above (or download the compiled `.dll`).
2. Copy the `FrikadelleModdingFramework.dll` into the `Mods` folder located inside your *Data Center* game directory.
3. Launch the game.
4. Press the designated hotkeys (<kbd>F8</kbd>, <kbd>F9</kbd>, <kbd>F10</kbd>) in-game to trigger the exports and UI logging.

---

## ⚙️ Runtime Behavior (Technical Details)

- **Scene Hierarchy Traversal:** The export relies on scene hierarchies (including inactive objects) to ensure it only captures "actually built/placed" content.
- **Robust UI Detection:** The <kbd>F9</kbd> UI path detection utilizes C# Reflection. This ensures that missing direct UI assembly references during the mod's compilation do not cause hard crashes at runtime.
- **Smart Filtering:** The `NotUsed` export process includes intelligent filtering to primarily export relevant asset candidates, ignoring obvious internal Unity or micro-assets.

---

## 📁 Project Structure

- `Main.cs` — Central mod logic (Hotkey handling, Export routing, UI Logging).
- `ModEventFramework.cs` + `Events/*.cs` — Central event hub and event-category files for framework-based modding.
- `Il2CppEventCatalogService.cs` — Runtime/decompiled event and trigger discovery exporter.
- `Il2CppGameplayIndexService.cs` — Full gameplay-relevant keyword index over `il2cpp-unpack` sources.
- `RuntimeHookService.cs` — Runtime hook discovery and Harmony-based installation pipeline.
- `AssetExport.md` — Documentation, requirements, and development notes regarding export behavior.
- `ui.md` — UI reference context and layout documentation.

---

## 🤝 Contributing

Contributions are always welcome! If you'd like to help improve the tool, please follow this workflow:

### Workflow
1. **Fork** the repository.
2. Create a new **branch** (`feature/AddCoolThing` or `fix/ExportBug`).
3. Keep your changes small, focused, and well-documented.
4. Test your build locally inside the game.
5. Open a **Pull Request** with a clear and detailed description.

### Where help is needed most

- **Hooks & events:** Expand stable hook coverage in `FrikaMF/JoniMF/HarmonyPatches.cs`.
- **Event transport:** Add strongly typed payloads in `FrikaMF/JoniMF/EventDispatcher.cs`.
- **Game API bridge:** Extend `FrikaMF/JoniMF/GameApi.cs` and `FrikaMF/JoniMF/GameHooks.cs` safely.
- **Docs & examples:** Improve `.wiki/Modding-Guide.md` with practical recipes.

### Contribution Guidelines
- **Strictly No Piracy:** Do not submit PRs that facilitate copyright infringement, asset theft, or bypassing game protections. Exports and features must serve a legitimate modding purpose.
- **Maintain Architecture:** Adhere to the existing coding style and project architecture.
- **Minimal Dependencies:** Do not add unnecessary external dependencies or libraries.
- **Conventional Commits:** Use `feat:`, `fix:`, `docs:`, `chore:`, etc.

---

## 🧩 Build Your Own Mod With This Framework

The project exposes two practical extension paths:

1. **C# side hooks/events**
	- Hook game methods in `FrikaMF/JoniMF/HarmonyPatches.cs`.
	- Emit events through `EventDispatcher` (`FireSimple`, `FireValueChanged`, and typed helpers).
2. **Rust native plugins**
	- Place Rust plugin DLLs in `Data Center/Mods/RustMods`.
	- Implement optional exports like `mod_init`, `mod_update`, `mod_on_event`.
	- Use the Rust bridge reference: `https://github.com/Joniii11/DataCenter-RustBridge`.

### Codemod/Hook bridge example

Use the built-in hooker launch flags to avoid manually wiring thousands of hooks:

```text
--hooker-auto
--hooker-catalog="C:\path\to\assembly-hooks.txt" --hooker-max=5000
```

### Current event ID source

- Event constants are defined in `FrikaMF/JoniMF/EventIds.cs`.
- Event dispatch implementation is in `FrikaMF/JoniMF/EventDispatcher.cs`.
- Runtime forwarding to native plugins happens via `FrikaMF/JoniMF/FfiBridge.cs`.

---

## 🐛 Issues & Bug Reports

If you encounter a bug, please open an issue and include the following information to help us troubleshoot:
- **Game Version**
- **MelonLoader Version**
- **Mod Version / Commit Hash**
- **Exact Reproduction Steps**
- **Relevant Logs / Error Messages** (Attach your `MelonLoader/Latest.log`)

---

## 🔒 Security Policy

If you discover a security vulnerability or a critical exploit, **please do not report it publicly via GitHub Issues.** Instead, report it responsibly through a private channel by contacting the repository maintainer directly.

---

## 📜 Code of Conduct

- **Be Respectful:** Treat all community members with respect.
- **Be Constructive:** Keep feedback helpful and focused on the code/project.
- **Zero Tolerance:** No discriminatory, offensive, or toxic behavior will be tolerated. Repeated violations will result in bans from interacting with the repository.

---

## 📄 License

This project is licensed under the **MIT License**. See the `LICENSE.txt` file for full details.

---

## ⚠️ Disclaimer

This project is a community-driven modding tool. It is **unofficial**, not affiliated with, nor endorsed by Waseku or the developers of *Data Center*. Use it at your own risk.
