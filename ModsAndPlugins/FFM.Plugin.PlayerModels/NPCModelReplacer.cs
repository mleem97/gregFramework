using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace FFM.Plugin.PlayerModels;

/// <summary>
/// Applies model replacements to NPC objects while preserving AI and collider components.
/// </summary>
public static class NPCModelReplacer
{
    private static readonly Dictionary<string, string> PersistentNpcModels = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Replaces an NPC visual model by identifier.
    /// </summary>
    public static void ReplaceNPCModel(string npcId, string modelName, bool persistent = true)
    {
        if (string.IsNullOrWhiteSpace(npcId) || string.IsNullOrWhiteSpace(modelName))
            return;

        if (persistent)
            PersistentNpcModels[npcId] = modelName;

        ApplyReplacement(npcId, modelName);
    }

    /// <summary>
    /// Reverts a persistent NPC replacement mapping.
    /// </summary>
    public static void RevertNPC(string npcId)
    {
        if (string.IsNullOrWhiteSpace(npcId))
            return;

        PersistentNpcModels.Remove(npcId);
    }

    /// <summary>
    /// Reapplies persistent NPC mappings after scene changes.
    /// </summary>
    public static void ReapplyPersistentReplacements()
    {
        foreach (KeyValuePair<string, string> pair in PersistentNpcModels)
            ApplyReplacement(pair.Key, pair.Value);
    }

    private static void ApplyReplacement(string npcId, string modelName)
    {
        LoadedPlayerModel model = FFMModelRegistry.GetLoadedModel(modelName);
        if (model == null)
            return;

        GameObject npc = FindNpcObject(npcId);
        if (npc == null)
            return;

        SkinnedMeshRenderer sourceRenderer = model.RootPrefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
        SkinnedMeshRenderer targetRenderer = npc.GetComponentInChildren<SkinnedMeshRenderer>(true);

        if (sourceRenderer == null || targetRenderer == null)
            return;

        targetRenderer.sharedMesh = sourceRenderer.sharedMesh;
        targetRenderer.sharedMaterials = sourceRenderer.sharedMaterials;

        Animator npcAnimator = npc.GetComponentInChildren<Animator>(true);
        Animator modelAnimator = model.RootPrefab.GetComponentInChildren<Animator>(true);
        if (npcAnimator != null && modelAnimator != null && modelAnimator.avatar != null && !modelAnimator.avatar.isHuman)
            MelonLogger.Warning($"FFM.PlayerModels: NPC '{npcId}' model '{modelName}' has non-humanoid avatar; compatibility may be limited.");

        MelonLogger.Msg($"FFM.PlayerModels: applied '{modelName}' to NPC '{npc.name}'.");
    }

    private static GameObject FindNpcObject(string npcId)
    {
        GameObject[] candidates = GameObject.FindObjectsOfType<GameObject>();
        for (int index = 0; index < candidates.Length; index++)
        {
            GameObject candidate = candidates[index];
            if (candidate == null)
                continue;

            if (string.Equals(candidate.name, npcId, StringComparison.OrdinalIgnoreCase)
                || candidate.name.IndexOf(npcId, StringComparison.OrdinalIgnoreCase) >= 0)
                return candidate;
        }

        return null;
    }
}
