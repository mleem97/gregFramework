using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace FFM.Plugin.PlayerModels;

/// <summary>
/// Applies registered model assignments to spawned player objects.
/// </summary>
public static class PlayerModelSwapper
{
    private static readonly Dictionary<string, string> AssignedModelsByPlayer = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Assigns a model name to a player identifier.
    /// </summary>
    public static void AssignModelToPlayer(string playerId, string modelName)
    {
        if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(modelName))
            return;

        AssignedModelsByPlayer[playerId] = modelName;
    }

    /// <summary>
    /// Reapplies known assignments for active players in the scene.
    /// </summary>
    public static void ReapplySceneAssignments()
    {
        if (AssignedModelsByPlayer.Count == 0)
            return;

        foreach (KeyValuePair<string, string> pair in AssignedModelsByPlayer)
            RefreshModel(pair.Key);
    }

    /// <summary>
    /// Applies an assigned model to the identified player object if present.
    /// </summary>
    public static void RefreshModel(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
            return;

        if (!AssignedModelsByPlayer.TryGetValue(playerId, out string modelName))
            return;

        LoadedPlayerModel loadedModel = FFMModelRegistry.GetLoadedModel(modelName);
        if (loadedModel == null)
        {
            MelonLogger.Warning($"FFM.PlayerModels: assigned model '{modelName}' for '{playerId}' not loaded.");
            return;
        }

        GameObject target = FindPlayerObject(playerId);
        if (target == null)
        {
            MelonLogger.Warning($"FFM.PlayerModels: player '{playerId}' not found for model refresh.");
            return;
        }

        ApplyModel(target, loadedModel);
    }

    private static GameObject FindPlayerObject(string playerId)
    {
        GameObject[] candidates = GameObject.FindObjectsOfType<GameObject>();
        for (int index = 0; index < candidates.Length; index++)
        {
            GameObject candidate = candidates[index];
            if (candidate == null)
                continue;

            if (string.Equals(candidate.name, playerId, StringComparison.OrdinalIgnoreCase)
                || candidate.name.IndexOf(playerId, StringComparison.OrdinalIgnoreCase) >= 0)
                return candidate;
        }

        return null;
    }

    private static void ApplyModel(GameObject target, LoadedPlayerModel model)
    {
        if (target == null || model?.RootPrefab == null)
            return;

        SkinnedMeshRenderer sourceRenderer = model.RootPrefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
        SkinnedMeshRenderer targetRenderer = target.GetComponentInChildren<SkinnedMeshRenderer>(true);

        if (sourceRenderer == null || targetRenderer == null)
        {
            MelonLogger.Warning($"FFM.PlayerModels: missing renderer while applying '{model.ModelName}' to '{target.name}'.");
            return;
        }

        targetRenderer.sharedMesh = sourceRenderer.sharedMesh;
        targetRenderer.sharedMaterials = sourceRenderer.sharedMaterials;

        Transform[] targetBones = targetRenderer.bones;
        Transform[] sourceBones = sourceRenderer.bones;
        if (sourceBones != null && sourceBones.Length > 0 && targetBones != null && targetBones.Length == sourceBones.Length)
            targetRenderer.bones = targetBones;

        Animator targetAnimator = target.GetComponentInChildren<Animator>(true);
        if (targetAnimator != null)
            targetRenderer.rootBone = targetAnimator.transform;

        MelonLogger.Msg($"FFM.PlayerModels: applied '{model.ModelName}' to player '{target.name}'.");
    }
}
