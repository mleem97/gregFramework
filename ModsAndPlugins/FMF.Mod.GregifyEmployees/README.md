# FMF Gregify Employees

This standalone mod replaces all employee visuals with Greg (employee 1) and supports a premium RGB Greg variant.

## Features

- Forces employee/technician models to Greg baseline model.
- Replaces employee card portraits with `image.png`.
- Works continuously to include employees introduced by other mods.
- Registers a custom hire: `RGB Greg` (`greg_rgb_star`) costing `1 Billiarde`.
- RGB Greg applies animated star-like color/emission overlay.

## Requirements

- `FrikaModdingFramework.dll` must be loaded first.
- Place `image.png` in the same folder as `FMF.GregifyEmployees.dll`.

## Build

```powershell
dotnet build .\StandaloneMods\FMF.GregifyEmployees\FMF.GregifyEmployees.csproj
```
