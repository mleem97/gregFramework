using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(FMF.HexLabelMod.HexLabelMelon), "FMF HexLabel Mod", "00.01.0008", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FMF.HexLabelMod;

public sealed class HexLabelMelon : MelonMod
{
    private const string AllowedSteamId64 = "76561198032682009";
    private const string LabelObjectName = "HexLabel_White";
    private const string RackLabelObjectName = "RackHexLabel_White";
    private static HexPositionConfig _config = HexPositionConfig.CreateDefault();

    private HarmonyLib.Harmony _harmony;
    private float _scanTimer;
    private float _configReloadTimer;
    private float _steamPermissionRecheckTimer;
    private float _startupWaitTimer;
    private int _configReloadRunning;
    private bool _isFullyInitialized;
    private bool _startupWaitMessageShown;
    private bool _liveReloadEnabled;
    private bool _liveReloadAllowed;
    private bool _frameworkAvailable;

    public override void OnInitializeMelon()
    {
        _frameworkAvailable = IsFrameworkLoaded();
        if (!_frameworkAvailable)
        {
            LoggerInstance.Error("FMF HexLabel Mod requires FrikaModdingFramework. Load `FrikaModdingFramework.dll` first.");
            return;
        }

        _config = HexPositionConfig.CreateDefault();
        _isFullyInitialized = false;
        _liveReloadEnabled = false;
        _liveReloadAllowed = false;

        LoggerInstance.Msg("HexLabelMod waiting for Steam runtime initialization...");
    }

    private void TryInitializeAfterSteamReady()
    {
        if (!IsSteamRuntimeReady())
        {
            if (!_startupWaitMessageShown)
            {
                LoggerInstance.Msg("HexLabelMod deferred: waiting for Steam runtime marker/SteamID in Latest.log.");
                _startupWaitMessageShown = true;
            }

            return;
        }

        _config = LoadOrCreateConfig();
        EnsureConfigFileExists();
        TryResolveLiveReloadPermission();

        _harmony = new HarmonyLib.Harmony("de.mleem.hexlabelmod");
        _harmony.PatchAll(typeof(HexLabelMelon).Assembly);
        _isFullyInitialized = true;

        LoggerInstance.Msg($"HexLabelMod initialized. Config: {GetConfigPath()}");

        if (_liveReloadAllowed)
        {
            LoggerInstance.Msg("Live reload available. Toggle with Ctrl+F1.");
        }
        else
        {
            LoggerInstance.Msg("POWERED BY FRIKADELLE MODDING FRAMEWORK - DONE CHECKING STEAMID FOR PERMISSION");
        }
    }

    public override void OnUpdate()
    {
        if (!_frameworkAvailable)
            return;

        if (!_isFullyInitialized)
        {
            _startupWaitTimer += Time.deltaTime;
            if (_startupWaitTimer >= 1f)
            {
                _startupWaitTimer = 0f;
                TryInitializeAfterSteamReady();
            }

            return;
        }

        _scanTimer += Time.deltaTime;

        if (!_liveReloadAllowed)
        {
            _steamPermissionRecheckTimer += Time.deltaTime;
            if (_steamPermissionRecheckTimer >= 5f)
            {
                _steamPermissionRecheckTimer = 0f;
                TryResolveLiveReloadPermission();
            }
        }

        HandleLiveReloadToggleHotkey();

        if (_liveReloadAllowed && _liveReloadEnabled)
            _configReloadTimer += Time.deltaTime;

        if (_liveReloadAllowed && _liveReloadEnabled && _configReloadTimer >= 6f)
        {
            _configReloadTimer = 0f;
            _ = ReloadConfigAsync();
        }

        if (_scanTimer < 1.5f)
            return;

        _scanTimer = 0f;
        TryApplyToAllSpinners();
    }

    private static bool IsFrameworkLoaded()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return assemblies.Any(assembly =>
        {
            var name = assembly.GetName().Name ?? string.Empty;
            return string.Equals(name, "FrikaModdingFramework", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "FrikaMF", StringComparison.OrdinalIgnoreCase);
        });
    }

    private void HandleLiveReloadToggleHotkey()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        var ctrlPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
        if (!ctrlPressed || !keyboard.f1Key.wasPressedThisFrame)
            return;

        if (!_liveReloadAllowed)
            TryResolveLiveReloadPermission();

        if (!_liveReloadAllowed)
        {
            LoggerInstance.Warning("Live reload is not allowed for this Steam account.");
            return;
        }

        _liveReloadEnabled = !_liveReloadEnabled;
        _configReloadTimer = 0f;

        LoggerInstance.Msg(_liveReloadEnabled
            ? "Live reload enabled (6s interval)."
            : "Live reload disabled.");
    }

    private async Task ReloadConfigAsync()
    {
        if (Interlocked.Exchange(ref _configReloadRunning, 1) == 1)
            return;

        try
        {
            var updated = await Task.Run(LoadOrCreateConfig);
            if (updated != null)
                _config = updated;
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"Config live-reload failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _configReloadRunning, 0);
        }
    }

    internal static void EnsureLabel(CableSpinner spinner)
    {
        if (spinner == null)
            return;

        if (!TryGetSpinnerHex(spinner, out var hex))
            return;

        var sourceLabel = spinner.txtLength;
        if (sourceLabel == null)
            return;

        var parent = sourceLabel.transform != null ? sourceLabel.transform.parent : spinner.transform;
        if (parent == null)
            return;

        TextMeshProUGUI label = null;
        var existing = parent.Find(LabelObjectName);
        if (existing != null)
            label = existing.GetComponent<TextMeshProUGUI>();

        if (label == null)
        {
            var clone = UnityEngine.Object.Instantiate(sourceLabel.gameObject, parent);
            clone.name = LabelObjectName;
            label = clone.GetComponent<TextMeshProUGUI>();
        }

        if (label == null)
            return;

        label.color = Color.white;
        label.alpha = 1f;
        label.text = hex;
        label.enableAutoSizing = true;
        label.fontSizeMin = _config.SpinnerFontMin;
        label.fontSizeMax = _config.SpinnerFontMax;
        label.fontSize = Mathf.Clamp(sourceLabel.fontSize * _config.SpinnerFontScale, _config.SpinnerFontMin, _config.SpinnerFontMax);
        label.enableWordWrapping = false;
        label.alignment = TextAlignmentOptions.Center;

        var rt = label.rectTransform;
        if (rt != null)
            rt.anchoredPosition = sourceLabel.rectTransform.anchoredPosition + new Vector2(_config.SpinnerOffsetX, _config.SpinnerOffsetY);
    }

    private void TryApplyToAllSpinners()
    {
        try
        {
            var spinners = UnityEngine.Object.FindObjectsOfType<CableSpinner>();
            if (spinners == null)
                return;

            for (var i = 0; i < spinners.Count; i++)
                EnsureLabel(spinners[i]);

            var racks = UnityEngine.Object.FindObjectsOfType<Rack>();
            if (racks == null)
                return;

            for (var i = 0; i < racks.Count; i++)
                EnsureRackLabel(racks[i]);
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"HexLabel scan failed: {ex.Message}");
        }
    }

    private static bool TryGetSpinnerHex(CableSpinner spinner, out string hex)
    {
        hex = null;

        var raw = spinner.rgbColor;
        if (!string.IsNullOrWhiteSpace(raw) && TryNormalizeHex(raw, out hex))
            return true;

        var material = spinner.cableMaterial;
        if (material == null)
            return false;

        try
        {
            if (material.HasProperty("_BaseColor"))
            {
                hex = ToHex(material.GetColor("_BaseColor"));
                return true;
            }

            if (material.HasProperty("_Color"))
            {
                hex = ToHex(material.color);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool TryNormalizeHex(string raw, out string hex)
    {
        hex = null;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var s = raw.Trim();

        if (s.StartsWith("#", StringComparison.Ordinal))
        {
            var h = s.ToUpperInvariant();
            if (h.Length == 7)
            {
                hex = h;
                return true;
            }

            if (h.Length == 9)
            {
                hex = "#" + h.Substring(3, 6);
                return true;
            }
        }

        if (ColorUtility.TryParseHtmlString(s, out var colorFromHtml))
        {
            hex = ToHex(colorFromHtml);
            return true;
        }

        var parts = s.Split(',');
        if (parts.Length == 3
            && TryParseColorPart(parts[0], out var r)
            && TryParseColorPart(parts[1], out var g)
            && TryParseColorPart(parts[2], out var b))
        {
            hex = $"#{r:X2}{g:X2}{b:X2}";
            return true;
        }

        return false;
    }

    private static bool TryParseColorPart(string value, out int parsed)
    {
        parsed = 0;
        var token = value.Trim();
        if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
        {
            if (f <= 1f)
            {
                parsed = Mathf.Clamp(Mathf.RoundToInt(f * 255f), 0, 255);
                return true;
            }

            parsed = Mathf.Clamp(Mathf.RoundToInt(f), 0, 255);
            return true;
        }

        return false;
    }

    private static string ToHex(Color c)
    {
        var r = Mathf.Clamp(Mathf.RoundToInt(c.r * 255f), 0, 255);
        var g = Mathf.Clamp(Mathf.RoundToInt(c.g * 255f), 0, 255);
        var b = Mathf.Clamp(Mathf.RoundToInt(c.b * 255f), 0, 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    internal static void EnsureRackLabel(Rack rack)
    {
        if (rack == null)
            return;

        if (!TryGetRackHex(rack, out var hex))
            hex = "#FFFFFF";

        var root = rack.transform;
        if (root == null)
            return;

        var existing = root.Find(RackLabelObjectName);
        TextMesh label;

        if (existing != null)
        {
            label = existing.GetComponent<TextMesh>();
        }
        else
        {
            var go = new GameObject(RackLabelObjectName);
            go.transform.SetParent(root, true);
            label = go.AddComponent<TextMesh>();
        }

        if (label == null)
            return;

        label.text = hex;
        label.color = Color.white;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = _config.RackFontSize;
        label.characterSize = _config.RackCharacterSize;

        if (!TryGetRackBackRightBottomPosition(rack, out var worldPos))
            return;

        var t = label.transform;
        t.position = worldPos;
        t.rotation = Quaternion.LookRotation(-rack.transform.forward, rack.transform.up);
        t.localScale = Vector3.one * _config.RackScale;
    }

    private static bool TryGetRackHex(Rack rack, out string hex)
    {
        hex = null;

        try
        {
            var renderers = rack.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Count == 0)
                return false;

            for (var i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    continue;

                var mat = renderer.sharedMaterial;
                if (mat == null)
                    continue;

                if (mat.HasProperty("_BaseColor"))
                {
                    hex = ToHex(mat.GetColor("_BaseColor"));
                    return true;
                }

                if (mat.HasProperty("_Color"))
                {
                    hex = ToHex(mat.color);
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool TryGetRackBackRightBottomPosition(Rack rack, out Vector3 pos)
    {
        pos = default;

        try
        {
            var renderers = rack.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Count == 0)
                return false;

            var hasBounds = false;
            var bounds = new Bounds();

            for (var i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    continue;

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
                return false;

            var rightExtent = ProjectExtent(bounds.extents, rack.transform.right);
            var backExtent = ProjectExtent(bounds.extents, -rack.transform.forward);
            var downExtent = ProjectExtent(bounds.extents, -rack.transform.up);

            pos = bounds.center
                + rack.transform.right * (rightExtent + _config.RackOffsetRight)
                + (-rack.transform.forward) * (backExtent + _config.RackOffsetBack)
                + (-rack.transform.up) * (downExtent + _config.RackOffsetDown);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static float ProjectExtent(Vector3 extents, Vector3 axis)
    {
        var a = new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z));
        return extents.x * a.x + extents.y * a.y + extents.z * a.z;
    }

    private static HexPositionConfig LoadOrCreateConfig()
    {
        var path = GetConfigPath();
        var config = HexPositionConfig.CreateDefault();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);

        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(path))
            {
                File.WriteAllText(path, config.ToConfigText());
                return config;
            }

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var key = ApplyConfigLine(config, line);
                if (!string.IsNullOrWhiteSpace(key))
                    seenKeys.Add(key);
            }

            if (seenKeys.Count < HexPositionConfig.ExpectedKeys.Length)
                File.WriteAllText(path, config.ToConfigText());

            return config;
        }
        catch
        {
            return config;
        }
    }

    private static string ApplyConfigLine(HexPositionConfig config, string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var trimmed = line.Trim();
        if (trimmed.StartsWith("#", StringComparison.Ordinal))
            return null;

        var splitIndex = trimmed.IndexOf('=');
        if (splitIndex <= 0 || splitIndex >= trimmed.Length - 1)
            return null;

        var key = trimmed.Substring(0, splitIndex).Trim();
        var value = trimmed.Substring(splitIndex + 1).Trim();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            return null;

        switch (key)
        {
            case "spinner_offset_x":
                config.SpinnerOffsetX = parsed;
                return key;
            case "spinner_offset_y":
                config.SpinnerOffsetY = parsed;
                return key;
            case "spinner_font_min":
                config.SpinnerFontMin = parsed;
                return key;
            case "spinner_font_max":
                config.SpinnerFontMax = parsed;
                return key;
            case "spinner_font_scale":
                config.SpinnerFontScale = parsed;
                return key;
            case "rack_offset_right":
                config.RackOffsetRight = parsed;
                return key;
            case "rack_offset_back":
                config.RackOffsetBack = parsed;
                return key;
            case "rack_offset_down":
                config.RackOffsetDown = parsed;
                return key;
            case "rack_font_size":
                config.RackFontSize = Mathf.RoundToInt(parsed);
                return key;
            case "rack_character_size":
                config.RackCharacterSize = parsed;
                return key;
            case "rack_scale":
                config.RackScale = parsed;
                return key;
            default:
                return null;
        }
    }

    private static string GetConfigPath()
    {
        return Path.Combine(MelonEnvironment.UserDataDirectory, "hexposition.cfg");
    }

    private static string TryGetSteamId64FromMelonLoader()
    {
        try
        {
            var logPath = Path.Combine(MelonEnvironment.GameRootDirectory, "MelonLoader", "Latest.log");
            if (!File.Exists(logPath))
                return null;

            var content = File.ReadAllText(logPath);
            var match = Regex.Match(content, @"(?<!\d)7656119\d{10}(?!\d)");
            if (!match.Success)
                return null;

            return match.Value;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsSteamRuntimeReady()
    {
        try
        {
            var logPath = Path.Combine(MelonEnvironment.GameRootDirectory, "MelonLoader", "Latest.log");
            if (!File.Exists(logPath))
                return false;

            var content = File.ReadAllText(logPath);

            var hasSteamMarker = content.Contains("SteamInternal_SetMinidumpSteamID:", StringComparison.Ordinal)
                                 || content.Contains("Caching Steam ID:", StringComparison.Ordinal);

            var hasSteamId = Regex.IsMatch(content, @"(?<!\d)7656119\d{10}(?!\d)");

            return hasSteamMarker || hasSteamId;
        }
        catch
        {
            return false;
        }
    }

    private void TryResolveLiveReloadPermission()
    {
        var detectedSteamId = TryGetSteamId64FromMelonLoader()?.Trim();
        if (string.IsNullOrWhiteSpace(detectedSteamId))
            return;

        if (string.Equals(detectedSteamId, AllowedSteamId64, StringComparison.Ordinal))
        {
            if (!_liveReloadAllowed)
                LoggerInstance.Msg("Live reload permission granted for this Steam account.");

            _liveReloadAllowed = true;
        }
    }

    private static void EnsureConfigFileExists()
    {
        var path = GetConfigPath();
        if (File.Exists(path))
            return;

        var defaultConfig = HexPositionConfig.CreateDefault();
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, defaultConfig.ToConfigText());
    }

    private sealed class HexPositionConfig
    {
        public float SpinnerOffsetX;
        public float SpinnerOffsetY;
        public float SpinnerFontMin;
        public float SpinnerFontMax;
        public float SpinnerFontScale;

        public float RackOffsetRight;
        public float RackOffsetBack;
        public float RackOffsetDown;
        public int RackFontSize;
        public float RackCharacterSize;
        public float RackScale;

        public static readonly string[] ExpectedKeys =
        {
            "spinner_offset_x",
            "spinner_offset_y",
            "spinner_font_min",
            "spinner_font_max",
            "spinner_font_scale",
            "rack_offset_right",
            "rack_offset_back",
            "rack_offset_down",
            "rack_font_size",
            "rack_character_size",
            "rack_scale",
        };

        public static HexPositionConfig CreateDefault()
        {
            return new HexPositionConfig
            {
                SpinnerOffsetX = 0f,
                SpinnerOffsetY = -6f,
                SpinnerFontMin = 1.8f,
                SpinnerFontMax = 6.2f,
                SpinnerFontScale = 0.24f,

                RackOffsetRight = -0.03f,
                RackOffsetBack = 0.06f,
                RackOffsetDown = -0.02f,
                RackFontSize = 42,
                RackCharacterSize = 0.05f,
                RackScale = 1f,
            };
        }

        public string ToConfigText()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "# Hex Label Position Config",
                "# File: UserData/hexposition.cfg",
                "# Edit values, then restart game.",
                "",
                "# Spinner (UI text near cable spool)",
                $"spinner_offset_x={SpinnerOffsetX.ToString(CultureInfo.InvariantCulture)}",
                $"spinner_offset_y={SpinnerOffsetY.ToString(CultureInfo.InvariantCulture)}",
                $"spinner_font_min={SpinnerFontMin.ToString(CultureInfo.InvariantCulture)}",
                $"spinner_font_max={SpinnerFontMax.ToString(CultureInfo.InvariantCulture)}",
                $"spinner_font_scale={SpinnerFontScale.ToString(CultureInfo.InvariantCulture)}",
                "",
                "# Rack (world-space text at rack back-right-bottom)",
                $"rack_offset_right={RackOffsetRight.ToString(CultureInfo.InvariantCulture)}",
                $"rack_offset_back={RackOffsetBack.ToString(CultureInfo.InvariantCulture)}",
                $"rack_offset_down={RackOffsetDown.ToString(CultureInfo.InvariantCulture)}",
                $"rack_font_size={RackFontSize.ToString(CultureInfo.InvariantCulture)}",
                $"rack_character_size={RackCharacterSize.ToString(CultureInfo.InvariantCulture)}",
                $"rack_scale={RackScale.ToString(CultureInfo.InvariantCulture)}",
            });
        }
    }
}

[HarmonyPatch(typeof(CableSpinner), nameof(CableSpinner.Start))]
internal static class CableSpinnerStartPatch
{
    private static void Postfix(CableSpinner __instance)
    {
        HexLabelMelon.EnsureLabel(__instance);
    }
}
