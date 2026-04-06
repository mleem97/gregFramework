using System;
using System.Collections.Generic;
using MelonLoader;
using MelonLoader.Utils;

namespace FFM.PlayerModels;

/// <summary>
/// Public API for loading and assigning custom player and NPC models at runtime.
/// </summary>
public static class API
{
    private static bool _initialized;

    /// <summary>
    /// Initializes the player-models subsystem.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        MelonLogger.Msg("FFM.PlayerModels.API ready.");
    }

    /// <summary>
    /// Loads a model by name using StreamingAssets first, then mod-local fallback.
    /// </summary>
    /// <param name="modelName">Model bundle file name without extension.</param>
    /// <returns>Loaded model container or <c>null</c> when loading fails.</returns>
    public static FFM.Plugin.PlayerModels.LoadedPlayerModel LoadModel(string modelName)
    {
        return FFM.Plugin.PlayerModels.ModelLoader.LoadModel(modelName);
    }

    /// <summary>
    /// Registers a custom player model for a specific player identifier.
    /// </summary>
    public static void AssignModelToPlayer(string playerId, string modelName)
    {
        FFM.Plugin.PlayerModels.PlayerModelSwapper.AssignModelToPlayer(playerId, modelName);

#if FFM_MULTIPLAYER
        MelonLogger.Msg($"FFM.PlayerModels: multiplayer sync requested for player '{playerId}' and model '{modelName}'.");
#endif
    }

    /// <summary>
    /// Assigns a custom model to the local player and persists preference path-wise.
    /// </summary>
    public static void SetLocalPlayerModel(string modelName)
    {
        string localId = "local-player";
        AssignModelToPlayer(localId, modelName);

        string path = System.IO.Path.Combine(MelonEnvironment.UserDataDirectory, "FrikaFM", "local-player-model.txt");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path) ?? MelonEnvironment.UserDataDirectory);
        System.IO.File.WriteAllText(path, modelName ?? string.Empty);
    }

    /// <summary>
    /// Re-applies a previously assigned model to a player object.
    /// </summary>
    public static void RefreshModel(string playerId)
    {
        FFM.Plugin.PlayerModels.PlayerModelSwapper.RefreshModel(playerId);
    }

    /// <summary>
    /// Replaces an NPC model by identifier.
    /// </summary>
    public static void ReplaceNPCModel(string npcId, string modelName, bool persistent = true)
    {
        FFM.Plugin.PlayerModels.NPCModelReplacer.ReplaceNPCModel(npcId, modelName, persistent);
    }

    /// <summary>
    /// Reverts a persistent NPC replacement mapping.
    /// </summary>
    public static void RevertNPC(string npcId)
    {
        FFM.Plugin.PlayerModels.NPCModelReplacer.RevertNPC(npcId);
    }

    /// <summary>
    /// Gets all registered model names.
    /// </summary>
    public static IEnumerable<string> RegisteredModels => FFM.Plugin.PlayerModels.FFMModelRegistry.RegisteredModels;

    /// <summary>
    /// Gets metadata for a registered model.
    /// </summary>
    public static FFM.Plugin.PlayerModels.ModelMetadata GetMetadata(string modelName)
    {
        return FFM.Plugin.PlayerModels.FFMModelRegistry.GetMetadata(modelName);
    }
}
