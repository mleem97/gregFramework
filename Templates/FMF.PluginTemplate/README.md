# FMF.PluginTemplate

Use this template to create a standalone **FMF Plugin** based on `FFMPluginBase`.

## What this template includes

- `FMF.PluginTemplate.csproj`: Build setup with game directory auto-detection.
- `Main.cs`: Minimal plugin implementation with `PluginId`, version gate, and framework-ready callback.

## Requirements

- MelonLoader installed in the game.
- Generated IL2CPP assemblies (`MelonLoader/Il2CppAssemblies`).
- `FrikaModdingFramework.dll` installed in the game `Mods` folder.

## Quick usage

1. Copy this folder and rename it (for example `FFM.Plugin.MyFeature`).
1. Rename `FMF.PluginTemplate.csproj` and update metadata (`AssemblyName`, `RootNamespace`, `PluginId`).
1. Build:

```powershell
dotnet build .\Templates\FMF.PluginTemplate\FMF.PluginTemplate.csproj /p:GameDir="C:\Path\To\Data Center"
```

1. The built DLL is copied to `Data Center/Mods` automatically (local builds).
