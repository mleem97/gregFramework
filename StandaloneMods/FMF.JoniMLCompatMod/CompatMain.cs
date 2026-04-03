using System;
using System.Linq;
using MelonLoader;

[assembly: MelonInfo(typeof(FMF.JoniMLCompatMod.CompatMain), "FMF JoniML Compat Mod", "00.01.0009", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FMF.JoniMLCompatMod;

public sealed class CompatMain : MelonMod
{
    private bool _frameworkAvailable;

    public override void OnInitializeMelon()
    {
        _frameworkAvailable = IsFrameworkLoaded();
        if (!_frameworkAvailable)
        {
            LoggerInstance.Error("FMF JoniML Compat Mod requires `FrikaModdingFramework.dll`.");
            return;
        }

        LoggerInstance.Msg("FMF JoniML Compat Mod initialized.");
        LoggerInstance.Msg("Legacy root JoniML code has been migrated into StandaloneMods and replaced by framework-compatible compatibility behavior.");
    }

    private static bool IsFrameworkLoaded()
    {
        return AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
        {
            string name = assembly.GetName().Name ?? string.Empty;
            return string.Equals(name, "FrikaModdingFramework", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "FrikaMF", StringComparison.OrdinalIgnoreCase);
        });
    }
}
