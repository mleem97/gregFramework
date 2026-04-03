using System;
using Il2Cpp;
using UnityEngine;

namespace DataCenterModLoader;

public static class ModSettingsMenuBridge
{
    private static bool _showChooser;
    private static bool _showModSettings;
    private static Rect _chooserRect = new Rect(0, 0, 420, 220);
    private static Rect _settingsRect = new Rect(0, 0, 580, 420);

    private static bool _stylesInitialized;
    private static GUIStyle _windowStyle;
    private static GUIStyle _titleStyle;
    private static GUIStyle _buttonStyle;
    private static GUIStyle _labelStyle;

    private static GameObject _settingsRoot;
    private static bool _replaceMainMenuWithWeb;

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
                DC2WebBridge.TryApplyOrReplace(_settingsRoot, "MainMenuSettings");
        }

    }

    private static void DrawModSettingsWindow()
    {
        float x = _settingsRect.x + 20f;
        float y = _settingsRect.y + 44f;
        float w = _settingsRect.width - 40f;

        GUI.Label(new Rect(x, y, w, 24f), "UI Framework", _titleStyle);
        y += 30f;

        DC2WebBridge.Enabled = GUI.Toggle(new Rect(x, y, w, 24f), DC2WebBridge.Enabled, "DC2WEB Bridge aktiv", _labelStyle);
        y += 28f;

        UiModernizer.Enabled = GUI.Toggle(new Rect(x, y, w, 24f), UiModernizer.Enabled, "Unity UI Modernizer aktiv", _labelStyle);
        y += 28f;

        _replaceMainMenuWithWeb = GUI.Toggle(new Rect(x, y, w, 24f), _replaceMainMenuWithWeb, "MainMenu Settings per Web-Overlay ersetzen", _labelStyle);
        y += 34f;

        if (GUI.Button(new Rect(x, y, w, 36f), "Apply to current Settings", _buttonStyle))
        {
            DC2WebBridge.SetProfileReplaceMode("MainMenuSettings", _replaceMainMenuWithWeb);
            if (_settingsRoot != null)
            {
                DC2WebBridge.ResetAppliedState(_settingsRoot);
                DC2WebBridge.TryApplyOrReplace(_settingsRoot, "MainMenuSettings");
            }
        }

        y += 46f;
        GUI.Label(new Rect(x, y, w, 80f), "Hinweis: Game Settings bleiben weiterhin erhalten. Das Mod Panel steuert nur Framework-UI-Features.", _labelStyle);
        y += 92f;

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
}
