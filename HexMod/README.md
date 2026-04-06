# HexMod (Pilot-Layout)

This folder follows the **target** layout from the monorepo plan. The **authoritative C# sources** for the hex label mod still live under:

- [`mods/FMF.Mod.HexLabelMod/`](../mods/FMF.Mod.HexLabelMod/)

## Layout

| Path | Purpose |
|------|---------|
| `fmf/hooks.json` | Declared / subscribed FMF hooks for this mod (pilot). |
| `workshop/` | Steam Workshop VDF for Data Center (AppID 4170200). |
| `docs/` | Mod-specific wiki notes; mirror into repo [`docs/`](../docs/) for Docusaurus if desired. |
| `assets/` | Preview image and optional UI assets (add `preview.png` for Workshop). |

## Next steps

1. Gradually move or symlink `mods/FMF.Mod.HexLabelMod` sources into `HexMod/src/csharp/` (or keep project path and update this README).
2. Point `workshop/hexmod.vdf` `contentfolder` at your build output folder.
3. Add `assets/preview.png` (512×512 recommended).
