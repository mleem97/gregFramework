using System;
using MelonLoader;

namespace FrikaMF.Plugins;

/// <summary>
/// Base class for FrikaModFramework standalone plugins.
/// </summary>
public abstract class FFMPluginBase : MelonMod
{
    /// <summary>
    /// Gets the plugin's unique identifier.
    /// </summary>
    public abstract string PluginId { get; }

    /// <summary>
    /// Gets the plugin's human-readable display name.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Gets the minimum required framework version.
    /// </summary>
    public abstract Version RequiredFrameworkVersion { get; }

    /// <summary>
    /// Called after the framework core has fully initialized and validated plugin compatibility.
    /// </summary>
    public abstract void OnFrameworkReady();

    /// <summary>
    /// Registers the plugin with the central framework registry.
    /// </summary>
    public override void OnInitializeMelon()
    {
        FFMRegistry.RegisterPlugin(this);
    }
}
