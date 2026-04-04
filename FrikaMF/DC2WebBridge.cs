using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DataCenterModLoader;

public enum Dc2WebSourceType
{
    Html,
    Css,
    TailwindCss,
    Sass,
    Scss,
    JavaScript,
    TypeScript,
    ReactJsx,
    ReactTsx,
    JsonManifest,
    CustomFramework,
}

public enum Dc2WebImageType
{
    Svg,
    Png,
    Jpeg,
    Jpg,
    Bmp,
    Gif,
    Tga,
    Unknown,
}

public sealed class Dc2WebImageAsset
{
    public string Name = string.Empty;
    public Dc2WebImageType Type = Dc2WebImageType.Unknown;
    public string FilePath = string.Empty;
    public byte[] BinaryData;
    public string SvgMarkup = string.Empty;
}

public sealed class Dc2WebAppDescriptor
{
    public string ScreenKey = string.Empty;
    public bool ReplaceExistingUi = true;
    public string Html = string.Empty;
    public string Css = string.Empty;
    public string Script = string.Empty;
    public string Framework = string.Empty;
    public readonly List<Dc2WebImageAsset> Images = new();
}

public sealed class Dc2WebSource
{
    public Dc2WebSourceType Type;
    public string Framework = string.Empty;
    public string Content = string.Empty;
}

public sealed class Dc2WebBundle
{
    public string BundleId = string.Empty;
    public bool ReplaceExistingUi;
    public readonly List<Dc2WebSource> Sources = new();
}

public interface IDc2WebFrameworkAdapter
{
    bool CanHandle(Dc2WebSource source);
    string TranslateToCss(Dc2WebSource source);
}

internal sealed class UnityUiStyleProfile
{
    public string ScreenKey = string.Empty;
    public bool ReplaceExistingUi;
    public string HtmlTemplate = string.Empty;
    public Color PanelColor = new Color(0.07f, 0.10f, 0.16f, 0.90f);
    public Color TextColor = new Color(0.93f, 0.96f, 1.00f, 1.00f);
    public Color SecondaryTextColor = new Color(0.72f, 0.78f, 0.90f, 1.00f);
    public Color ButtonNormal = new Color(0.13f, 0.17f, 0.25f, 1.00f);
    public Color ButtonHighlighted = new Color(0.19f, 0.25f, 0.37f, 1.00f);
    public Color ButtonPressed = new Color(0.10f, 0.14f, 0.22f, 1.00f);
    public Color Accent = new Color(0.28f, 0.65f, 1.00f, 1.00f);
    public float TitleSize = 30f;
    public float BodySize = 20f;
    public string PreferredFontFamily = string.Empty;
    public string PreferredGoogleFontFamily = string.Empty;
    public string PreferredLocalFontAsset = string.Empty;
}

public static class DC2WebBridge
{
    public static bool Enabled = true;

    private static readonly Dictionary<string, UnityUiStyleProfile> ProfilesByScreen = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<IDc2WebFrameworkAdapter> Adapters = new();
    private static readonly HashSet<int> AppliedRoots = new();
    private static readonly Dictionary<string, TMP_FontAsset> FontCache = new(StringComparer.OrdinalIgnoreCase);

    static DC2WebBridge()
    {
        RegisterAdapter(new TailwindAdapter());
        RegisterAdapter(new SassAdapter());
        RegisterAdapter(new JsTsAdapter());
        RegisterAdapter(new ReactAdapter());

        RegisterBundle("HRSystem", new Dc2WebBundle
        {
            BundleId = "default-hr-tailwind",
            ReplaceExistingUi = false,
            Sources =
            {
                new Dc2WebSource { Type = Dc2WebSourceType.TailwindCss, Content = "bg-slate-900 text-slate-100 text-sm" },
                new Dc2WebSource
                {
                    Type = Dc2WebSourceType.Css,
                    Content = ":root{--panel-color:#0f172acc;--text-color:#f1f5f9;--secondary-text-color:#94a3b8;--btn-normal:#1e293b;--btn-highlight:#334155;--btn-pressed:#0f172a;--accent:#38bdf8;}"
                }
            }
        });

        RegisterBundle("MainMenuSettings", new Dc2WebBundle
        {
            BundleId = "default-mainmenu-settings",
            ReplaceExistingUi = false,
            Sources =
            {
                new Dc2WebSource { Type = Dc2WebSourceType.Html, Content = "<div><h1>Mod Settings</h1><p>Switch between game settings and mod framework settings.</p></div>" },
                new Dc2WebSource { Type = Dc2WebSourceType.TailwindCss, Content = "bg-slate-900 text-slate-100 text-base text-sky-400" },
            }
        });

        RegisterWebApp(new Dc2WebAppDescriptor
        {
            ScreenKey = "MainMenuReact",
            ReplaceExistingUi = true,
            Framework = "react-ts",
            Html = "<div id='root'><h1>DC2WEB React UI</h1><p>Runtime-translated app skin</p></div>",
            Css = ":root{--panel-color:#111827dd;--text-color:#f9fafb;--secondary-text-color:#d1d5db;--accent:#60a5fa;}",
            Script = "const App = () => <div className='bg-slate-900 text-slate-100 text-3xl'>React UI</div>;",
        });
    }

    public static bool RegisterWebApp(Dc2WebAppDescriptor app)
    {
        if (app == null || string.IsNullOrWhiteSpace(app.ScreenKey))
            return false;

        var bundle = new Dc2WebBundle
        {
            BundleId = string.IsNullOrWhiteSpace(app.Framework) ? $"app-{app.ScreenKey}" : app.Framework,
            ReplaceExistingUi = app.ReplaceExistingUi,
        };

        if (!string.IsNullOrWhiteSpace(app.Html))
            bundle.Sources.Add(new Dc2WebSource { Type = Dc2WebSourceType.Html, Content = app.Html, Framework = app.Framework });

        if (!string.IsNullOrWhiteSpace(app.Css))
            bundle.Sources.Add(new Dc2WebSource { Type = Dc2WebSourceType.Css, Content = app.Css, Framework = app.Framework });

        if (!string.IsNullOrWhiteSpace(app.Script))
        {
            bool react = app.Framework.IndexOf("react", StringComparison.OrdinalIgnoreCase) >= 0;
            bundle.Sources.Add(new Dc2WebSource
            {
                Type = react ? Dc2WebSourceType.ReactTsx : Dc2WebSourceType.TypeScript,
                Content = app.Script,
                Framework = app.Framework,
            });
        }

        RegisterBundle(app.ScreenKey, bundle);
        return true;
    }

    public static void SetProfileReplaceMode(string screenKey, bool replaceUi)
    {
        if (string.IsNullOrWhiteSpace(screenKey))
            return;

        if (ProfilesByScreen.TryGetValue(screenKey, out var profile))
        {
            profile.ReplaceExistingUi = replaceUi;
            return;
        }

        ProfilesByScreen[screenKey] = new UnityUiStyleProfile { ReplaceExistingUi = replaceUi };
    }

    public static void ResetAppliedState(GameObject root)
    {
        if (root == null) return;
        AppliedRoots.Remove(root.GetInstanceID());
    }

    public static bool TryAssignImage(Image target, Dc2WebImageAsset asset, int preferredSize = 256)
    {
        if (target == null || asset == null)
            return false;

        Sprite sprite = TryCreateSprite(asset, preferredSize, preferredSize);
        if (sprite == null)
            return false;

        target.sprite = sprite;
        target.preserveAspect = true;
        return true;
    }

    public static Sprite TryCreateSprite(Dc2WebImageAsset asset, int width = 256, int height = 256)
    {
        if (asset == null)
            return null;

        Dc2WebImageType kind = asset.Type;
        if (kind == Dc2WebImageType.Unknown && !string.IsNullOrWhiteSpace(asset.FilePath))
            kind = GuessImageTypeFromPath(asset.FilePath);

        if (kind == Dc2WebImageType.Svg)
            return CreateSpriteFromSvgAsset(asset, width, height);

        return CreateSpriteFromRasterAsset(asset);
    }

    public static void RegisterAdapter(IDc2WebFrameworkAdapter adapter)
    {
        if (adapter == null) return;
        Adapters.Add(adapter);
    }

    public static void RegisterBundle(string screenKey, Dc2WebBundle bundle)
    {
        if (string.IsNullOrWhiteSpace(screenKey) || bundle == null)
            return;

        ProfilesByScreen[screenKey] = BuildProfile(screenKey, bundle);
    }

    public static bool TryApplyOrReplace(GameObject root, string screenKey)
    {
        if (!Enabled || root == null || string.IsNullOrWhiteSpace(screenKey))
            return false;

        if (!ProfilesByScreen.TryGetValue(screenKey, out var profile))
            return false;

        int rootId = root.GetInstanceID();
        if (!AppliedRoots.Add(rootId))
            return true;

        ApplyProfile(root, profile);
        if (profile.ReplaceExistingUi)
            BuildReplacementOverlay(root, profile);

        CrashLog.Log($"DC2WebBridge: applied profile '{screenKey}' to '{root.name}'");
        return true;
    }

    public static bool TryApplyInlineSources(GameObject root, string screenKey, bool replaceUi, params Dc2WebSource[] sources)
    {
        if (root == null || string.IsNullOrWhiteSpace(screenKey) || sources == null || sources.Length == 0)
            return false;

        var bundle = new Dc2WebBundle { BundleId = $"inline-{screenKey}", ReplaceExistingUi = replaceUi };
        bundle.Sources.AddRange(sources);
        RegisterBundle(screenKey, bundle);
        return TryApplyOrReplace(root, screenKey);
    }

    private static UnityUiStyleProfile BuildProfile(string screenKey, Dc2WebBundle bundle)
    {
        var profile = new UnityUiStyleProfile
        {
            ScreenKey = screenKey,
            ReplaceExistingUi = bundle.ReplaceExistingUi,
        };

        foreach (var source in bundle.Sources)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.Content))
                continue;

            if (source.Type == Dc2WebSourceType.Html)
            {
                profile.HtmlTemplate = source.Content;
                continue;
            }

            string css = TranslateSourceToCss(source);
            if (!string.IsNullOrWhiteSpace(css))
                IngestCssIntoProfile(profile, css);
        }

        return profile;
    }

    private static string TranslateSourceToCss(Dc2WebSource source)
    {
        if (source.Type == Dc2WebSourceType.Css)
            return source.Content;

        foreach (var adapter in Adapters)
        {
            if (adapter.CanHandle(source))
                return adapter.TranslateToCss(source);
        }

        return source.Content;
    }

    private static void IngestCssIntoProfile(UnityUiStyleProfile profile, string css)
    {
        TryExtractGoogleFontImport(css, profile);

        var variableRegex = new Regex("--(?<name>[a-zA-Z0-9-_]+)\\s*:\\s*(?<value>[^;]+);", RegexOptions.CultureInvariant);
        foreach (Match m in variableRegex.Matches(css))
        {
            string key = m.Groups["name"].Value.Trim();
            string value = m.Groups["value"].Value.Trim();
            ApplyVariable(profile, key, value);
        }

        var propRegex = new Regex("(?<prop>[a-zA-Z-]+)\\s*:\\s*(?<value>[^;}{]+);", RegexOptions.CultureInvariant);
        foreach (Match m in propRegex.Matches(css))
        {
            ApplyProperty(profile, m.Groups["prop"].Value.Trim(), m.Groups["value"].Value.Trim());
        }
    }

    private static void ApplyVariable(UnityUiStyleProfile profile, string name, string value)
    {
        switch (name.ToLowerInvariant())
        {
            case "panel-color":
                TrySetColor(value, c => profile.PanelColor = c);
                break;
            case "text-color":
                TrySetColor(value, c => profile.TextColor = c);
                break;
            case "secondary-text-color":
                TrySetColor(value, c => profile.SecondaryTextColor = c);
                break;
            case "btn-normal":
                TrySetColor(value, c => profile.ButtonNormal = c);
                break;
            case "btn-highlight":
                TrySetColor(value, c => profile.ButtonHighlighted = c);
                break;
            case "btn-pressed":
                TrySetColor(value, c => profile.ButtonPressed = c);
                break;
            case "accent":
                TrySetColor(value, c => profile.Accent = c);
                break;
            case "font-family":
                profile.PreferredFontFamily = ExtractPrimaryFontFamily(value);
                break;
            case "google-font":
                profile.PreferredGoogleFontFamily = ExtractPrimaryFontFamily(value);
                break;
            case "local-font":
            case "font-asset":
                profile.PreferredLocalFontAsset = value.Trim().Trim('"', '\'');
                break;
        }
    }

    private static void ApplyProperty(UnityUiStyleProfile profile, string property, string value)
    {
        switch (property.ToLowerInvariant())
        {
            case "background":
            case "background-color":
                TrySetColor(value, c => profile.PanelColor = c);
                break;
            case "color":
                TrySetColor(value, c => profile.TextColor = c);
                break;
            case "font-size":
                if (TryParsePx(value, out float px))
                    profile.BodySize = Mathf.Clamp(px, 12f, 48f);
                break;
            case "font-family":
                profile.PreferredFontFamily = ExtractPrimaryFontFamily(value);
                break;
        }
    }

    private static void ApplyProfile(GameObject root, UnityUiStyleProfile profile)
    {
        TMP_FontAsset resolvedFont = ResolvePreferredFont(profile);

        var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (texts != null)
        {
            foreach (var text in texts)
            {
                if (text == null) continue;
                bool title = IsTitleLike(text.gameObject.name);
                text.color = title ? profile.TextColor : profile.SecondaryTextColor;
                text.fontSize = Mathf.Max(text.fontSize, title ? profile.TitleSize : profile.BodySize);
                if (resolvedFont != null)
                    text.font = resolvedFont;
                if (title)
                    text.fontStyle = FontStyles.Bold;
            }
        }

        var selectables = root.GetComponentsInChildren<Selectable>(true);
        if (selectables != null)
        {
            foreach (var selectable in selectables)
            {
                if (selectable == null) continue;
                var colors = selectable.colors;
                colors.normalColor = profile.ButtonNormal;
                colors.highlightedColor = profile.ButtonHighlighted;
                colors.pressedColor = profile.ButtonPressed;
                colors.selectedColor = profile.Accent;
                colors.disabledColor = new Color(0.35f, 0.35f, 0.4f, 0.55f);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.08f;
                selectable.transition = Selectable.Transition.ColorTint;
                selectable.colors = colors;
            }
        }

        var images = root.GetComponentsInChildren<Image>(true);
        if (images != null)
        {
            foreach (var image in images)
            {
                if (image == null) continue;
                if (IsPanelLike(image.gameObject.name))
                    image.color = profile.PanelColor;
            }
        }
    }

    private static void BuildReplacementOverlay(GameObject root, UnityUiStyleProfile profile)
    {
        GameObject overlay = FindOrCreateOverlay(root);
        var image = overlay.GetComponent<Image>();
        if (image != null)
            image.color = profile.PanelColor;

        string title = ExtractHtmlTitle(profile.HtmlTemplate);
        string body = ExtractHtmlBody(profile.HtmlTemplate);

        EnsureOverlayText(overlay.transform, "DC2WEB_Title", title, profile.TextColor, profile.TitleSize, new Vector2(0.5f, 0.86f), new Vector2(0.8f, 0.12f), true);
        EnsureOverlayText(overlay.transform, "DC2WEB_Body", body, profile.SecondaryTextColor, profile.BodySize, new Vector2(0.5f, 0.70f), new Vector2(0.9f, 0.22f), false);

        var iconNode = overlay.transform.Find("DC2WEB_Icon");
        if (iconNode != null)
        {
            var iconImage = iconNode.GetComponent<Image>();
            if (iconImage != null)
                iconImage.color = profile.Accent;
        }

        EnsureMainMenuActions(root, overlay, profile);
    }

    private static GameObject FindOrCreateOverlay(GameObject root)
    {
        var existing = root.transform.Find("DC2WEB_Overlay");
        if (existing != null)
            return existing.gameObject;

        var go = new GameObject("DC2WEB_Overlay");
        go.transform.SetParent(root.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<CanvasRenderer>();
        var image = go.AddComponent<Image>();
        image.raycastTarget = true;
        go.transform.SetAsLastSibling();
        return go;
    }

    private static void EnsureMainMenuActions(GameObject root, GameObject overlay, UnityUiStyleProfile profile)
    {
        if (root == null || overlay == null || profile == null)
            return;

        bool isMainMenu = string.Equals(profile.ScreenKey, "MainMenuReact", StringComparison.OrdinalIgnoreCase)
                          || (root.name?.IndexOf("mainmenu", StringComparison.OrdinalIgnoreCase) >= 0);
        if (!isMainMenu)
            return;

        var overlayImage = overlay.GetComponent<Image>();
        if (overlayImage != null)
            overlayImage.raycastTarget = true;

        var actionsRow = EnsureActionRow(overlay.transform);

        EnsureMainMenuButton(actionsRow.transform, "DC2WEB_Action_Continue", "Continue", profile, () => TryInvokeMainMenuAction(root, "continue"));
        EnsureMainMenuButton(actionsRow.transform, "DC2WEB_Action_New", "New Game", profile, () => TryInvokeMainMenuAction(root, "new"));
        EnsureMainMenuButton(actionsRow.transform, "DC2WEB_Action_Multiplayer", "Multiplayer", profile, () => TryInvokeMainMenuAction(root, "multiplayer"));
        EnsureMainMenuButton(actionsRow.transform, "DC2WEB_Action_Settings", "Settings", profile, () => TryInvokeMainMenuAction(root, "settings"));
        EnsureMainMenuButton(actionsRow.transform, "DC2WEB_Action_Exit", "Exit", profile, () => TryInvokeMainMenuAction(root, "exit"));
    }

    private static GameObject EnsureActionRow(Transform parent)
    {
        var existing = parent.Find("DC2WEB_MainMenu_Actions");
        if (existing != null)
            return existing.gameObject;

        var row = new GameObject("DC2WEB_MainMenu_Actions");
        row.transform.SetParent(parent, false);

        var rt = row.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.30f);
        rt.anchorMax = new Vector2(0.5f, 0.30f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(Mathf.Min(Screen.width * 0.90f, 1240f), 78f);
        rt.anchoredPosition = Vector2.zero;

        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        return row;
    }

    private static void EnsureMainMenuButton(Transform parent, string nodeName, string text, UnityUiStyleProfile profile, Action callback)
    {
        var existing = parent.Find(nodeName);
        GameObject node = existing != null ? existing.gameObject : new GameObject(nodeName);
        if (existing == null)
            node.transform.SetParent(parent, false);

        var layoutElement = node.GetComponent<LayoutElement>() ?? node.AddComponent<LayoutElement>();
        layoutElement.minWidth = 170f;
        layoutElement.preferredWidth = 190f;
        layoutElement.minHeight = 64f;

        var image = node.GetComponent<Image>() ?? node.AddComponent<Image>();
        image.color = profile.ButtonNormal;

        var button = node.GetComponent<Button>() ?? node.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.normalColor = profile.ButtonNormal;
        colors.highlightedColor = profile.ButtonHighlighted;
        colors.pressedColor = profile.ButtonPressed;
        colors.selectedColor = profile.Accent;
        colors.disabledColor = new Color(0.25f, 0.28f, 0.33f, 0.65f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener((System.Action)(() => callback?.Invoke()));

        var labelTransform = node.transform.Find("Label");
        GameObject labelNode = labelTransform != null ? labelTransform.gameObject : new GameObject("Label");
        if (labelTransform == null)
            labelNode.transform.SetParent(node.transform, false);

        var lrt = labelNode.GetComponent<RectTransform>() ?? labelNode.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(10f, 6f);
        lrt.offsetMax = new Vector2(-10f, -6f);

        var tmp = labelNode.GetComponent<TextMeshProUGUI>() ?? labelNode.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = profile.TextColor;
        tmp.fontSize = Mathf.Max(20f, profile.BodySize);
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private static bool TryInvokeMainMenuAction(GameObject root, string action)
    {
        string[] aliases = GetMainMenuAliases(action);
        if (aliases == null || aliases.Length == 0)
            return false;

        var buttons = root.GetComponentsInChildren<Button>(true);
        if (buttons == null)
            return false;

        Button bestMatch = null;
        int bestScore = int.MaxValue;

        foreach (var button in buttons)
        {
            if (button == null)
                continue;

            string caption = GetButtonCaption(button);
            if (string.IsNullOrWhiteSpace(caption))
                continue;

            string normalized = caption.Trim().ToLowerInvariant();
            for (int index = 0; index < aliases.Length; index++)
            {
                string alias = aliases[index];
                if (!normalized.Contains(alias))
                    continue;

                int score = Math.Abs(normalized.Length - alias.Length);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMatch = button;
                }
            }
        }

        if (bestMatch == null)
        {
            CrashLog.Log($"DC2WebBridge: no main menu target found for action '{action}'.");
            return false;
        }

        try
        {
            bestMatch.onClick?.Invoke();
            CrashLog.Log($"DC2WebBridge: invoked main menu action '{action}' via '{GetButtonCaption(bestMatch)}'.");
            return true;
        }
        catch (Exception ex)
        {
            CrashLog.Log($"DC2WebBridge: failed to invoke action '{action}': {ex.Message}");
            return false;
        }
    }

    private static string GetButtonCaption(Button button)
    {
        if (button == null)
            return string.Empty;

        var tmps = button.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (tmps != null)
        {
            foreach (var tmp in tmps)
            {
                if (tmp == null) continue;
                if (!string.IsNullOrWhiteSpace(tmp.text))
                    return tmp.text;
            }
        }

        var legacy = button.GetComponentsInChildren<Text>(true);
        if (legacy != null)
        {
            foreach (var text in legacy)
            {
                if (text == null) continue;
                if (!string.IsNullOrWhiteSpace(text.text))
                    return text.text;
            }
        }

        return button.name ?? string.Empty;
    }

    private static string[] GetMainMenuAliases(string action)
    {
        switch ((action ?? string.Empty).Trim().ToLowerInvariant())
        {
            case "continue":
                return new[] { "continue", "weiter", "fortsetzen" };
            case "new":
                return new[] { "new game", "new", "neues spiel", "start" };
            case "multiplayer":
                return new[] { "multiplayer", "online", "host", "join" };
            case "settings":
                return new[] { "settings", "options", "einstellungen" };
            case "exit":
                return new[] { "exit", "quit", "beenden" };
            default:
                return Array.Empty<string>();
        }
    }

    private static void EnsureOverlayText(Transform parent, string nodeName, string textValue, Color color, float size, Vector2 anchorCenter, Vector2 sizeScale, bool title)
    {
        var child = parent.Find(nodeName);
        GameObject node = child != null ? child.gameObject : new GameObject(nodeName);
        if (child == null)
            node.transform.SetParent(parent, false);

        var rt = node.GetComponent<RectTransform>() ?? node.AddComponent<RectTransform>();
        rt.anchorMin = anchorCenter;
        rt.anchorMax = anchorCenter;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(Screen.width * sizeScale.x, Screen.height * sizeScale.y);
        rt.anchoredPosition = Vector2.zero;

        var tmp = node.GetComponent<TextMeshProUGUI>() ?? node.AddComponent<TextMeshProUGUI>();
        tmp.text = string.IsNullOrWhiteSpace(textValue) ? (title ? "DC2WEB" : "Web-to-Game UI active") : textValue;
        tmp.color = color;
        tmp.alignment = title ? TextAlignmentOptions.Center : TextAlignmentOptions.TopLeft;
        tmp.enableWordWrapping = true;
        tmp.fontSize = size;
        tmp.fontStyle = title ? FontStyles.Bold : FontStyles.Normal;
    }

    private static Sprite CreateSpriteFromRasterAsset(Dc2WebImageAsset asset)
    {
        byte[] data = asset.BinaryData;
        if ((data == null || data.Length == 0) && !string.IsNullOrWhiteSpace(asset.FilePath) && File.Exists(asset.FilePath))
            data = File.ReadAllBytes(asset.FilePath);

        if (data == null || data.Length == 0)
            return null;

        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(tex, data))
            return null;

        tex.name = string.IsNullOrWhiteSpace(asset.Name) ? "dc2web_raster" : asset.Name;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite CreateSpriteFromSvgAsset(Dc2WebImageAsset asset, int width, int height)
    {
        string svg = asset.SvgMarkup;
        if (string.IsNullOrWhiteSpace(svg) && !string.IsNullOrWhiteSpace(asset.FilePath) && File.Exists(asset.FilePath))
            svg = File.ReadAllText(asset.FilePath);

        if (string.IsNullOrWhiteSpace(svg))
            return null;

        if (!TryParseSvgSize(svg, out int svgW, out int svgH))
        {
            svgW = width > 0 ? width : 256;
            svgH = height > 0 ? height : 256;
        }

        int w = Mathf.Max(16, width > 0 ? width : svgW);
        int h = Mathf.Max(16, height > 0 ? height : svgH);

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        Color fill = TryParseSvgFill(svg, out var parsed) ? parsed : Color.white;
        bool circle = svg.IndexOf("<circle", StringComparison.OrdinalIgnoreCase) >= 0;

        if (circle)
        {
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float r = Mathf.Min(w, h) * 0.42f;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    if (dx * dx + dy * dy <= r * r)
                        pixels[y * w + x] = fill;
                }
            }
        }
        else
        {
            int marginX = Mathf.RoundToInt(w * 0.12f);
            int marginY = Mathf.RoundToInt(h * 0.12f);
            for (int y = marginY; y < h - marginY; y++)
            {
                for (int x = marginX; x < w - marginX; x++)
                    pixels[y * w + x] = fill;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(false, false);
        tex.name = string.IsNullOrWhiteSpace(asset.Name) ? "dc2web_svg" : asset.Name;

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Dc2WebImageType GuessImageTypeFromPath(string path)
    {
        string ext = Path.GetExtension(path ?? string.Empty)?.ToLowerInvariant() ?? string.Empty;
        return ext switch
        {
            ".svg" => Dc2WebImageType.Svg,
            ".png" => Dc2WebImageType.Png,
            ".jpg" => Dc2WebImageType.Jpg,
            ".jpeg" => Dc2WebImageType.Jpeg,
            ".bmp" => Dc2WebImageType.Bmp,
            ".gif" => Dc2WebImageType.Gif,
            ".tga" => Dc2WebImageType.Tga,
            _ => Dc2WebImageType.Unknown,
        };
    }

    private static bool TryParseSvgSize(string svg, out int width, out int height)
    {
        width = 0;
        height = 0;

        var w = Regex.Match(svg, "width\\s*=\\s*\"(?<v>[0-9]+)", RegexOptions.IgnoreCase);
        var h = Regex.Match(svg, "height\\s*=\\s*\"(?<v>[0-9]+)", RegexOptions.IgnoreCase);
        if (w.Success && h.Success)
        {
            width = int.Parse(w.Groups["v"].Value);
            height = int.Parse(h.Groups["v"].Value);
            return true;
        }

        var vb = Regex.Match(svg, "viewBox\\s*=\\s*\"[0-9\\.\\-]+\\s+[0-9\\.\\-]+\\s+(?<w>[0-9\\.]+)\\s+(?<h>[0-9\\.]+)\"", RegexOptions.IgnoreCase);
        if (vb.Success)
        {
            width = Mathf.RoundToInt(float.Parse(vb.Groups["w"].Value, System.Globalization.CultureInfo.InvariantCulture));
            height = Mathf.RoundToInt(float.Parse(vb.Groups["h"].Value, System.Globalization.CultureInfo.InvariantCulture));
            return width > 0 && height > 0;
        }

        return false;
    }

    private static bool TryParseSvgFill(string svg, out Color color)
    {
        color = Color.white;
        var m = Regex.Match(svg, "fill\\s*=\\s*\"(?<c>#[0-9a-fA-F]{6,8}|rgba?\\([^\\)]*\\))\"", RegexOptions.IgnoreCase);
        if (!m.Success)
            return false;

        return TryParseColor(m.Groups["c"].Value, out color);
    }

    private static bool IsPanelLike(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return name.IndexOf("panel", StringComparison.OrdinalIgnoreCase) >= 0
               || name.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0
               || name.IndexOf("window", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsTitleLike(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return name.IndexOf("title", StringComparison.OrdinalIgnoreCase) >= 0
               || name.IndexOf("header", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void TrySetColor(string value, Action<Color> setter)
    {
        if (TryParseColor(value, out Color c))
            setter(c);
    }

    private static bool TryParseColor(string value, out Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(value)) return false;
        string trimmed = value.Trim();

        if (ColorUtility.TryParseHtmlString(trimmed, out color))
            return true;

        var rgba = Regex.Match(trimmed, @"rgba?\((?<r>\d+),(?<g>\d+),(?<b>\d+)(,(?<a>[0-9.]+))?\)", RegexOptions.IgnoreCase);
        if (rgba.Success)
        {
            float r = Mathf.Clamp(int.Parse(rgba.Groups["r"].Value) / 255f, 0f, 1f);
            float g = Mathf.Clamp(int.Parse(rgba.Groups["g"].Value) / 255f, 0f, 1f);
            float b = Mathf.Clamp(int.Parse(rgba.Groups["b"].Value) / 255f, 0f, 1f);
            float a = rgba.Groups["a"].Success ? Mathf.Clamp(float.Parse(rgba.Groups["a"].Value, System.Globalization.CultureInfo.InvariantCulture), 0f, 1f) : 1f;
            color = new Color(r, g, b, a);
            return true;
        }

        return false;
    }

    private static bool TryParsePx(string value, out float px)
    {
        px = 0f;
        var m = Regex.Match(value ?? string.Empty, @"(?<v>[0-9]+(\.[0-9]+)?)px", RegexOptions.IgnoreCase);
        if (!m.Success) return false;
        return float.TryParse(m.Groups["v"].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out px);
    }

    private static void TryExtractGoogleFontImport(string css, UnityUiStyleProfile profile)
    {
        if (string.IsNullOrWhiteSpace(css) || profile == null)
            return;

        var importMatch = Regex.Match(css, "@import\\s+url\\((?<u>[^\\)]+)\\)", RegexOptions.IgnoreCase);
        if (!importMatch.Success)
            return;

        string importUrl = importMatch.Groups["u"].Value.Trim().Trim('"', '\'');
        int familyIndex = importUrl.IndexOf("family=", StringComparison.OrdinalIgnoreCase);
        if (familyIndex < 0)
            return;

        string familyPart = importUrl[(familyIndex + 7)..];
        int ampIndex = familyPart.IndexOf('&');
        if (ampIndex >= 0)
            familyPart = familyPart[..ampIndex];

        string family = familyPart.Replace('+', ' ');
        int variantIndex = family.IndexOf(':');
        if (variantIndex >= 0)
            family = family[..variantIndex];

        profile.PreferredGoogleFontFamily = family.Trim();
    }

    private static string ExtractPrimaryFontFamily(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string trimmed = value.Trim();
        int comma = trimmed.IndexOf(',');
        if (comma >= 0)
            trimmed = trimmed[..comma];

        return trimmed.Trim().Trim('"', '\'');
    }

    private static TMP_FontAsset ResolvePreferredFont(UnityUiStyleProfile profile)
    {
        if (profile == null)
            return null;

        string[] candidates =
        {
            profile.PreferredLocalFontAsset,
            profile.PreferredGoogleFontFamily,
            profile.PreferredFontFamily,
        };

        foreach (string candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            if (FontCache.TryGetValue(candidate, out var cached) && cached != null)
                return cached;

            TMP_FontAsset resolved = ResolveFontByName(candidate);
            if (resolved != null)
            {
                FontCache[candidate] = resolved;
                return resolved;
            }
        }

        return null;
    }

    private static TMP_FontAsset ResolveFontByName(string name)
    {
        string query = name.Trim();
        if (string.IsNullOrWhiteSpace(query))
            return null;

        var allTmpFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        if (allTmpFonts != null)
        {
            foreach (var tmp in allTmpFonts)
            {
                if (tmp == null || string.IsNullOrWhiteSpace(tmp.name))
                    continue;

                if (string.Equals(tmp.name, query, StringComparison.OrdinalIgnoreCase)
                    || tmp.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    return tmp;
            }
        }

        TMP_FontAsset resourceFont = Resources.Load<TMP_FontAsset>(query);
        if (resourceFont != null)
            return resourceFont;

        try
        {
            Font osFont = Font.CreateDynamicFontFromOSFont(query, 24);
            if (osFont != null)
            {
                TMP_FontAsset generated = TMP_FontAsset.CreateFontAsset(osFont);
                if (generated != null)
                {
                    generated.name = $"DC2WEB_{query}";
                    return generated;
                }
            }
        }
        catch
        {
        }

        return null;
    }

    private static string ExtractHtmlTitle(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var m = Regex.Match(html, "<h1[^>]*>(?<t>.*?)</h1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!m.Success) return string.Empty;
        return StripTags(m.Groups["t"].Value).Trim();
    }

    private static string ExtractHtmlBody(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var m = Regex.Match(html, "<p[^>]*>(?<t>.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!m.Success) return string.Empty;
        return StripTags(m.Groups["t"].Value).Trim();
    }

    private static string StripTags(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return Regex.Replace(text, "<.*?>", string.Empty);
    }

    private sealed class TailwindAdapter : IDc2WebFrameworkAdapter
    {
        public bool CanHandle(Dc2WebSource source) => source.Type == Dc2WebSourceType.TailwindCss;

        public string TranslateToCss(Dc2WebSource source)
        {
            string css = string.Empty;
            foreach (var token in source.Content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                switch (token.Trim())
                {
                    case "bg-slate-900": css += "--panel-color:#0f172a;"; break;
                    case "bg-slate-800": css += "--panel-color:#1e293b;"; break;
                    case "text-white": css += "--text-color:#ffffff;"; break;
                    case "text-slate-100": css += "--text-color:#f1f5f9;"; break;
                    case "text-slate-300": css += "--secondary-text-color:#cbd5e1;"; break;
                    case "text-sm": css += "font-size:14px;"; break;
                    case "text-base": css += "font-size:16px;"; break;
                    case "text-lg": css += "font-size:18px;"; break;
                    case "text-xl": css += "font-size:20px;"; break;
                    case "text-2xl": css += "font-size:24px;"; break;
                    case "text-3xl": css += "font-size:30px;"; break;
                    case "text-sky-400": css += "--accent:#38bdf8;"; break;
                }
            }
            return ":root{" + css + "}";
        }
    }

    private sealed class SassAdapter : IDc2WebFrameworkAdapter
    {
        public bool CanHandle(Dc2WebSource source)
            => source.Type == Dc2WebSourceType.Sass || source.Type == Dc2WebSourceType.Scss;

        public string TranslateToCss(Dc2WebSource source)
        {
            var lines = source.Content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var vars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var output = new List<string>();

            foreach (var raw in lines)
            {
                string line = raw.Trim();
                if (line.StartsWith("$", StringComparison.Ordinal) && line.Contains(":"))
                {
                    int idx = line.IndexOf(':');
                    string key = line.Substring(1, idx - 1).Trim();
                    string val = line[(idx + 1)..].Trim().TrimEnd(';');
                    vars[key] = val;
                    continue;
                }

                foreach (var kv in vars)
                    line = line.Replace("$" + kv.Key, kv.Value);

                output.Add(line);
            }

            return string.Join("\n", output);
        }
    }

    private sealed class JsTsAdapter : IDc2WebFrameworkAdapter
    {
        public bool CanHandle(Dc2WebSource source)
            => source.Type == Dc2WebSourceType.JavaScript || source.Type == Dc2WebSourceType.TypeScript;

        public string TranslateToCss(Dc2WebSource source)
        {
            string content = source.Content ?? string.Empty;
            string css = ":root{";

            Match bg = Regex.Match(content, "backgroundColor\\s*[:=]\\s*['\"](?<v>#[0-9a-fA-F]{6,8}|rgba?\\([^\\)]*\\))['\"]");
            if (bg.Success) css += "--panel-color:" + bg.Groups["v"].Value + ";";

            Match fg = Regex.Match(content, "color\\s*[:=]\\s*['\"](?<v>#[0-9a-fA-F]{6,8}|rgba?\\([^\\)]*\\))['\"]");
            if (fg.Success) css += "--text-color:" + fg.Groups["v"].Value + ";";

            Match btn = Regex.Match(content, "buttonColor\\s*[:=]\\s*['\"](?<v>#[0-9a-fA-F]{6,8}|rgba?\\([^\\)]*\\))['\"]");
            if (btn.Success) css += "--btn-normal:" + btn.Groups["v"].Value + ";";

            Match accent = Regex.Match(content, "accentColor\\s*[:=]\\s*['\"](?<v>#[0-9a-fA-F]{6,8}|rgba?\\([^\\)]*\\))['\"]");
            if (accent.Success) css += "--accent:" + accent.Groups["v"].Value + ";";

            Match fs = Regex.Match(content, "fontSize\\s*[:=]\\s*(?<v>[0-9]+)");
            if (fs.Success) css += "font-size:" + fs.Groups["v"].Value + "px;";

            css += "}";
            return css;
        }
    }

    private sealed class ReactAdapter : IDc2WebFrameworkAdapter
    {
        public bool CanHandle(Dc2WebSource source)
        {
            if (source == null) return false;
            if (source.Type == Dc2WebSourceType.ReactJsx || source.Type == Dc2WebSourceType.ReactTsx)
                return true;

            return source.Framework.IndexOf("react", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public string TranslateToCss(Dc2WebSource source)
        {
            string content = source.Content ?? string.Empty;
            string css = ":root{";

            Match className = Regex.Match(content, "className\\s*=\\s*[\"'](?<v>[^\"']+)[\"']", RegexOptions.IgnoreCase);
            if (className.Success)
            {
                string[] tokens = className.Groups["v"].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    switch (token)
                    {
                        case "bg-slate-900": css += "--panel-color:#0f172a;"; break;
                        case "text-slate-100": css += "--text-color:#f1f5f9;"; break;
                        case "text-slate-300": css += "--secondary-text-color:#cbd5e1;"; break;
                        case "text-2xl": css += "font-size:24px;"; break;
                        case "text-3xl": css += "font-size:30px;"; break;
                    }
                }
            }

            Match styleBg = Regex.Match(content, "backgroundColor\\s*:\\s*[\"'](?<v>#[0-9a-fA-F]{6,8}|rgba?\\([^\\)]*\\))[\"']", RegexOptions.IgnoreCase);
            if (styleBg.Success)
                css += "--panel-color:" + styleBg.Groups["v"].Value + ";";

            Match styleColor = Regex.Match(content, "color\\s*:\\s*[\"'](?<v>#[0-9a-fA-F]{6,8}|rgba?\\([^\\)]*\\))[\"']", RegexOptions.IgnoreCase);
            if (styleColor.Success)
                css += "--text-color:" + styleColor.Groups["v"].Value + ";";

            css += "}";
            return css;
        }
    }
}
