using System;
using System.IO;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;

namespace FFM.Plugin.PlayerModels;

/// <summary>
/// Loads custom player model bundles from supported runtime paths.
/// </summary>
public static class ModelLoader
{
    private static readonly string StreamingModelsRoot = Path.Combine(
        MelonEnvironment.GameRootDirectory,
        "DataCenter_Data",
        "StreamingAssets",
        "FrikaMF",
        "PlayerModels");

    private static readonly string ModsModelsRoot = Path.Combine(
        MelonEnvironment.ModsDirectory,
        "FrikaMF",
        "PlayerModels");

    /// <summary>
    /// Loads a model from StreamingAssets, with required validation.
    /// </summary>
    public static LoadedPlayerModel LoadFromStreamingAssets(string modelName)
    {
        string bundlePath = Path.Combine(StreamingModelsRoot, modelName + ".bundle");
        return LoadFromPath(modelName, bundlePath, "StreamingAssets");
    }

    /// <summary>
    /// Loads a model from the mod-local fallback path.
    /// </summary>
    public static LoadedPlayerModel LoadFromModsPath(string modelName)
    {
        string bundlePath = Path.Combine(ModsModelsRoot, modelName + ".bundle");
        return LoadFromPath(modelName, bundlePath, "ModsPath");
    }

    /// <summary>
    /// Loads a model using the configured fallback order: StreamingAssets first, then Mods path.
    /// </summary>
    public static LoadedPlayerModel LoadModel(string modelName)
    {
        LoadedPlayerModel streamingModel = LoadFromStreamingAssets(modelName);
        if (streamingModel != null)
            return streamingModel;

        return LoadFromModsPath(modelName);
    }

    private static LoadedPlayerModel LoadFromPath(string modelName, string bundlePath, string source)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return null;

        if (!File.Exists(bundlePath))
        {
            MelonLogger.Warning($"FFM.PlayerModels: {source} bundle missing '{bundlePath}'");
            return null;
        }

        try
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                MelonLogger.Error($"FFM.PlayerModels: failed to load bundle '{bundlePath}'");
                return null;
            }

            GameObject rootPrefab = bundle.LoadAsset<GameObject>("PlayerModel_Root");
            if (rootPrefab == null)
            {
                MelonLogger.Error($"FFM.PlayerModels: bundle '{bundlePath}' missing required prefab 'PlayerModel_Root'.");
                bundle.Unload(unloadAllLoadedObjects: false);
                return null;
            }

            Animator animator = rootPrefab.GetComponent<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
            {
                MelonLogger.Error($"FFM.PlayerModels: prefab 'PlayerModel_Root' in '{bundlePath}' needs a humanoid Animator avatar.");
                bundle.Unload(unloadAllLoadedObjects: false);
                return null;
            }

            var loaded = new LoadedPlayerModel(modelName, bundlePath, bundle, rootPrefab);
            FFMModelRegistry.Register(loaded);
            return loaded;
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"FFM.PlayerModels: exception while loading '{bundlePath}': {exception.Message}");
            return null;
        }
    }
}
