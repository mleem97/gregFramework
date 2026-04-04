using System;
using DataCenterModLoader;
using FrikaMF;
using FrikaMF.Plugins;
using MelonLoader;

[assembly: MelonInfo(typeof(FFM.Plugin.WebUIBridge.Main), "FFM.Plugin.WebUIBridge", ReleaseVersion.Current, "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FFM.Plugin.WebUIBridge;

/// <summary>
/// Standalone plugin hosting the DC2 web-to-Unity UI bridge.
/// </summary>
public sealed class Main : FFMPluginBase
{
    private bool _registered;

    public override string PluginId => "FFM.Plugin.WebUIBridge";
    public override string DisplayName => "FrikaMF WebUI Bridge Plugin";
    public override Version RequiredFrameworkVersion => ParseFrameworkVersion(ReleaseVersion.Current);

    public override void OnInitializeMelon()
    {
        base.OnInitializeMelon();

        if (Core.Instance != null)
            OnFrameworkReady();
    }

    public override void OnFrameworkReady()
    {
        if (_registered)
            return;

        _registered = true;
        UiExtensionBridge.RegisterWebReplacement((root, screenKey) => DataCenterModLoader.DC2WebBridge.TryApplyOrReplace(root, screenKey));
        UiExtensionBridge.RegisterWebConfiguration(
            getEnabled: () => DataCenterModLoader.DC2WebBridge.Enabled,
            setEnabled: enabled => DataCenterModLoader.DC2WebBridge.Enabled = enabled,
            setProfileReplaceMode: (profileKey, replace) => DataCenterModLoader.DC2WebBridge.SetProfileReplaceMode(profileKey, replace),
            resetAppliedState: root => DataCenterModLoader.DC2WebBridge.ResetAppliedState(root));
        MelonLogger.Msg("FFM.Plugin.WebUIBridge registered web replacement handler.");
    }

    public override void OnApplicationQuit()
    {
        UiExtensionBridge.UnregisterWebReplacement();
        _registered = false;
    }

    private static Version ParseFrameworkVersion(string version)
    {
        return Version.TryParse(version, out Version parsed) ? parsed : new Version(0, 0, 0, 0);
    }
}
