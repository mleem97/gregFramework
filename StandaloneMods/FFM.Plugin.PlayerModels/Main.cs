using System;
using FrikaMF;
using FrikaMF.Hooks;
using FrikaMF.Plugins;
using MelonLoader;

[assembly: MelonInfo(typeof(FFM.Plugin.PlayerModels.Main), "FFM.Plugin.PlayerModels", ReleaseVersion.Current, "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FFM.Plugin.PlayerModels;

/// <summary>
/// Standalone plugin that exposes runtime player and NPC model replacement services.
/// </summary>
public sealed class Main : FFMPluginBase
{
    private static Main _instance;
    private bool _hookSampleRegistered;

    /// <summary>
    /// Gets the active plugin instance.
    /// </summary>
    public static Main Instance => _instance;

    /// <inheritdoc />
    public override string PluginId => "FFM.Plugin.PlayerModels";

    /// <inheritdoc />
    public override string DisplayName => "FrikaMF Player Models Plugin";

    /// <inheritdoc />
    public override Version RequiredFrameworkVersion => ParseFrameworkVersion(ReleaseVersion.Current);

    /// <inheritdoc />
    public override void OnInitializeMelon()
    {
        base.OnInitializeMelon();
        _instance = this;

        if (DataCenterModLoader.Core.Instance != null)
            OnFrameworkReady();
    }

    /// <inheritdoc />
    public override void OnFrameworkReady()
    {
        FFM.PlayerModels.API.Initialize();
        RegisterHookBinderSample();
        MelonLogger.Msg("FFM.Plugin.PlayerModels initialized.");
    }

    /// <inheritdoc />
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        PlayerModelSwapper.ReapplySceneAssignments();
        NPCModelReplacer.ReapplyPersistentReplacements();
    }

    private static Version ParseFrameworkVersion(string version)
    {
        return Version.TryParse(version, out Version parsed) ? parsed : new Version(0, 0, 0, 0);
    }

    private void RegisterHookBinderSample()
    {
        if (_hookSampleRegistered)
            return;

        _hookSampleRegistered = true;

        HookBinder.OnAfter("FFM.Server.OnPowerButton", OnServerPowerButtonHookAfter);
        MelonLogger.Msg("FFM.Plugin.PlayerModels: HookBinder sample registered for FFM.Server.OnPowerButton.");
    }

    private static void OnServerPowerButtonHookAfter(HookContext context)
    {
        if (context?.Method == null)
            return;

        MelonLogger.Msg($"[HookSample] {context.HookName} triggered by {context.Method.DeclaringType?.Name}.{context.Method.Name}");
    }
}
