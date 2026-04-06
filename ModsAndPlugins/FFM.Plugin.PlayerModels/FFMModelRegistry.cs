using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;

namespace FFM.Plugin.PlayerModels;

/// <summary>
/// Metadata container for a loaded player or NPC model.
/// </summary>
public sealed class ModelMetadata
{
    /// <summary>
    /// Initializes model metadata.
    /// </summary>
    public ModelMetadata(string name, string bundlePath, bool isHumanoid, bool hasVoiceAttachment, DateTime loadedAt)
    {
        Name = name;
        BundlePath = bundlePath;
        IsHumanoid = isHumanoid;
        HasVoiceAttachment = hasVoiceAttachment;
        LoadedAt = loadedAt;
    }

    /// <summary>
    /// Registered model name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Source bundle path.
    /// </summary>
    public string BundlePath { get; }

    /// <summary>
    /// Indicates whether the model uses a humanoid avatar.
    /// </summary>
    public bool IsHumanoid { get; }

    /// <summary>
    /// Indicates whether the model exposes a voice attachment node.
    /// </summary>
    public bool HasVoiceAttachment { get; }

    /// <summary>
    /// UTC timestamp when the model was loaded.
    /// </summary>
    public DateTime LoadedAt { get; }
}

/// <summary>
/// Runtime container for a loaded model prefab and metadata.
/// </summary>
public sealed class LoadedPlayerModel
{
    /// <summary>
    /// Initializes a loaded model container.
    /// </summary>
    public LoadedPlayerModel(string modelName, string bundlePath, UnityEngine.AssetBundle bundle, UnityEngine.GameObject rootPrefab)
    {
        ModelName = modelName;
        BundlePath = bundlePath;
        Bundle = bundle;
        RootPrefab = rootPrefab;

        UnityEngine.Animator animator = rootPrefab.GetComponent<UnityEngine.Animator>();
        UnityEngine.Transform voice = rootPrefab.transform.Find("Voice");

        Metadata = new ModelMetadata(
            modelName,
            bundlePath,
            animator != null && animator.avatar != null && animator.avatar.isHuman,
            voice != null,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Registered model name.
    /// </summary>
    public string ModelName { get; }

    /// <summary>
    /// Source bundle path.
    /// </summary>
    public string BundlePath { get; }

    /// <summary>
    /// Loaded Unity asset bundle instance.
    /// </summary>
    public UnityEngine.AssetBundle Bundle { get; }

    /// <summary>
    /// Root prefab named <c>PlayerModel_Root</c>.
    /// </summary>
    public UnityEngine.GameObject RootPrefab { get; }

    /// <summary>
    /// Extracted model metadata.
    /// </summary>
    public ModelMetadata Metadata { get; }
}

/// <summary>
/// Central registry of all loaded player and NPC models.
/// </summary>
public static class FFMModelRegistry
{
    private static readonly Dictionary<string, LoadedPlayerModel> Models = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object SyncRoot = new();

    /// <summary>
    /// Fired whenever a model is successfully registered.
    /// </summary>
    public static event Action<string, ModelMetadata> OnModelRegistered;

    /// <summary>
    /// Returns all registered model names.
    /// </summary>
    public static IEnumerable<string> RegisteredModels
    {
        get
        {
            lock (SyncRoot)
                return Models.Keys.ToArray();
        }
    }

    /// <summary>
    /// Registers a loaded model in the central registry.
    /// </summary>
    /// <returns><c>true</c> when registration succeeds, otherwise <c>false</c>.</returns>
    public static bool Register(LoadedPlayerModel model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.ModelName))
            return false;

        lock (SyncRoot)
        {
            Models[model.ModelName] = model;
        }

        MelonLogger.Msg($"FFM.PlayerModels: registered model '{model.ModelName}' from '{model.BundlePath}'");
        OnModelRegistered?.Invoke(model.ModelName, model.Metadata);
        return true;
    }

    /// <summary>
    /// Gets loaded model metadata by model name.
    /// </summary>
    public static ModelMetadata GetMetadata(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return null;

        lock (SyncRoot)
        {
            return Models.TryGetValue(modelName, out LoadedPlayerModel model) ? model.Metadata : null;
        }
    }

    /// <summary>
    /// Gets the loaded model container by model name.
    /// </summary>
    public static LoadedPlayerModel GetLoadedModel(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return null;

        lock (SyncRoot)
        {
            return Models.TryGetValue(modelName, out LoadedPlayerModel model) ? model : null;
        }
    }
}
