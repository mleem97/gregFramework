# GREG FRAMEWORK
**DATA CENTER MODDING API**
[![.NET CI](https://github.com/mleem97/gregFramework/actions/workflows/dotnet-ci.yml/badge.svg?branch=master)](https://github.com/mleem97/gregFramework/actions/workflows/dotnet-ci.yml)
[![Commit Lint](https://github.com/mleem97/gregFramework/actions/workflows/commitlint.yml/badge.svg?branch=master)](https://github.com/mleem97/gregFramework/actions/workflows/commitlint.yml)
[![Last Commit](https://img.shields.io/github/last-commit/mleem97/gregFramework/master)](https://github.com/mleem97/gregFramework/commits/master)
[![License: Custom-NC--NR](https://img.shields.io/badge/license-Custom%20NC--NR-red.svg)](LICENSE.txt)

<img width="838" height="150" alt="gregframeworkapibanner" src="https://github.com/user-attachments/assets/3e78050a-67e8-4eaa-981e-7fa5cfbc466c" />

*FrikaMF** (Frika Mod Framework) is a MelonLoader-based modding stack for **Data Center**: Harmony hooks, events, Rust/native bridge, and optional Workshop tooling. |

## What end users need

- A compatible **Data Center** installation
- **MelonLoader** for your game build
- The framework DLL in the game **`Mods`** folder (see releases or build from source)

## Quick install

1. Build or download **`FrikaModdingFramework.dll`** (output name may match your build configuration).
2. Copy it to: **`Data Center/Mods`**
3. Optional: add companion mods (for example **`FMF.HexLabelMod.dll`**) from releases or build outputs under `mods/`.
4. Start the game and confirm load order in **`MelonLoader/Latest.log`**.

## In-game folder layout

- **C# mods:** `Data Center/Mods`
- **Rust/native plugins:** `Data Center/Mods/RustMods`
- **Content packs:** `Data Center/Data Center_Data/StreamingAssets/Mods`
- **Workshop uploader (optional):** deploy the published MAUI app next to the game — see [`WorkshopUploader/README.md`](WorkshopUploader/README.md) (not inside `Mods` or `MelonLoader`)

## Repository layout (contributors)

| Area | Path | Notes |
|------|------|--------|
| **Framework (runtime)** | [`framework/`](framework/) | Build [`framework/FrikaMF.csproj`](framework/FrikaMF.csproj) or [`FrikaMF.sln`](FrikaMF.sln) |
| **Target monorepo layout (registry, stubs)** | [`FrikaModFramework/`](FrikaModFramework/) | `fmf_hooks.json`, planned bindings/docs stubs |
| **Gameplay mods** | [`mods/`](mods/) | e.g. `mods/FMF.Mod.HexLabelMod/`, `mods/FMF.Mod.GregifyEmployees/` |
| **FFM plugins** | [`plugins/`](plugins/) | e.g. `plugins/FFM.Plugin.AssetExporter/` |
| **Pilot mod layout** | [`HexMod/`](HexMod/) | Workshop VDF + `fmf/hooks.json` example (ties to Hex label mod) |
| **Templates** | [`Templates/`](Templates/), [`templates/`](templates/) | Lowercase `templates/mod` is the current mod scaffold path |
| **Docs (Markdown)** | [`docs/`](docs/) | Source for the public wiki |
| **Docusaurus app** | [`wiki/`](wiki/) | `npm install` / `npm run start` — site base path `/wiki` |
| **Workshop uploader** | [`WorkshopUploader/`](WorkshopUploader/) | .NET MAUI (Windows), Steam Workshop + workspace workflow |
| **MCP server (LLM tools)** | [`mcp-server/`](mcp-server/) | Model Context Protocol over stdio or HTTP — see [`docs/reference/mcp-server.md`](docs/reference/mcp-server.md) |
| **Scripts & tools** | [`scripts/`](scripts/), [`tools/`](tools/) | Releases, hook catalog, scanners, Workshop scripts |
| **RustBridge sync** | [`scripts/Sync-RustBridge.ps1`](scripts/Sync-RustBridge.ps1) | Staged upstream merge (see `CONTRIBUTING.md`) |

Legacy GitHub Wiki sources may still exist under **`.wiki/`**; the **browsable documentation** is built from **`docs/`** + **`wiki/`** (Docusaurus).

## Documentation & wiki (current)

- **Start page (Docusaurus):** [`docs/intro.md`](docs/intro.md) — routes under `/wiki/...` on the built site.
- **Repo inventory & layout:** [`docs/contributors/repo-inventory.md`](docs/contributors/repo-inventory.md)
- **MCP (assistants / IDEs):** [`docs/reference/mcp-server.md`](docs/reference/mcp-server.md)
- **Steam Workshop & tooling:** [`docs/meta/Steam-Workshop-and-Tooling.md`](docs/meta/Steam-Workshop-and-Tooling.md) (or search `Steam-Workshop` in `docs/`)

### Docker (docs + optional MCP in one image)

From the repo root:

```bash
docker compose up docs-mcp
```

Serves the **static** built wiki on **http://localhost:3040** (maps to container port 3000) and exposes MCP on `/mcp`. For **live-reload dev** of the Docusaurus app, use `docker compose up docs` on port **3000** — see [`wiki/README.md`](wiki/README.md).

## Developer tooling

- `python tools/refresh_refs.py` — refresh MelonLoader interop DLLs under `lib/references/` for local builds.
- `python tools/diff_assembly_metadata.py` — compare `Assembly-CSharp` metadata after game updates.
- `tools/Generate-FmfHookCatalog.ps1` — regenerate the hook catalog consumed by `docs/reference/fmf-hooks-catalog.md`.

## Cross-language extension model (`FFM.Langserver.Compat`)

- Drop-in adapters are discovered from: **`Data Center/Mods/FFM.Langserver.Compat`**
- Lets language runtimes (Rust, Python, Lua, etc.) register hook claims without baking every language into the core.
- Diagnostics: **`Data Center/FrikaFM/ffm-langserver-compat-status.json`**
- Conflicting hook claims are logged so mod authors can coordinate.

## Important notes

- Unofficial project — not affiliated with the game developer.
- Do not use this for piracy, unauthorized redistribution, or asset theft.
- License is restrictive: see [`LICENSE.txt`](LICENSE.txt).

## Troubleshooting (quick)

- Mod not loading: **`MelonLoader/Latest.log`**
- Missing output files: check write permissions under the game directory.

## Legacy `.wiki/` links (optional)

Some deep-dive pages still live alongside imports under [`.wiki/`](.wiki/) (Setup, Architecture, HOOKS, etc.). Prefer the **Docusaurus** tree under **`docs/`** for navigation and search; legacy paths are gradually merged.

## Security & contribution

- [Security policy](SECURITY.md)
- [Support](SUPPORT.md)
- [Contributing](CONTRIBUTING.md)
- [AI policy (root)](AI_POLICY.md)

## Thanks

Thanks to the Data Center community for feedback and ideas.

Highlighted for inspiration: **Joniii**, **Mochimus**, **EgoDeath**, **mane_ss**

## License

See [`LICENSE.txt`](LICENSE.txt).
