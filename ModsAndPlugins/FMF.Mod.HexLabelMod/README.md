# FMF HexLabel Mod

Standalone version of the former root `HexLabelMod`, rewritten for the FrikaModdingFramework workflow.

## Requirements

- `FrikaModdingFramework.dll` must be loaded first.
- MelonLoader + generated IL2CPP assemblies.

## Build

```powershell
dotnet build .\StandaloneMods\FMF.HexLabelMod\FMF.HexLabelMod.csproj
```

## Notes

- Original gameplay behavior remains intact.
- This mod now runs from `StandaloneMods` and no longer from repository root.
