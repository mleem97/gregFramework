using System;
using Il2Cpp;
using UnityEngine;

namespace DataCenterModLoader;

/// <summary>
/// Optional extension bridge for UI/sysadmin features provided by standalone plugins.
/// </summary>
public static class UiExtensionBridge
{
    private static Func<GameObject, string, bool> _tryApplyOrReplace;
    private static Func<bool> _getWebEnabled;
    private static Action<bool> _setWebEnabled;
    private static Action<string, bool> _setWebProfileReplaceMode;
    private static Action<GameObject> _resetWebAppliedState;
    private static Action<GameObject, string> _tryModernize;
    private static Action<string> _onSceneLoaded;
    private static Action _drawGui;
    private static Action<MainMenu> _onSettingsOpened;

    /// <summary>
    /// Registers only the web-replacement handler.
    /// </summary>
    public static void RegisterWebReplacement(Func<GameObject, string, bool> tryApplyOrReplace)
    {
        _tryApplyOrReplace = tryApplyOrReplace;
    }

    /// <summary>
    /// Registers optional web bridge configuration handlers for settings UI.
    /// </summary>
    public static void RegisterWebConfiguration(
        Func<bool> getEnabled,
        Action<bool> setEnabled,
        Action<string, bool> setProfileReplaceMode,
        Action<GameObject> resetAppliedState)
    {
        _getWebEnabled = getEnabled;
        _setWebEnabled = setEnabled;
        _setWebProfileReplaceMode = setProfileReplaceMode;
        _resetWebAppliedState = resetAppliedState;
    }

    /// <summary>
    /// Unregisters only the web-replacement handler.
    /// </summary>
    public static void UnregisterWebReplacement()
    {
        _tryApplyOrReplace = null;
        _getWebEnabled = null;
        _setWebEnabled = null;
        _setWebProfileReplaceMode = null;
        _resetWebAppliedState = null;
    }

    /// <summary>
    /// Gets the registered web bridge enabled state.
    /// </summary>
    public static bool GetWebBridgeEnabled()
    {
        try
        {
            return _getWebEnabled?.Invoke() ?? false;
        }
        catch (Exception ex)
        {
            CrashLog.LogException("UiExtensionBridge.GetWebBridgeEnabled", ex);
            return false;
        }
    }

    /// <summary>
    /// Sets the registered web bridge enabled state.
    /// </summary>
    public static void SetWebBridgeEnabled(bool enabled)
    {
        try
        {
            _setWebEnabled?.Invoke(enabled);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("UiExtensionBridge.SetWebBridgeEnabled", ex);
        }
    }

    /// <summary>
    /// Sets web profile replacement mode when available.
    /// </summary>
    public static void SetWebProfileReplaceMode(string profileKey, bool replace)
    {
        try
        {
            _setWebProfileReplaceMode?.Invoke(profileKey, replace);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("UiExtensionBridge.SetWebProfileReplaceMode", ex);
        }
    }

    /// <summary>
    /// Resets web applied state for a target root object when available.
    /// </summary>
    public static void ResetWebAppliedState(GameObject root)
    {
        try
        {
            _resetWebAppliedState?.Invoke(root);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("UiExtensionBridge.ResetWebAppliedState", ex);
        }
    }

    /// <summary>
    /// Registers optional non-web UI handlers (modernizer, scene hook, GUI, settings hook).
    /// </summary>
    public static void RegisterUiHandlers(
        Action<GameObject, string> tryModernize,
        Action<string> onSceneLoaded,
        Action drawGui,
        Action<MainMenu> onSettingsOpened)
    {
        _tryModernize = tryModernize;
        _onSceneLoaded = onSceneLoaded;
        _drawGui = drawGui;
        _onSettingsOpened = onSettingsOpened;
    }

    /// <summary>
    /// Unregisters optional non-web UI handlers.
    /// </summary>
    public static void UnregisterUiHandlers()
    {
        _tryModernize = null;
        _onSceneLoaded = null;
        _drawGui = null;
        _onSettingsOpened = null;
    }

    /// <summary>
    /// Registers plugin-provided UI extension handlers.
    /// </summary>
    public static void Register(
        Func<GameObject, string, bool> tryApplyOrReplace,
        Action<GameObject, string> tryModernize,
        Action<string> onSceneLoaded,
        Action drawGui,
        Action<MainMenu> onSettingsOpened)
    {
        RegisterWebReplacement(tryApplyOrReplace);
        RegisterUiHandlers(tryModernize, onSceneLoaded, drawGui, onSettingsOpened);
    }

    /// <summary>
    /// Clears all currently registered handlers.
    /// </summary>
    public static void Unregister()
    {
        UnregisterWebReplacement();
        UnregisterUiHandlers();
    }

    /// <summary>
    /// Tries to apply a web replacement profile if a standalone plugin registered this capability.
    /// </summary>
    public static bool TryApplyOrReplace(GameObject root, string screenKey)
    {
        try
        {
            return _tryApplyOrReplace?.Invoke(root, screenKey) ?? false;
        }
        catch (Exception ex)
        {
            CrashLog.LogException("UiExtensionBridge.TryApplyOrReplace", ex);
            return false;
        }
    }

    /// <summary>
    /// Tries to modernize UI if a standalone plugin registered this capability.
    /// </summary>
    public static void TryModernize(GameObject root, string sourceTag)
    {
        try
        {
            _tryModernize?.Invoke(root, sourceTag);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("UiExtensionBridge.TryModernize", ex);
        }
    }

    /// <summary>
    /// Notifies optional UI extension plugins about scene loads.
    /// </summary>
    public static void OnSceneLoaded(string sceneName)
    {
        try
        {
            _onSceneLoaded?.Invoke(sceneName);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("UiExtensionBridge.OnSceneLoaded", ex);
        }
    }

    /// <summary>
    /// Invokes optional plugin GUI drawing.
    /// </summary>
    public static void DrawGui()
    {
        try
        {
            _drawGui?.Invoke();
        }
        catch (Exception ex)
        {
            CrashLog.LogException("UiExtensionBridge.DrawGui", ex);
        }
    }

    /// <summary>
    /// Notifies optional UI extension plugins that settings were opened.
    /// </summary>
    public static void OnSettingsOpened(MainMenu mainMenu)
    {
        try
        {
            _onSettingsOpened?.Invoke(mainMenu);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("UiExtensionBridge.OnSettingsOpened", ex);
        }
    }
}
