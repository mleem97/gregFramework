using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(FMF.ConsoleInputGuard.ConsoleInputGuardMod), "FMF Console Input Guard", "00.01.0001", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FMF.ConsoleInputGuard;

public sealed class ConsoleInputGuardMod : MelonMod
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
            LoggerInstance.Error("FMF Console Input Guard requires FrikaModdingFramework.dll.");
            return;
        }

        HarmonyInstance.PatchAll(typeof(ConsoleInputGuardMod).Assembly);
        LoggerInstance.Msg("FMF Console Input Guard initialized. Console callback is blocked while typing into UI input fields.");
    }

    internal static bool IsTypingInInputField()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        var selected = eventSystem.currentSelectedGameObject;
        if (selected == null)
            return false;

        if (selected.GetComponent<InputField>() != null)
            return true;

        if (selected.GetComponent<TMP_InputField>() != null)
            return true;

        return false;
    }
}

[HarmonyPatch]
internal static class PauseMenuConsolePerformedPatch
{
    private static MethodBase TargetMethod()
    {
        var gameplayAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, "Assembly-CSharp", StringComparison.OrdinalIgnoreCase));
        if (gameplayAssembly == null)
            return null;

        var pauseMenuType = gameplayAssembly.GetType("Il2Cpp.PauseMenu", throwOnError: false);
        if (pauseMenuType == null)
            return null;

        return pauseMenuType.GetMethod("_Awake_b__30_2", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }

    private static bool Prefix()
    {
        return !ConsoleInputGuardMod.IsTypingInInputField();
    }
}
