using System;
using FrikaMF;
using FrikaMF.Plugins;
using MelonLoader;

[assembly: MelonInfo(typeof(FFM.PluginTemplate.Main), "FFM.PluginTemplate", "00.01.0001", "your-name")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FFM.PluginTemplate;

public sealed class Main : FFMPluginBase
{
    private bool _registered;

    public override string PluginId => "FFM.Plugin.Template";

    public override string DisplayName => "FrikaMF Plugin Template";

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
        MelonLogger.Msg($"{PluginId} initialized and framework-ready.");
    }

    public override void OnApplicationQuit()
    {
        _registered = false;
    }

    private static Version ParseFrameworkVersion(string version)
    {
        return Version.TryParse(version, out Version parsed)
            ? parsed
            : new Version(0, 0, 0, 0);
    }
}
