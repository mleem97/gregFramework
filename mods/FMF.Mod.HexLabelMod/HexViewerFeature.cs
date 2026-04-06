using System;
using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FMF.HexLabelMod;

internal static class HexViewerFeature
{
    private static bool _visible;
    private static Vector2 _scroll;
    private static readonly List<CableColorEntry> _entries = new();
    private static bool _colorblindMode;
    private static string _heldLine = "Held: —";
    public static void Initialize()
    {
    }

    public static void Update()
    {
        var kb = Keyboard.current;
        if (kb == null)
            return;

        if (kb.f2Key.wasPressedThisFrame)
        {
            _visible = !_visible;
            if (_visible)
                RefreshList();
        }
    }

    private static void RefreshList()
    {
        try
        {
            _entries.Clear();
            _entries.AddRange(CableColorCollector.CollectAll());
        }
        catch (Exception ex)
        {
            MelonLogger.Msg($"HexViewer: {ex.Message}");
        }

        UpdateHeldLine();
    }

    private static void UpdateHeldLine()
    {
        var kind = HeldCableKindResolver.Resolve();
        HeldCableKindResolver.TryGetHeldCableHex(out var heldHex);

        if (string.IsNullOrEmpty(kind) && string.IsNullOrEmpty(heldHex))
            _heldLine = "Held: —";
        else if (!string.IsNullOrEmpty(kind) && !string.IsNullOrEmpty(heldHex))
            _heldLine = $"Held: {kind} — {heldHex}";
        else if (!string.IsNullOrEmpty(kind))
            _heldLine = $"Held: {kind}";
        else
            _heldLine = $"Held: {heldHex}";
    }

    public static void OnGui()
    {
        if (!_visible)
            return;

        if (Time.frameCount % 30 == 0)
            UpdateHeldLine();

        const float w = 560f;
        const float h = 420f;
        var x = (Screen.width - w) * 0.5f;
        var y = (Screen.height - h) * 0.5f;

        GUI.Box(new Rect(x, y, w, h), "Cable color viewer (F2)");
        GUILayout.BeginArea(new Rect(x + 10, y + 28, w - 20, h - 38));

        GUILayout.Label("Colors from scene (CableSpinner), Save.member_values, and save JSON files.", GUI.skin.box);

        _colorblindMode = GUILayout.Toggle(_colorblindMode,
            "Colorblind: show RJ / SFP / QSFP + hex for held cable");

        var heldStyle = new GUIStyle
        {
            fontSize = _colorblindMode ? 22 : 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        GUILayout.Label(_heldLine, heldStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh", GUILayout.Width(120)))
            RefreshList();
        if (GUILayout.Button("Close", GUILayout.Width(120)))
            _visible = false;
        GUILayout.EndHorizontal();

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(220));
        foreach (var e in _entries)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(28), GUILayout.Height(22));
            var last = GUI.color;
            if (HexColorUtil.TryHexToColor(e.Hex, out var col))
                GUI.color = col;
            GUILayout.Box("", GUILayout.Width(28), GUILayout.Height(22));
            GUI.color = last;

            GUILayout.Label(e.Hex, GUILayout.Width(100));
            GUILayout.Label(e.Source, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
