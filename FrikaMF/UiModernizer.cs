using System;
using System.Collections.Generic;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DataCenterModLoader;

/// <summary>
/// Applies a lightweight visual refresh to existing Unity UI trees.
/// Styling is intentionally non-destructive and only adjusts color/font/tint settings.
/// </summary>
public static class UiModernizer
{
    // Tracks roots already processed to avoid repeated styling passes on the same object tree.
    private static readonly HashSet<int> ProcessedRoots = new();

    // Shared color tokens used by all modernization passes.
    private static readonly Color PrimaryText = new Color(0.94f, 0.96f, 1.00f, 1f);
    private static readonly Color SecondaryText = new Color(0.72f, 0.78f, 0.90f, 1f);
    private static readonly Color Accent = new Color(0.28f, 0.65f, 1.00f, 1f);
    private static readonly Color ButtonNormal = new Color(0.12f, 0.16f, 0.24f, 1f);
    private static readonly Color ButtonHighlight = new Color(0.18f, 0.24f, 0.36f, 1f);
    private static readonly Color ButtonPressed = new Color(0.10f, 0.13f, 0.20f, 1f);
    private static readonly Color PanelTint = new Color(0.06f, 0.08f, 0.12f, 0.88f);

    // Global toggle for runtime styling.
    public static bool Enabled = true;

    /// <summary>
    /// Attempts to modernize one UI root and all its child controls.
    /// This method is idempotent per root instance.
    /// </summary>
    /// <param name="root">UI root to process.</param>
    /// <param name="sourceTag">Context label used for diagnostics logging.</param>
    public static void TryModernize(GameObject root, string sourceTag)
    {
        // Fast guard: disabled or invalid root.
        if (!Enabled || root == null)
            return;

        try
        {
            // Skip if this tree was already modernized.
            int id = root.GetInstanceID();
            if (!ProcessedRoots.Add(id))
                return;

            // Apply style transformations by control category.
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

    /// <summary>
    /// Modernizes text components by applying title/body heuristics and color tokens.
    /// </summary>
    private static void ApplyToTexts(GameObject root)
    {
        var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (labels == null) return;

        foreach (var label in labels)
        {
            if (label == null) continue;

            // Simple naming heuristic: objects containing "title" or "header" are treated as headings.
            string objectName = label.gameObject.name ?? string.Empty;
            bool isTitle = objectName.IndexOf("title", StringComparison.OrdinalIgnoreCase) >= 0
                           || objectName.IndexOf("header", StringComparison.OrdinalIgnoreCase) >= 0;

            // Apply semantic color and minimal size floors to improve readability.
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

            // Force TMP mesh refresh so visual updates are reflected immediately.
            try { label.ForceMeshUpdate(); } catch { }
        }
    }

    /// <summary>
    /// Modernizes Unity Selectables (buttons/toggles/etc.) with a consistent tint palette.
    /// </summary>
    private static void ApplyToSelectables(GameObject root)
    {
        var selectables = root.GetComponentsInChildren<Selectable>(true);
        if (selectables == null) return;

        foreach (var selectable in selectables)
        {
            if (selectable == null) continue;

            // Replace ColorBlock with framework defaults while keeping the existing component setup.
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

    /// <summary>
    /// Tints panel/background/window images for a unified dark overlay look.
    /// </summary>
    private static void ApplyToPanelImages(GameObject root)
    {
        var images = root.GetComponentsInChildren<Image>(true);
        if (images == null) return;

        foreach (var image in images)
        {
            if (image == null) continue;

            // Name-based target selection keeps behavior generic across different menu prefabs.
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
