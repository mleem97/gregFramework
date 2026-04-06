using System;
using DataCenterModLoader;
using FrikaMF;
using FrikaMF.Plugins;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(FFM.Plugin.Sysadmin.Main), "FFM.Plugin.Sysadmin", ReleaseVersion.Current, "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FFM.Plugin.Sysadmin;

/// <summary>
/// Standalone plugin hosting optional UI modernization and settings bridge features.
/// </summary>
public sealed class Main : FFMPluginBase
{
    private bool _registered;

    /// <inheritdoc />
    public override string PluginId => "FFM.Plugin.Sysadmin";

    /// <inheritdoc />
    public override string DisplayName => "FrikaMF Sysadmin Plugin";

    /// <inheritdoc />
    public override Version RequiredFrameworkVersion => ParseFrameworkVersion(ReleaseVersion.Current);

    /// <inheritdoc />
    public override void OnInitializeMelon()
    {
        base.OnInitializeMelon();

        if (Core.Instance != null)
            OnFrameworkReady();
    }

    /// <inheritdoc />
    public override void OnFrameworkReady()
    {
        if (_registered)
            return;

        _registered = true;

        UiExtensionBridge.RegisterUiHandlers(
            tryModernize: (root, sourceTag) => DataCenterModLoader.UiModernizer.TryModernize(root, sourceTag),
            onSceneLoaded: sceneName => DataCenterModLoader.ModSettingsMenuBridge.OnSceneLoaded(sceneName),
            drawGui: () => DataCenterModLoader.ModSettingsMenuBridge.DrawGUI(),
            onSettingsOpened: mainMenu => DataCenterModLoader.ModSettingsMenuBridge.OnSettingsOpened(mainMenu));

        MelonLogger.Msg("FFM.Plugin.Sysadmin registered UI extension handlers.");
    }

    /// <inheritdoc />
    public override void OnApplicationQuit()
    {
        UiExtensionBridge.UnregisterUiHandlers();
        _registered = false;
    }

    private static Version ParseFrameworkVersion(string version)
    {
        return Version.TryParse(version, out Version parsed) ? parsed : new Version(0, 0, 0, 0);
    }
}
