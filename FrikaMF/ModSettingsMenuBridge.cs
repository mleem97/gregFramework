using System;
using Il2Cpp;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DataCenterModLoader;

public static class ModSettingsMenuBridge
{
    private static bool _showChooser;
    private static bool _showModSettings;
    private static Rect _chooserRect = new Rect(0, 0, 420, 220);
    private static Rect _settingsRect = new Rect(0, 0, 580, 700);

    private static bool _stylesInitialized;
    private static GUIStyle _windowStyle;
    private static GUIStyle _titleStyle;
    private static GUIStyle _buttonStyle;
    private static GUIStyle _labelStyle;

    private static GameObject _settingsRoot;
    private static bool _replaceMainMenuWithWeb;
    private static readonly Key[] _keyOptions =
    {
        Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6,
        Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12
    };

    private static Key _hostKey = Key.F9;
    private static Key _panelKey = Key.F10;
    private static Key _disconnectKey = Key.F11;

    public static void OnSceneLoaded(string sceneName)
    {
        _showChooser = false;
        _showModSettings = false;
        _settingsRoot = null;
    }

    public static void OnSettingsOpened(MainMenu mainMenu)
    {
        if (mainMenu == null)
            return;

        _settingsRoot = mainMenu.settings;
        _showChooser = true;
        _showModSettings = false;

        var bridge = Core.Multiplayer;
        if (bridge != null)
        {
            _hostKey = bridge.HostKey;
            _panelKey = bridge.PanelKey;
            _disconnectKey = bridge.DisconnectKey;
        }

        CenterRects();
    }

    public static void DrawGUI()
    {
        if (!_showChooser && !_showModSettings)
            return;

        EnsureStyles();
        CenterRects();

        if (_showChooser)
        {
            GUI.Box(_chooserRect, "Settings Auswahl", _windowStyle);
            DrawChooserWindow();
        }

        if (_showModSettings)
        {
            GUI.Box(_settingsRect, "Mod Settings", _windowStyle);
            DrawModSettingsWindow();
        }
    }

    private static void DrawChooserWindow()
    {
        float x = _chooserRect.x + 20f;
        float y = _chooserRect.y + 44f;
        float w = _chooserRect.width - 40f;

        GUI.Label(new Rect(x, y, w, 44f), "Welche Einstellungen willst du öffnen?", _titleStyle);
        y += 60f;

        if (GUI.Button(new Rect(x, y, w, 42f), "Game Settings", _buttonStyle))
        {
            _showChooser = false;
            _showModSettings = false;
        }

        y += 52f;
        if (GUI.Button(new Rect(x, y, w, 42f), "Mod Settings", _buttonStyle))
        {
            _showChooser = false;
            _showModSettings = true;
            if (_settingsRoot != null)
                UiExtensionBridge.TryApplyOrReplace(_settingsRoot, "MainMenuSettings");
        }

    }

    private static void DrawModSettingsWindow()
    {
        float x = _settingsRect.x + 20f;
        float y = _settingsRect.y + 44f;
        float w = _settingsRect.width - 40f;

        GUI.Label(new Rect(x, y, w, 24f), "UI Framework", _titleStyle);
        y += 30f;

        bool webBridgeEnabled = UiExtensionBridge.GetWebBridgeEnabled();
        webBridgeEnabled = GUI.Toggle(new Rect(x, y, w, 24f), webBridgeEnabled, "DC2WEB Bridge aktiv", _labelStyle);
        UiExtensionBridge.SetWebBridgeEnabled(webBridgeEnabled);
        y += 28f;

        UiModernizer.Enabled = GUI.Toggle(new Rect(x, y, w, 24f), UiModernizer.Enabled, "Unity UI Modernizer aktiv", _labelStyle);
        y += 28f;

        _replaceMainMenuWithWeb = GUI.Toggle(new Rect(x, y, w, 24f), _replaceMainMenuWithWeb, "MainMenu Settings per Web-Overlay ersetzen", _labelStyle);
        y += 34f;

        if (GUI.Button(new Rect(x, y, w, 36f), "Apply to current Settings", _buttonStyle))
        {
            UiExtensionBridge.SetWebProfileReplaceMode("MainMenuSettings", _replaceMainMenuWithWeb);
            if (_settingsRoot != null)
            {
                UiExtensionBridge.ResetWebAppliedState(_settingsRoot);
                UiExtensionBridge.TryApplyOrReplace(_settingsRoot, "MainMenuSettings");
            }
        }

        y += 46f;
        GUI.Label(new Rect(x, y, w, 80f), "Hinweis: Game Settings bleiben weiterhin erhalten. Das Mod Panel steuert nur Framework-UI-Features.", _labelStyle);
        y += 70f;

        GUI.Label(new Rect(x, y, w, 24f), "Multiplayer Keybinds", _titleStyle);
        y += 30f;

        if (GUI.Button(new Rect(x, y, w, 32f), $"Host Key: {_hostKey}", _buttonStyle))
            _hostKey = NextKey(_hostKey);
        y += 38f;

        if (GUI.Button(new Rect(x, y, w, 32f), $"Panel Key: {_panelKey}", _buttonStyle))
            _panelKey = NextKey(_panelKey);
        y += 38f;

        if (GUI.Button(new Rect(x, y, w, 32f), $"Disconnect Key: {_disconnectKey}", _buttonStyle))
            _disconnectKey = NextKey(_disconnectKey);
        y += 38f;

        if (GUI.Button(new Rect(x, y, w, 36f), "MP Keybinds speichern", _buttonStyle))
        {
            var bridge = Core.Multiplayer;
            bridge?.SetKeybinds(_hostKey, _panelKey, _disconnectKey);
        }

        y += 46f;

        if (GUI.Button(new Rect(x, y, w, 36f), "Zurück zur Auswahl", _buttonStyle))
        {
            _showModSettings = false;
            _showChooser = true;
        }

        y += 46f;
        if (GUI.Button(new Rect(x, y, w, 36f), "Schließen", _buttonStyle))
        {
            _showModSettings = false;
            _showChooser = false;
        }

    }

    private static void EnsureStyles()
    {
        if (_stylesInitialized)
            return;

        _windowStyle = GUI.skin.window;
        _titleStyle = GUI.skin.label;
        _buttonStyle = GUI.skin.button;
        _labelStyle = GUI.skin.label;

        _stylesInitialized = true;
    }

    private static void CenterRects()
    {
        _chooserRect.x = (Screen.width - _chooserRect.width) * 0.5f;
        _chooserRect.y = (Screen.height - _chooserRect.height) * 0.5f;

        _settingsRect.x = (Screen.width - _settingsRect.width) * 0.5f;
        _settingsRect.y = (Screen.height - _settingsRect.height) * 0.5f;
    }

    private static Key NextKey(Key current)
    {
        int index = Array.IndexOf(_keyOptions, current);
        if (index < 0)
            return _keyOptions[0];

        return _keyOptions[(index + 1) % _keyOptions.Length];
    }
}
