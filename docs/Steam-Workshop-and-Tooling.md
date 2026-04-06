# Steam Workshop and tooling

## Live-Sync references

After a *Data Center* update, run the game once so MelonLoader regenerates interop assemblies, then from the repo root:

```bash
python tools/refresh_refs.py
```

Optionally save a baseline for diffs:

```bash
python tools/diff_assembly_metadata.py --save-snapshot
```

After future updates:

```bash
python tools/diff_assembly_metadata.py
```

Do **not** commit `*.dll` from `lib/references/` (see `.gitignore`).

## Steam Workshop (research)

Official upload and item layout for *Data Center* Workshop content may be undocumented by the developer. Until documented:

- Treat Workshop delivery as **game-defined** (often content under game data / `StreamingAssets`; MelonLoader mods remain **DLLs in `Mods/`**).
- The **WorkshopUploader** desktop app (see `WorkshopUploader/`) is the supported path for authors to manage Workshop items and DevServer betas once Steamworks is configured.

## Legal

Do not redistribute game binaries or extracted assets. Workshop packages should contain **your** content only.

## CI / agents

- `FrikaMF.csproj` builds on Windows agents that have either a Steam *Data Center* install **or** a populated `lib/references/MelonLoader/` (from `refresh_refs.py`).
- `WorkshopUploader` targets `net6.0-windows`; build it on Windows (not Linux-hosted runners unless cross-compilation is configured).
