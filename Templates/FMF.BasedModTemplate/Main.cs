using System;
using FrikaMF;
using MelonLoader;

[assembly: MelonInfo(typeof(FMF.BasedModTemplate.Main), "FMF.BasedModTemplate", "00.01.0001", "your-name")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FMF.BasedModTemplate;

public sealed class Main : MelonMod
{
    private bool _frameworkReady;

    public override void OnInitializeMelon()
    {
        if (Core.Instance == null)
        {
            LoggerInstance.Warning("FrikaModdingFramework core not available yet. Waiting for game update loop.");
            return;
        }

        _frameworkReady = true;
        LoggerInstance.Msg("FMF.BasedModTemplate initialized.");
    }

    public override void OnUpdate()
    {
        if (_frameworkReady)
            return;

        if (Core.Instance == null)
            return;

        _frameworkReady = true;
        LoggerInstance.Msg("FMF core detected. Mod is now active.");
    }

    public override void OnApplicationQuit()
    {
        _frameworkReady = false;
    }
}
