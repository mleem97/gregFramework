using MelonLoader;
using MelonLoader.Utils;
using System;
using System.IO;
using HarmonyLib;
using UnityEngine;

[assembly: MelonInfo(typeof(FrikaMF.Core), "FrikadelleModdingFramework", FrikaMF.ReleaseVersion.Current, "DataCenterModding")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FrikaMF;

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
                $"===== FrikaMF Debug Log ====={Environment.NewLine}" +
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
    private string _legacyNativeModsPath;
    private string _streamingModsPath;
    private string _frameworkDiagnosticsPath;

    public override void OnInitializeMelon()
    {
        try
        {
            Instance = this;

            CrashLog.Init(MelonEnvironment.GameRootDirectory);
            FrameworkLog.Initialize(LoggerInstance);
            FrameworkLog.Info("core", $"Booting FrikadelleModdingFramework v{ReleaseVersion.Current}");
            FrameworkLog.Info("core", "Runtime bridge: Rust FFI enabled");

            _modsPath = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "RustMods");
            _legacyNativeModsPath = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "native");
            _streamingModsPath = Path.Combine(MelonEnvironment.GameRootDirectory, "Data Center_Data", "StreamingAssets", "Mods");
            _frameworkDiagnosticsPath = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "FrikaMF", "Diagnostics");

            if (!Directory.Exists(_modsPath))
            {
                Directory.CreateDirectory(_modsPath);
                FrameworkLog.Info("paths", $"Created Rust mod directory: {_modsPath}");
            }

            if (Directory.Exists(_legacyNativeModsPath))
            {
                FrameworkLog.Warn("paths", $"Legacy native path detected: {_legacyNativeModsPath}");
                FrameworkLog.Warn("paths", "Please migrate Rust mods to Mods/RustMods (auto fallback is enabled)");

                bool preferredHasDll = Directory.GetFiles(_modsPath, "*.dll", SearchOption.AllDirectories).Length > 0;
                bool legacyHasDll = Directory.GetFiles(_legacyNativeModsPath, "*.dll", SearchOption.AllDirectories).Length > 0;
                if (!preferredHasDll && legacyHasDll)
                {
                    _modsPath = _legacyNativeModsPath;
                    FrameworkLog.Warn("paths", "Using legacy native path because Mods/RustMods has no DLLs");
                }
            }
            else
            {
                FrameworkLog.Debug("paths", $"Using Rust mod directory: {_modsPath}");
            }

            if (!Directory.Exists(_streamingModsPath))
            {
                Directory.CreateDirectory(_streamingModsPath);
                FrameworkLog.Info("paths", $"Created StreamingAssets mod directory: {_streamingModsPath}");
            }
            else
            {
                FrameworkLog.Debug("paths", $"Using StreamingAssets mod directory: {_streamingModsPath}");
            }

            FrameworkLog.Debug("core", "Creating FFI bridge instance");
            _ffiBridge = new FFIBridge(LoggerInstance, _modsPath);

            FrameworkLog.Debug("core", "Initializing event dispatcher");
            EventDispatcher.Initialize(_ffiBridge, LoggerInstance);

            ExportRuntimeAssemblyHooks();

            FrameworkLog.Debug("harmony", "Applying assembly patches");
            try
            {
                HarmonyInstance.PatchAll(typeof(Core).Assembly);
                FrameworkLog.Info("harmony", "Harmony patches applied successfully");
            }
            catch (Exception ex)
            {
                FrameworkLog.Exception("harmony", "Failed to apply Harmony patches", ex);
                FrameworkLog.Warn("harmony", "Continuing without full event support");
            }

            RunHookerCommandIfRequested();

            FrameworkLog.Debug("ffi", "Scanning and loading native Rust mods");
            _ffiBridge.LoadAllMods();
            FrameworkLog.Info("core", "Initialization complete");
        }
        catch (Exception ex)
        {
            FrameworkLog.Exception("core", "OnInitializeMelon failed", ex);
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
            FrameworkLog.Exception("scene", "OnSceneWasLoaded failed", ex);
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
            FrameworkLog.Exception("loop", "OnUpdate failed", ex);
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
            FrameworkLog.Exception("loop", "OnFixedUpdate failed", ex);
        }
    }

    public override void OnApplicationQuit()
    {
        try
        {
            FrameworkLog.Info("core", "Shutdown started");
            _ffiBridge?.Shutdown();
            _ffiBridge?.Dispose();
            FrameworkLog.Info("core", "Shutdown complete");
        }
        catch (Exception ex)
        {
            FrameworkLog.Exception("core", "OnApplicationQuit failed", ex);
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
                FrameworkLog.Info("hooker", $"Command detected: install from catalog '{catalogPath}', max={maxHooks}");
                result = Hooker.InstallFromCatalog(catalogPath, maxHooks);
            }
            else
            {
                FrameworkLog.Info("hooker", $"Command detected: scan install, max={maxHooks}");
                result = Hooker.InstallByScan(maxHooks);
            }

            FrameworkLog.Info("hooker", $"Result: scanned={result.Scanned}, installed={result.Installed}, failed={result.Failed}");

            if (result.Errors.Count > 0)
            {
                string diagnosticsPath = Path.Combine(MelonEnvironment.GameRootDirectory, "Mods", "ExportedAssets", "Diagnostics");
                Directory.CreateDirectory(diagnosticsPath);
                string errorFile = Path.Combine(diagnosticsPath, "hooker-install-errors.txt");
                File.WriteAllLines(errorFile, result.Errors);
                FrameworkLog.Warn("hooker", $"Error log written: {errorFile}");
            }
        }
        catch (Exception ex)
        {
            FrameworkLog.Exception("hooker", "Hooker command failed", ex);
        }
    }

    private void ExportRuntimeAssemblyHooks()
    {
        try
        {
            Directory.CreateDirectory(_frameworkDiagnosticsPath);
            string outputFile = Path.Combine(_frameworkDiagnosticsPath, "assembly-hooks.txt");
            AssemblyHookDumpResult dump = AssemblyHookDumpService.ExportAssemblyCSharpDump(outputFile);

            if (!dump.Success)
            {
                FrameworkLog.Warn("hooks", "Assembly-CSharp not yet available; wrote empty runtime dump file");
                return;
            }

            FrameworkLog.Info("hooks", $"Runtime hook dump exported: {dump.OutputPath} (types={dump.TypeCount}, methods={dump.MethodCount})");
        }
        catch (Exception ex)
        {
            FrameworkLog.Exception("hooks", "Failed to export assembly-hooks runtime dump", ex);
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