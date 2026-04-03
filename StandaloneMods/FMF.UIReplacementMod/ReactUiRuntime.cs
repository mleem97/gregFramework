using System;
using System.IO;
using System.Reflection;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;

namespace FMF.UIReplacementMod;

internal sealed class ReactUiRuntime
{
    private const string AssetFolderName = "FMF.UIReplacementMod";
    private const string HtmlFileName = "react-app.html";
    private const string CssFileName = "react-app.css";
    private const string TsxFileName = "react-app.tsx";

    private readonly MelonLogger.Instance _logger;

    private Type _dc2WebBridgeType;
    private Type _descriptorType;
    private MethodInfo _registerWebAppMethod;
    private MethodInfo _tryApplyOrReplaceMethod;
    private MethodInfo _resetAppliedStateMethod;
    private FileSystemWatcher _watcher;
    private bool _reloadRequested;
    private string _activeAssetDirectory = string.Empty;

    private string _html = string.Empty;
    private string _css = string.Empty;
    private string _tsx = string.Empty;

    public bool IsReady => _dc2WebBridgeType != null
                           && _descriptorType != null
                           && _registerWebAppMethod != null
                           && _tryApplyOrReplaceMethod != null;

    public ReactUiRuntime(MelonLogger.Instance logger)
    {
        _logger = logger;
    }

    public string GetActiveAssetDirectory()
    {
        return _activeAssetDirectory;
    }

    public void LoadAssets()
    {
        string dir = ResolvePreferredAssetDirectory();
        Directory.CreateDirectory(dir);

        _activeAssetDirectory = dir;

        string htmlPath = Path.Combine(dir, HtmlFileName);
        string cssPath = Path.Combine(dir, CssFileName);
        string tsxPath = Path.Combine(dir, TsxFileName);

        EnsureDefaultFile(htmlPath, DefaultHtml());
        EnsureDefaultFile(cssPath, DefaultCss());
        EnsureDefaultFile(tsxPath, DefaultTsx());

        _html = SafeRead(htmlPath, DefaultHtml());
        _css = SafeRead(cssPath, DefaultCss());
        _tsx = SafeRead(tsxPath, DefaultTsx());

        EnsureWatcher(dir);
    }

    public bool ConsumeLiveReloadRequest()
    {
        if (!_reloadRequested)
            return false;

        _reloadRequested = false;
        return true;
    }

    public bool TryAttachBridge(bool logFailures)
    {
        var fmfAssembly = AppDomain.CurrentDomain
            .GetAssemblies();

        Assembly loadedAssembly = null;
        for (int index = 0; index < fmfAssembly.Length; index++)
        {
            if (string.Equals(fmfAssembly[index].GetName().Name, "FrikaModdingFramework", StringComparison.OrdinalIgnoreCase))
            {
                loadedAssembly = fmfAssembly[index];
                break;
            }
        }

        if (loadedAssembly == null)
        {
            if (logFailures)
                _logger.Warning("[FMF.UIReplacement] FrikaModdingFramework assembly not loaded yet.");
            return false;
        }

        _dc2WebBridgeType = loadedAssembly.GetType("DataCenterModLoader.DC2WebBridge", throwOnError: false);
        _descriptorType = loadedAssembly.GetType("DataCenterModLoader.Dc2WebAppDescriptor", throwOnError: false);

        if (_dc2WebBridgeType == null || _descriptorType == null)
        {
            if (logFailures)
                _logger.Error("[FMF.UIReplacement] DC2Web bridge types are unavailable in FMF.");
            return false;
        }

        _registerWebAppMethod = _dc2WebBridgeType.GetMethod("RegisterWebApp", BindingFlags.Public | BindingFlags.Static);
        _tryApplyOrReplaceMethod = _dc2WebBridgeType.GetMethod("TryApplyOrReplace", BindingFlags.Public | BindingFlags.Static);
        _resetAppliedStateMethod = _dc2WebBridgeType.GetMethod("ResetAppliedState", BindingFlags.Public | BindingFlags.Static);

        if (!IsReady)
        {
            if (logFailures)
                _logger.Error("[FMF.UIReplacement] Required DC2Web methods are missing.");
            return false;
        }

        _logger.Msg("[FMF.UIReplacement] FMF DC2Web bridge attached.");
        return true;
    }

    public void RegisterProfiles()
    {
        if (!IsReady)
            return;

        RegisterApp("MainMenuReact", true, "react-ts");
        RegisterApp("MainMenuSettings", true, "react-ts");
        RegisterApp("HRSystem", true, "react-ts");
        RegisterApp("ComputerShop", true, "react-ts");
        RegisterApp("GlobalCanvas", true, "react-ts");
    }

    public int ApplyToAllCanvases(bool forceReset)
    {
        if (!IsReady)
            return 0;

        int applied = 0;
        var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>(true);
        if (canvases == null)
            return 0;

        for (int index = 0; index < canvases.Count; index++)
        {
            var canvas = canvases[index];
            if (canvas == null || canvas.gameObject == null)
                continue;

            string screenKey = ResolveScreenKey(canvas.gameObject.name);

            if (forceReset && _resetAppliedStateMethod != null)
                _resetAppliedStateMethod.Invoke(null, new object[] { canvas.gameObject });

            bool ok = false;
            try
            {
                ok = (bool)_tryApplyOrReplaceMethod.Invoke(null, new object[] { canvas.gameObject, screenKey });
            }
            catch (Exception ex)
            {
                _logger.Warning($"[FMF.UIReplacement] Failed on '{canvas.gameObject.name}': {ex.GetBaseException().Message}");
            }

            if (ok)
                applied++;
        }

        return applied;
    }

    private void RegisterApp(string screenKey, bool replaceExistingUi, string framework)
    {
        object descriptor = Activator.CreateInstance(_descriptorType);
        SetField(descriptor, "ScreenKey", screenKey);
        SetField(descriptor, "ReplaceExistingUi", replaceExistingUi);
        SetField(descriptor, "Framework", framework);
        SetField(descriptor, "Html", _html);
        SetField(descriptor, "Css", _css);
        SetField(descriptor, "Script", _tsx);

        _registerWebAppMethod.Invoke(null, new[] { descriptor });
    }

    private void EnsureWatcher(string dir)
    {
        if (string.IsNullOrWhiteSpace(dir))
            return;

        if (_watcher != null)
        {
            if (string.Equals(_watcher.Path, dir, StringComparison.OrdinalIgnoreCase))
                return;

            DisposeWatcher();
        }

        _watcher = new FileSystemWatcher(dir)
        {
            Filter = "*.*",
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
        };

        _watcher.Changed += OnAssetFileChanged;
        _watcher.Created += OnAssetFileChanged;
        _watcher.Renamed += OnAssetFileChanged;
        _watcher.Deleted += OnAssetFileChanged;
    }

    private void OnAssetFileChanged(object sender, FileSystemEventArgs args)
    {
        string file = args?.FullPath ?? string.Empty;
        if (string.IsNullOrWhiteSpace(file))
            return;

        string name = Path.GetFileName(file);
        if (name == null)
            return;

        if (name.Equals(HtmlFileName, StringComparison.OrdinalIgnoreCase)
            || name.Equals(CssFileName, StringComparison.OrdinalIgnoreCase)
            || name.Equals(TsxFileName, StringComparison.OrdinalIgnoreCase))
        {
            _reloadRequested = true;
        }
    }

    private static string ResolvePreferredAssetDirectory()
    {
        string gameRoot = MelonEnvironment.GameRootDirectory;
        string candidatePrimary = Path.Combine(gameRoot, "UI", AssetFolderName);
        string candidateFramework = Path.Combine(gameRoot, "FrikaFramework", "UI", AssetFolderName);
        string candidateModsFallback = Path.Combine(MelonEnvironment.ModsDirectory, AssetFolderName);

        if (Directory.Exists(candidatePrimary) || HasReactAssetFile(candidatePrimary))
            return candidatePrimary;

        if (Directory.Exists(candidateFramework) || HasReactAssetFile(candidateFramework))
            return candidateFramework;

        return candidateModsFallback;
    }

    private static bool HasReactAssetFile(string dir)
    {
        return File.Exists(Path.Combine(dir, HtmlFileName))
               || File.Exists(Path.Combine(dir, CssFileName))
               || File.Exists(Path.Combine(dir, TsxFileName));
    }

    private void DisposeWatcher()
    {
        if (_watcher == null)
            return;

        _watcher.Changed -= OnAssetFileChanged;
        _watcher.Created -= OnAssetFileChanged;
        _watcher.Renamed -= OnAssetFileChanged;
        _watcher.Deleted -= OnAssetFileChanged;
        _watcher.Dispose();
        _watcher = null;
    }

    private static string ResolveScreenKey(string rootName)
    {
        string n = rootName?.ToLowerInvariant() ?? string.Empty;
        if (n.Contains("mainmenu")) return "MainMenuReact";
        if (n.Contains("hr")) return "HRSystem";
        if (n.Contains("shop") || n.Contains("computer")) return "ComputerShop";
        return "GlobalCanvas";
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (field != null)
            field.SetValue(target, value);
    }

    private static void EnsureDefaultFile(string path, string content)
    {
        if (File.Exists(path))
            return;

        File.WriteAllText(path, content);
    }

    private static string SafeRead(string path, string fallback)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch
        {
            return fallback;
        }
    }

    private static string DefaultHtml()
    {
        return "<div id='root'><h1>Frika Modern UI</h1><p>React-inspired animated replacement layer active.</p></div>";
    }

    private static string DefaultCss()
    {
        return @":root{--panel-color:#0b1220f2;--text-color:#f8fafc;--secondary-text-color:#cbd5e1;--btn-normal:#1d4ed8;--btn-highlight:#2563eb;--btn-pressed:#1e40af;--accent:#22d3ee;}\n"
            + ".app-shell{animation:fade-in .35s ease-out;}\n"
            + ".app-card{backdrop-filter:blur(8px);border:1px solid rgba(56,189,248,.35);box-shadow:0 10px 30px rgba(2,6,23,.5);animation:slide-up .3s ease-out;}\n"
            + ".btn-primary{transition:transform .12s ease,filter .12s ease;}\n"
            + ".btn-primary:hover{transform:translateY(-1px);filter:brightness(1.08);}\n"
            + "@keyframes fade-in{from{opacity:0}to{opacity:1}}\n"
            + "@keyframes slide-up{from{opacity:0;transform:translateY(8px)}to{opacity:1;transform:translateY(0)}}";
    }

    private static string DefaultTsx()
    {
        return "const App = () => (\n"
            + "  <div className='app-shell'>\n"
            + "    <div className='app-card'>\n"
            + "      <h1>Frika Modern React UI</h1>\n"
            + "      <p>All major screens are replaced through FMF DC2WebBridge descriptors.</p>\n"
            + "      <button className='btn-primary'>Open Dashboard</button>\n"
            + "    </div>\n"
            + "  </div>\n"
            + ");";
    }
}
