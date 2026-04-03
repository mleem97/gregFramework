using System;
using System.Collections.Generic;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DataCenterModLoader;

public static class UiModernizer
{
    private static readonly HashSet<int> ProcessedRoots = new();

    private static readonly Color PrimaryText = new Color(0.94f, 0.96f, 1.00f, 1f);
    private static readonly Color SecondaryText = new Color(0.72f, 0.78f, 0.90f, 1f);
    private static readonly Color Accent = new Color(0.28f, 0.65f, 1.00f, 1f);
    private static readonly Color ButtonNormal = new Color(0.12f, 0.16f, 0.24f, 1f);
    private static readonly Color ButtonHighlight = new Color(0.18f, 0.24f, 0.36f, 1f);
    private static readonly Color ButtonPressed = new Color(0.10f, 0.13f, 0.20f, 1f);
    private static readonly Color PanelTint = new Color(0.06f, 0.08f, 0.12f, 0.88f);

    public static bool Enabled = true;

    public static void TryModernize(GameObject root, string sourceTag)
    {
        if (!Enabled || root == null)
            return;

        try
        {
            int id = root.GetInstanceID();
            if (!ProcessedRoots.Add(id))
                return;

            ApplyToTexts(root);
            ApplyToSelectables(root);
            ApplyToPanelImages(root);

            CrashLog.Log($"UiModernizer: applied modern style to '{root.name}' via {sourceTag}");
        }
        catch (Exception ex)
        {
            CrashLog.LogException($"UiModernizer.TryModernize({sourceTag})", ex);
        }
    }

    private static void ApplyToTexts(GameObject root)
    {
        var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (labels == null) return;

        foreach (var label in labels)
        {
            if (label == null) continue;

            string objectName = label.gameObject.name ?? string.Empty;
            bool isTitle = objectName.IndexOf("title", StringComparison.OrdinalIgnoreCase) >= 0
                           || objectName.IndexOf("header", StringComparison.OrdinalIgnoreCase) >= 0;

            label.color = isTitle ? PrimaryText : SecondaryText;
            if (isTitle)
            {
                label.fontSize = Mathf.Max(label.fontSize, 30f);
                label.fontStyle = FontStyles.Bold;
            }
            else
            {
                label.fontSize = Mathf.Max(label.fontSize, 20f);
            }

            try { label.ForceMeshUpdate(); } catch { }
        }
    }

    private static void ApplyToSelectables(GameObject root)
    {
        var selectables = root.GetComponentsInChildren<Selectable>(true);
        if (selectables == null) return;

        foreach (var selectable in selectables)
        {
            if (selectable == null) continue;

            var colors = selectable.colors;
            colors.normalColor = ButtonNormal;
            colors.highlightedColor = ButtonHighlight;
            colors.pressedColor = ButtonPressed;
            colors.selectedColor = Accent;
            colors.disabledColor = new Color(0.35f, 0.35f, 0.40f, 0.55f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;

            selectable.transition = Selectable.Transition.ColorTint;
            selectable.colors = colors;
        }
    }

    private static void ApplyToPanelImages(GameObject root)
    {
        var images = root.GetComponentsInChildren<Image>(true);
        if (images == null) return;

        foreach (var image in images)
        {
            if (image == null) continue;

            string objectName = image.gameObject.name ?? string.Empty;
            if (objectName.IndexOf("panel", StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("window", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                image.color = PanelTint;
            }
        }
    }
}
