# FMF.ConsoleInputGuard

Prevents accidental `P`-hotkey actions while typing into UI text inputs.

## Behavior

- If a Unity `InputField` or `TMP_InputField` is focused, `KeyCode.P` is suppressed.
- Outside focused text input, `P` behaves normally.

## Build

```powershell
dotnet build .\mods\FMF.ConsoleInputGuard\FMF.ConsoleInputGuard.csproj -c Release /p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\Data Center" -v minimal
```
