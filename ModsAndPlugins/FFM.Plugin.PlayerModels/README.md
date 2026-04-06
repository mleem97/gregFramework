# FFM.Plugin.PlayerModels

`FFM.Plugin.PlayerModels` adds runtime model loading and replacement for players and NPCs.

## Unity Version

Build your bundles with the same Unity editor family as the game. Target Unity `6000.3.12f`.

## Bundle Placement

The runtime loader checks these locations in order:

1. `{GameRoot}/DataCenter_Data/StreamingAssets/FrikaMF/PlayerModels/{ModelName}.bundle`
2. `{MelonLoader Mods Folder}/FrikaMF/PlayerModels/{ModelName}.bundle`

## Required Prefab Structure

Your bundle must contain a root prefab named exactly `PlayerModel_Root`.

Required components and structure:

- `Animator` on `PlayerModel_Root`
- Humanoid avatar rig
- Standard Unity humanoid bones (`Hips`, `Spine`, `Chest`, `Head`, `LeftUpperArm`, etc.)

Optional nodes:

- `Body` child with `SkinnedMeshRenderer`
- `Voice` child with `AudioSource`

If validation fails, the plugin logs an error and keeps the default in-game model.

## Example Modder Usage

```csharp
FFM.PlayerModels.API.LoadModel("MyCustomPlayer");
FFM.PlayerModels.API.AssignModelToPlayer("76561198000000000", "MyCustomPlayer");
FFM.PlayerModels.API.RefreshModel("76561198000000000");
```

## API Surface

- `FFM.PlayerModels.API.LoadModel(string modelName)`
- `FFM.PlayerModels.API.AssignModelToPlayer(string playerId, string modelName)`
- `FFM.PlayerModels.API.SetLocalPlayerModel(string modelName)`
- `FFM.PlayerModels.API.RefreshModel(string playerId)`
- `FFM.PlayerModels.API.ReplaceNPCModel(string npcId, string modelName, bool persistent = true)`
- `FFM.PlayerModels.API.RevertNPC(string npcId)`

## HookBinder Example

You can subscribe to canonical runtime aliases exposed by `HookBinder`:

```csharp
HookBinder.OnAfter("FFM.Server.OnPowerButton", context =>
{
    MelonLogger.Msg($"Hook hit: {context.HookName}");
});
```

This plugin registers the same sample in `Main.OnFrameworkReady()`.

## Notes

- Multiplayer sync hooks are optional and gated by `FFM_MULTIPLAYER`.
- Current replacement logic preserves existing gameplay objects and swaps visual mesh/material state.
