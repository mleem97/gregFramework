using MelonLoader;
using MelonLoader.Utils;
using System;
using System.IO;
using HarmonyLib;
using UnityEngine;

[assembly: MelonInfo(typeof(DataCenterModLoader.Core), "DataCenterModLoader", DataCenterModLoader.ReleaseVersion.Current, "DataCenterModding")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace DataCenterModLoader;

// file-based crash logger, never throws
public static class CrashLog
{
    private static string _logPath;
    private static readonly object _lock = new();

    public static void Init(string gameRoot)
    {
        try
        {
            _logPath = Path.Combine(gameRoot, "dc_modloader_debug.log");
            var header =
                $"===== DataCenterModLoader Debug Log ====={Environment.NewLine}" +
                $"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{Environment.NewLine}" +
                $"========================================={Environment.NewLine}";
            File.WriteAllText(_logPath, header);
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
                File.AppendAllText(_logPath,
                    $"[{DateTime.Now:HH:mm:ss.fff}] EXCEPTION in {context}:{Environment.NewLine}" +
                    $"  Type: {ex.GetType().FullName}{Environment.NewLine}" +
                    $"  Message: {ex.Message}{Environment.NewLine}" +
                    $"  StackTrace:{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}" +
                    (ex.InnerException != null
                        ? $"  InnerException: {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}{Environment.NewLine}" +
                          $"  InnerStackTrace:{Environment.NewLine}{ex.InnerException.StackTrace}{Environment.NewLine}"
                        : "") +
                    Environment.NewLine);
            }
        }
        catch { }
    }
}

public class Core : MelonMod
{
    public static Core Instance { get; private set; }

    private FFIBridge _ffiBridge;
    private string _modsPath;
    private string _streamingModsPath;

    public override void OnInitializeMelon()
    {
        try
        {
            Instance = this;

            CrashLog.Init(MelonEnvironment.GameRootDirectory);
            CrashLog.Log("step: CrashLog initialized");

            _modsPath = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "RustMods");
            _streamingModsPath = Path.Combine(MelonEnvironment.GameRootDirectory, "Data Center_Data", "StreamingAssets", "Mods");

            LoggerInstance.Msg("╔══════════════════════════════════════════╗");
            LoggerInstance.Msg($"║   Data Center Modloader v{ReleaseVersion.Current}    ║");
            LoggerInstance.Msg("║   Rust FFI Bridge Active                ║");
            LoggerInstance.Msg("╚══════════════════════════════════════════╝");

            if (!Directory.Exists(_modsPath))
            {
                Directory.CreateDirectory(_modsPath);
                LoggerInstance.Msg($"Created Mods/RustMods directory: {_modsPath}");
            }

            if (!Directory.Exists(_streamingModsPath))
            {
                Directory.CreateDirectory(_streamingModsPath);
                LoggerInstance.Msg($"Created StreamingAssets/Mods directory: {_streamingModsPath}");
            }
            else
            {
                LoggerInstance.Msg($"Using StreamingAssets/Mods directory: {_streamingModsPath}");
            }

            CrashLog.Log("step: creating FFIBridge");
            _ffiBridge = new FFIBridge(LoggerInstance, _modsPath);

            CrashLog.Log("step: initializing EventDispatcher");
            EventDispatcher.Initialize(_ffiBridge, LoggerInstance);

            CrashLog.Log("step: applying Harmony patches");
            try
            {
                HarmonyInstance.PatchAll(typeof(Core).Assembly);
                LoggerInstance.Msg("Harmony patches applied.");
                CrashLog.Log("step: Harmony patches applied successfully");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Failed to apply Harmony patches: {ex.Message}");
                LoggerInstance.Msg("Continuing without full event support.");
                CrashLog.LogException("Harmony patching", ex);
            }

            RunHookerCommandIfRequested();

            CrashLog.Log("step: loading all mods");
            _ffiBridge.LoadAllMods();
            LoggerInstance.Msg("Modloader initialization complete.");
            CrashLog.Log("step: OnInitializeMelon complete");
        }
        catch (Exception ex)
        {
            CrashLog.LogException("OnInitializeMelon", ex);
            throw;
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        try
        {
            _ffiBridge?.OnSceneLoaded(sceneName);
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
        }
        catch (Exception ex)
        {
            CrashLog.LogException("OnUpdate", ex);
        }


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

    public override void OnApplicationQuit()
    {
        try
        {
            LoggerInstance.Msg("Shutting down modloader...");
            CrashLog.Log("step: OnApplicationQuit starting");
            _ffiBridge?.Shutdown();
            _ffiBridge?.Dispose();
            CrashLog.Log("step: OnApplicationQuit complete");
        }
        catch (Exception ex)
        {
            CrashLog.LogException("OnApplicationQuit", ex);
        }
    }

    private void RunHookerCommandIfRequested()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            bool autoScan = HasArg(args, "--hooker-auto");
            bool installAll = HasArg(args, "--hooker-all");
            string catalogPath = GetArgValue(args, "--hooker-catalog=");

            if (!autoScan && string.IsNullOrWhiteSpace(catalogPath))
                return;

            int defaultMax = installAll ? int.MaxValue : 500;
            int maxHooks = GetIntArgValue(args, "--hooker-max=", defaultMax);
            HookerInstallResult result;

            if (!string.IsNullOrWhiteSpace(catalogPath))
            {
                LoggerInstance.Msg($"Hooker command: install from catalog ({catalogPath}), max={maxHooks}");
                result = Hooker.InstallFromCatalog(catalogPath, maxHooks);
            }
            else
            {
                LoggerInstance.Msg($"Hooker command: scan install, max={maxHooks}");
                result = Hooker.InstallByScan(maxHooks);
            }

            LoggerInstance.Msg($"Hooker result: scanned={result.Scanned}, installed={result.Installed}, failed={result.Failed}");

            if (result.Errors.Count > 0)
            {
                string diagnosticsPath = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "ExportedAssets", "Diagnostics");
                Directory.CreateDirectory(diagnosticsPath);
                string errorFile = Path.Combine(diagnosticsPath, "hooker-install-errors.txt");
                File.WriteAllLines(errorFile, result.Errors);
                LoggerInstance.Warning($"Hooker error log written: {errorFile}");
            }
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"Hooker command failed: {ex.Message}");
            CrashLog.LogException("RunHookerCommandIfRequested", ex);
        }
    }

    private static bool HasArg(string[] args, string value)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], value, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string GetArgValue(string[] args, string prefix)
    {
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (!arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            return arg.Substring(prefix.Length).Trim('"');
        }

        return string.Empty;
    }

    private static int GetIntArgValue(string[] args, string prefix, int fallback)
    {
        string raw = GetArgValue(args, prefix);
        if (string.IsNullOrWhiteSpace(raw))
            return fallback;

        return int.TryParse(raw, out int value) && value > 0 ? value : fallback;
    }
}