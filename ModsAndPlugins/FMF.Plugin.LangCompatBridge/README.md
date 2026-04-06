# FMF JoniML Compat Mod

Framework-compatible standalone migration of the former root `JoniML` mod.

## Purpose

- Removes legacy root-mod coupling.
- Provides a compatibility entrypoint that enforces FrikaModdingFramework presence.
- Keeps migration path clean while old sources remain archived in this folder.

## Build

```powershell
dotnet build .\StandaloneMods\FMF.JoniMLCompatMod\FMF.JoniMLCompatMod.csproj
```
