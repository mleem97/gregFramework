using System;
using System.Linq;
using MelonLoader;

[assembly: MelonInfo(typeof(TemplateStandaloneMod.TemplateMod), "FMF Template Mod", "0.0.1", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace TemplateStandaloneMod;

public sealed class TemplateMod : MelonMod
{
    private bool _frameworkAvailable;

    public override void OnInitializeMelon()
    {
        _frameworkAvailable = AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
        {
            string name = assembly.GetName().Name ?? string.Empty;
            return string.Equals(name, "FrikaModdingFramework", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "FrikaMF", StringComparison.OrdinalIgnoreCase);
        });

        if (!_frameworkAvailable)
        {
            LoggerInstance.Error("FMF Template Mod requires FrikaModdingFramework.dll.");
            return;
        }

        LoggerInstance.Msg("FMF Template Mod initialized.");
    }
}
