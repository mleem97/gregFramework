using MelonLoader;
using MelonLoader.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using HarmonyLib;
using FrikaMF.Plugins;
using AssetExporter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(DataCenterModLoader.Core), "RustBridge", FrikaMF.ReleaseVersion.Current, "Joniii")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace DataCenterModLoader;

// file-based crash logger, never throws
public static class CrashLog
{
    private static string _frameworkDirectory;
    private static string _logPath;
    private static string _errorLogPath;
    private static readonly object _lock = new();

    public static void Init(string gameRoot)
    {
        try
        {
            _frameworkDirectory = Path.Combine(gameRoot, "FrikaFM");
            Directory.CreateDirectory(_frameworkDirectory);

            _logPath = Path.Combine(_frameworkDirectory, "frikafm-debug.log");
            _errorLogPath = Path.Combine(_frameworkDirectory, "frikafm-errors.log");

            var header =
                $"===== FrikaMF Debug Log ====={Environment.NewLine}" +
                $"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{Environment.NewLine}" +
                $"========================================={Environment.NewLine}";

            File.WriteAllText(_logPath, header);
            File.AppendAllText(_errorLogPath, header);
        }
        catch { }
    }

    public static void Log(string msg)
    {
        try
        {
            if (_logPath == null) return;
            lock (_lock)
            {
                File.AppendAllText(_logPath,
                    $"[{DateTime.Now:HH:mm:ss.fff}] {msg}{Environment.NewLine}");
            }
        }
        catch { }
    }

    public static void LogException(string context, Exception ex)
    {
        try
        {
            if (_logPath == null) return;
            lock (_lock)
            {
                string exceptionBlock =
                    $"[{DateTime.Now:HH:mm:ss.fff}] EXCEPTION in {context}:{Environment.NewLine}" +
                    $"  Type: {ex.GetType().FullName}{Environment.NewLine}" +
                    $"  Message: {ex.Message}{Environment.NewLine}" +
                    $"  StackTrace:{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}" +
                    (ex.InnerException != null
                        ? $"  InnerException: {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}{Environment.NewLine}" +
                          $"  InnerStackTrace:{Environment.NewLine}{ex.InnerException.StackTrace}{Environment.NewLine}"
                        : "") +
                    Environment.NewLine;

                File.AppendAllText(_logPath, exceptionBlock);

                if (!string.IsNullOrEmpty(_errorLogPath))
                    File.AppendAllText(_errorLogPath, exceptionBlock);
            }
        }
        catch { }
    }

    public static void LogError(string category, string message)
    {
        try
        {
            if (string.IsNullOrEmpty(_errorLogPath))
                return;

            lock (_lock)
            {
                string safeCategory = string.IsNullOrWhiteSpace(category) ? "general" : category.Trim();
                File.AppendAllText(_errorLogPath,
                    $"[{DateTime.Now:HH:mm:ss.fff}] ERROR [{safeCategory}] {message}{Environment.NewLine}");
            }
        }
        catch { }
    }

    public static void LogModError(string modId, string message, Exception ex = null)
    {
        string safeModId = string.IsNullOrWhiteSpace(modId) ? "unknown-mod" : modId.Trim();
        LogError($"mod:{safeModId}", message ?? "(no message)");

        if (ex != null)
            LogException($"mod:{safeModId}", ex);
    }
}

public class Core : MelonMod
{
    public static Core Instance { get; private set; }
    public static MultiplayerBridge Multiplayer { get; private set; }

    private FFIBridge _ffiBridge;
    private FfmLangserverCompatRuntime _langserverCompatRuntime;
    private string _modsPath;
    private bool _globalExceptionHooksInstalled;
    private float _nextHarmonyGuardAt;
    private float _nextHotReloadAt;

#if DEBUG
    private readonly Il2CppEventCatalogService _il2CppEventCatalog = new Il2CppEventCatalogService();
    private readonly Il2CppGameplayIndexService _il2CppGameplayIndex = new Il2CppGameplayIndexService();
    private readonly RuntimeHookService _runtimeHookService = new RuntimeHookService();
    private readonly GameSignalSnapshotService _gameSignalSnapshot = new GameSignalSnapshotService();
#endif

    public override void OnInitializeMelon()
    {
        try
        {
            Instance = this;

            CrashLog.Init(MelonEnvironment.GameRootDirectory);
            CrashLog.Log("step: CrashLog initialized");

            ModSaveCompatibilityService.Initialize();

            _modsPath = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "RustMods");

            LoggerInstance.Msg("╔══════════════════════════════════════════╗");
            LoggerInstance.Msg("║   Rust Bridge v0.1.0                     ║");
            LoggerInstance.Msg("║   Rust FFI Bridge Active                 ║");
            LoggerInstance.Msg("╚══════════════════════════════════════════╝");

            if (!Directory.Exists(_modsPath))
            {
                Directory.CreateDirectory(_modsPath);
                LoggerInstance.Msg($"Created Mods/RustMods directory: {_modsPath}");
            }

            CrashLog.Log("step: creating FFIBridge");
            _ffiBridge = new FFIBridge(LoggerInstance, _modsPath);

            CrashLog.Log("step: initializing FFM.Langserver.Compat runtime");
            _langserverCompatRuntime = FfmLangserverCompatRuntime.Initialize(LoggerInstance);

            InstallGlobalExceptionHooks();

            CrashLog.Log("step: initializing EventDispatcher");
            EventDispatcher.Initialize(_ffiBridge, LoggerInstance);

            CrashLog.Log("step: applying Harmony patches");
            try
            {
                int applied = ApplyHarmonyPatchesWithDiagnostics();
                LoggerInstance.Msg($"Harmony patches applied (classes): {applied}");
                CrashLog.Log($"step: Harmony patches applied successfully ({applied} classes)");
                _langserverCompatRuntime?.DetectHarmonyConflicts(HarmonyInstance?.Id ?? string.Empty);
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Failed to apply Harmony patches: {ex.Message}");
                LoggerInstance.Msg("Continuing without full event support.");
                CrashLog.LogException("Harmony patching", ex);
            }

            CrashLog.Log("step: loading all mods");
            _ffiBridge.LoadAllMods();

            // ModigApi is now fully integrated into FrikaMF.
            // All game API surfaces (Player, Network, Time, Localisation, UI, World)
            // are accessible via the consolidated ModigApi class.
            CrashLog.Log("step: ModigApi integrated");

            CrashLog.Log("step: notifying registered FFM plugins");
            FFMRegistry.NotifyFrameworkReady();

            LoggerInstance.Msg("Modloader initialization complete.");
            LoggerInstance.Msg("Hotkeys: Ctrl+Shift+R reload Rust mods (Main Menu only)");
            LoggerInstance.Msg("API: Access game systems via ModigApi (Player, Network, Time, Localisation, UI, World)");
            ModFramework.Events.Publish(new ModInitializedEvent(DateTime.UtcNow, FrikaMF.ReleaseVersion.Current));

#if DEBUG
            try
            {
                string diagnosticsDir = Path.Combine(MelonEnvironment.GameRootDirectory, "FrikaFM", "Diagnostics");
                string snapshotPath = _gameSignalSnapshot.ExportAll(
                    diagnosticsDir,
                    _il2CppEventCatalog,
                    _il2CppGameplayIndex,
                    _runtimeHookService);
                LoggerInstance.Msg($"FrikaMF: IL2CPP diagnostics snapshot written to {snapshotPath}");
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning($"FrikaMF: IL2CPP diagnostics snapshot failed: {ex.Message}");
                CrashLog.LogException("Il2CppDiagnostics", ex);
            }
#endif
            CrashLog.Log("step: OnInitializeMelon complete");
        }
        catch (Exception ex)
        {
            CrashLog.LogException("OnInitializeMelon", ex);
            CrashLog.LogError("core", "Initialization failed. See exception details above.");
            throw;
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        try
        {
            _ffiBridge?.OnSceneLoaded(sceneName);
            UiExtensionBridge.OnSceneLoaded(sceneName);

            // Initialize extra technician hiring (safe to call multiple times)
            TechnicianHiring.Initialize();

            // Re-register salaries for previously hired custom employees
            CustomEmployeeManager.ReregisterSalariesIfNeeded();

            // Export hires snapshot for framework clients/mod tooling
            HireRosterService.ExportAvailableHiresSnapshot();
        }
        catch (Exception ex)
        {
            CrashLog.LogException("OnSceneWasLoaded", ex);
        }
    }

    public override void OnUpdate()
    {
        try
        {
            _ffiBridge?.OnUpdate(Time.deltaTime);
            HandleMainMenuHotReload();

            if (Time.time >= _nextHarmonyGuardAt)
            {
                _nextHarmonyGuardAt = Time.time + 5f;
                bool priorityStable = _langserverCompatRuntime?.EnsureFrameworkPatchPresence(HarmonyInstance?.Id ?? string.Empty) ?? true;
                if (!priorityStable)
                {
                    CrashLog.Log("Harmony guard detected missing framework owners. Reapplying framework patches.");
                    ApplyHarmonyPatchesWithDiagnostics();
                    _langserverCompatRuntime?.DetectHarmonyConflicts(HarmonyInstance?.Id ?? string.Empty);
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException("OnUpdate", ex);
        }


    }

    private void HandleMainMenuHotReload()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        bool ctrlPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
        bool shiftPressed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        if (!ctrlPressed || !shiftPressed || !keyboard.rKey.wasPressedThisFrame)
            return;

        if (Time.unscaledTime < _nextHotReloadAt)
            return;

        _nextHotReloadAt = Time.unscaledTime + 0.75f;

        var activeScene = SceneManager.GetActiveScene();
        bool isMainMenu = string.Equals(activeScene.name, "MainMenu", StringComparison.OrdinalIgnoreCase);
        if (!isMainMenu)
        {
            LoggerInstance.Warning("Hotload is Main Menu only. Return to MainMenu and press Ctrl+Shift+R.");
            return;
        }

        try
        {
            int loaded = _ffiBridge?.ReloadAllMods() ?? 0;
            LoggerInstance.Msg($"Hotload complete. Loaded Rust mods: {loaded}");
        }
        catch (Exception ex)
        {
            LoggerInstance.Error($"Hotload failed: {ex.Message}");
            CrashLog.LogException("HandleMainMenuHotReload", ex);
        }
    }

    private int ApplyHarmonyPatchesWithDiagnostics()
    {
        int appliedClasses = 0;
        var assembly = typeof(Core).Assembly;
        var types = assembly.GetTypes();

        for (int index = 0; index < types.Length; index++)
        {
            var type = types[index];
            if (!Attribute.IsDefined(type, typeof(HarmonyPatch), inherit: true))
                continue;

            try
            {
                HarmonyInstance.CreateClassProcessor(type).Patch();
                appliedClasses++;
            }
            catch (Exception ex)
            {
                string typeName = type.FullName ?? type.Name;
                LoggerInstance.Error($"Harmony patch failed for '{typeName}': {ex.Message}");
                CrashLog.LogException($"Harmony patching '{typeName}'", ex);
            }
        }

        return appliedClasses;
    }

    public override void OnFixedUpdate()
    {
        try
        {
            _ffiBridge?.OnFixedUpdate(Time.fixedDeltaTime);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("OnFixedUpdate", ex);
        }
    }

    public override void OnGUI()
    {
        try
        {
            UiExtensionBridge.DrawGui();
        }
        catch (Exception ex)
        {
            CrashLog.LogException("OnGUI", ex);
        }
    }

    public override void OnApplicationQuit()
    {
        try
        {
            LoggerInstance.Msg("Shutting down modloader...");
            CrashLog.Log("step: OnApplicationQuit starting");
            UninstallGlobalExceptionHooks();
            _ffiBridge?.Shutdown();
            _ffiBridge?.Dispose();
            CrashLog.Log("step: OnApplicationQuit complete");
        }
        catch (Exception ex)
        {
            CrashLog.LogException("OnApplicationQuit", ex);
        }
    }

    private void InstallGlobalExceptionHooks()
    {
        if (_globalExceptionHooksInstalled)
            return;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        _globalExceptionHooksInstalled = true;
        CrashLog.Log("step: global exception hooks installed");
    }

    private void UninstallGlobalExceptionHooks()
    {
        if (!_globalExceptionHooksInstalled)
            return;

        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        _globalExceptionHooksInstalled = false;
        CrashLog.Log("step: global exception hooks removed");
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is Exception ex)
        {
            CrashLog.LogException("AppDomain.CurrentDomain.UnhandledException", ex);
            CrashLog.LogError("runtime", "Unhandled exception captured from AppDomain.");
            return;
        }

        CrashLog.LogError("runtime", "Unhandled non-Exception object captured from AppDomain.");
    }

    private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
    {
        CrashLog.LogException("TaskScheduler.UnobservedTaskException", args.Exception);
        CrashLog.LogError("runtime", "Unobserved task exception captured.");
        args.SetObserved();
    }

    public static void RegisterMultiplayerBridge(MultiplayerBridge bridge)
    {
        Multiplayer = bridge;
    }

    public static void UnregisterMultiplayerBridge(MultiplayerBridge bridge)
    {
        if (ReferenceEquals(Multiplayer, bridge))
            Multiplayer = null;
    }
}
