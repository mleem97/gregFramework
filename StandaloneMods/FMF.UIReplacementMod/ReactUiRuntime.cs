using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
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
    private const string ScreenRegistryFileName = "react-screens.json";

    private readonly MelonLogger.Instance _logger;

    private Type _dc2WebBridgeType;
    private Type _descriptorType;
    private MethodInfo _registerWebAppMethod;
    private MethodInfo _tryApplyOrReplaceMethod;
    private MethodInfo _resetAppliedStateMethod;
    private Assembly _attachedAssembly;
    private bool _bridgeAttachedLogged;
    private FileSystemWatcher _watcher;
    private bool _reloadRequested;
    private string _activeAssetDirectory = string.Empty;

    private string _html = string.Empty;
    private string _css = string.Empty;
    private string _tsx = string.Empty;
    private List<ScreenRegistration> _screenRegistrations = BuildDefaultScreenRegistrations();

    private sealed class ScreenRegistration
    {
        public string ScreenKey { get; set; } = string.Empty;
        public bool ReplaceExistingUi { get; set; } = true;
        public string Framework { get; set; } = "react-ts";
    }

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
        _screenRegistrations = LoadScreenRegistrations(dir);

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
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        Assembly loadedAssembly = null;
        for (int index = 0; index < assemblies.Length; index++)
        {
            if (assemblies[index].GetType("DataCenterModLoader.DC2WebBridge", throwOnError: false) != null)
            {
                loadedAssembly = assemblies[index];
                break;
            }
        }

        if (loadedAssembly == null)
        {
            if (logFailures)
                _logger.Warning("[FMF.UIReplacement] FrikaMF runtime bridge is not available yet.");
            return false;
        }

        if (IsReady && ReferenceEquals(_attachedAssembly, loadedAssembly))
            return true;

        _dc2WebBridgeType = loadedAssembly.GetType("DataCenterModLoader.DC2WebBridge", throwOnError: false);
        _descriptorType = loadedAssembly.GetType("DataCenterModLoader.Dc2WebAppDescriptor", throwOnError: false);

        if (_dc2WebBridgeType == null || _descriptorType == null)
        {
            if (logFailures)
                _logger.Error("[FMF.UIReplacement] Required FrikaMF bridge types are unavailable.");
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

        bool shouldLogAttach = !ReferenceEquals(_attachedAssembly, loadedAssembly) || !_bridgeAttachedLogged;
        _attachedAssembly = loadedAssembly;

        if (shouldLogAttach)
        {
            _logger.Msg("[FMF.UIReplacement] FMF DC2Web bridge attached.");
            _bridgeAttachedLogged = true;
        }

        return true;
    }

    public void RegisterProfiles()
    {
        if (!IsReady)
            return;

        for (int index = 0; index < _screenRegistrations.Count; index++)
        {
            var registration = _screenRegistrations[index];
            if (registration == null || string.IsNullOrWhiteSpace(registration.ScreenKey))
                continue;

            RegisterApp(
                registration.ScreenKey,
                registration.ReplaceExistingUi,
                string.IsNullOrWhiteSpace(registration.Framework) ? "react-ts" : registration.Framework);
        }
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

    private static List<ScreenRegistration> BuildDefaultScreenRegistrations()
    {
        return new List<ScreenRegistration>
        {
            new() { ScreenKey = "MainMenuReact", ReplaceExistingUi = true, Framework = "react-ts" },
            new() { ScreenKey = "MainMenuSettings", ReplaceExistingUi = true, Framework = "react-ts" },
            new() { ScreenKey = "HRSystem", ReplaceExistingUi = true, Framework = "react-ts" },
            new() { ScreenKey = "ComputerShop", ReplaceExistingUi = true, Framework = "react-ts" },
            new() { ScreenKey = "GlobalCanvas", ReplaceExistingUi = true, Framework = "react-ts" },
        };
    }

    private List<ScreenRegistration> LoadScreenRegistrations(string assetDirectory)
    {
        var defaults = BuildDefaultScreenRegistrations();
        string registryPath = Path.Combine(assetDirectory, ScreenRegistryFileName);

        if (!File.Exists(registryPath))
        {
            TryWriteDefaultScreenRegistry(registryPath, defaults);
            return defaults;
        }

        try
        {
            string raw = File.ReadAllText(registryPath);
            var parsed = JsonSerializer.Deserialize<List<ScreenRegistration>>(raw);
            if (parsed == null || parsed.Count == 0)
                return defaults;

            parsed.RemoveAll(item => item == null || string.IsNullOrWhiteSpace(item.ScreenKey));
            return parsed.Count == 0 ? defaults : parsed;
        }
        catch (Exception ex)
        {
            _logger.Warning($"[FMF.UIReplacement] Failed to read {ScreenRegistryFileName}: {ex.Message}. Using defaults.");
            return defaults;
        }
    }

    private static void TryWriteDefaultScreenRegistry(string registryPath, List<ScreenRegistration> defaults)
    {
        try
        {
            string json = JsonSerializer.Serialize(defaults, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(registryPath, json);
        }
        catch
        {
        }
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
        return "<div id='root'><h1>Data Center — Start Menu</h1><p>Continue, New Game, Multiplayer, Settings, and Exit are available from the modern overlay.</p></div>";
    }

    private static string DefaultCss()
    {
        return @":root{--panel-color:#0b1220f2;--text-color:#f8fafc;--secondary-text-color:#cbd5e1;--btn-normal:#1d4ed8;--btn-highlight:#2563eb;--btn-pressed:#1e40af;--accent:#22d3ee;}\n"
            + ".app-shell{animation:fade-in .35s ease-out;}\n"
            + ".app-card{backdrop-filter:blur(8px);border:1px solid rgba(56,189,248,.35);box-shadow:0 10px 30px rgba(2,6,23,.5);animation:slide-up .3s ease-out;}\n"
            + ".menu-action-button{border:1px solid transparent;transition:transform .12s ease,filter .12s ease;}\n"
            + ".menu-action-button:hover{transform:translateY(-1px);filter:brightness(1.08);}\n"
            + ".menu-action-button:focus,.menu-action-button:focus-visible{outline:2px solid var(--accent);outline-offset:2px;box-shadow:0 0 0 3px #22d3ee44;}\n"
            + ".menu-action-button--primary{background:var(--btn-normal);border-color:#60a5fa;color:#fff;}\n"
            + ".menu-action-button--secondary{background:#1e293b;border-color:#64748b;color:#dbeafe;}\n"
            + ".menu-action-button--danger{background:#7f1d1d;border-color:#f87171;color:#ffe4e6;}\n"
            + "@keyframes fade-in{from{opacity:0}to{opacity:1}}\n"
            + "@keyframes slide-up{from{opacity:0;transform:translateY(8px)}to{opacity:1;transform:translateY(0)}}";
    }

    private static string DefaultTsx()
    {
        return "const App = () => (\n"
            + "  <div className='app-shell'>\n"
            + "    <div className='app-card'>\n"
            + "      <h1>Data Center - Start Menu</h1>\n"
            + "      <p>Fully modernized main menu for FrikaMF.</p>\n"
            + "      <div className='menu-grid'>\n"
            + "        <button className='menu-action-button menu-action-button--primary' style={{ borderWidth: '1px', borderStyle: 'solid' }}>Continue</button>\n"
            + "        <button className='menu-action-button menu-action-button--secondary' style={{ borderWidth: '1px', borderStyle: 'solid' }}>New Game</button>\n"
            + "        <button className='menu-action-button menu-action-button--secondary' style={{ borderWidth: '1px', borderStyle: 'solid' }}>Multiplayer</button>\n"
            + "        <button className='menu-action-button menu-action-button--secondary' style={{ borderWidth: '1px', borderStyle: 'solid' }}>Settings</button>\n"
            + "        <button className='menu-action-button menu-action-button--danger' style={{ borderWidth: '1px', borderStyle: 'solid' }}>Exit</button>\n"
            + "      </div>\n"
            + "    </div>\n"
            + "  </div>\n"
            + ");";
    }
}
