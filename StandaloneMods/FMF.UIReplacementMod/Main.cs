using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(FMF.UIReplacementMod.Main), "FMF UI Replacement", "00.01.0009", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FMF.UIReplacementMod;

public sealed class Main : MelonMod
{
    private const string StateSnapshotFileName = "react-state.json";
    private static readonly Color WindowBackground = new(0.06f, 0.09f, 0.14f, 0.92f);
    private static readonly Color SurfaceBackground = new(0.10f, 0.14f, 0.21f, 0.95f);
    private static readonly Color PrimaryAction = new(0.18f, 0.44f, 0.78f, 0.98f);
    private static readonly Color PrimaryHighlight = new(0.23f, 0.53f, 0.90f, 1f);
    private static readonly Color PrimaryPressed = new(0.15f, 0.36f, 0.66f, 0.95f);
    private static readonly Color TextPrimary = new(0.93f, 0.96f, 0.99f, 1f);
    private static readonly Color TextSecondary = new(0.75f, 0.82f, 0.90f, 1f);
    private static readonly Color Accent = new(0.26f, 0.88f, 0.72f, 1f);

    private bool _frameworkReady;
    private bool _dependencyWarningShown;
    private bool _uiReplacementEnabled = true;
    private float _nextRefreshAt;
    private float _nextDependencyCheckAt;
    private int _lastAppliedGraphics;
    private ReactUiRuntime _reactRuntime;
    private DiscordRichPresenceRuntime _discordRuntime;
    private RuntimeOptions _options;
    private float _nextStateSnapshotAt;
    private string _lastStateSnapshot = string.Empty;

    private sealed class UiStateEnvelope
    {
        public string Type { get; set; } = "STATE_UPDATE";
        public UiStatePayload Payload { get; set; } = new();
    }

    private sealed class UiStatePayload
    {
        public string SceneName { get; set; } = "unknown";
        public int MaxPlayers { get; set; }
        public bool UiReplacementEnabled { get; set; }
        public bool AutoRefreshEnabled { get; set; }
        public float AutoRefreshIntervalSeconds { get; set; }
        public bool LiveReloadEnabled { get; set; }
        public bool BridgeReady { get; set; }
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");
    }

    public override void OnInitializeMelon()
    {
        _options = RuntimeOptions.LoadOrCreate(LoggerInstance);

        _reactRuntime = new ReactUiRuntime(LoggerInstance);
        _reactRuntime.LoadAssets();

        _discordRuntime = new DiscordRichPresenceRuntime(LoggerInstance, _options);
        _discordRuntime.TryInitialize();

        _frameworkReady = IsFrikaMfRuntimeLoaded();
        if (!_frameworkReady)
        {
            LoggerInstance.Warning("FrikaMF runtime not ready yet. Waiting for runtime initialization...");
            _dependencyWarningShown = true;
            _nextDependencyCheckAt = Time.unscaledTime + 1.0f;
            return;
        }

        _reactRuntime.TryAttachBridge(logFailures: true);
        _reactRuntime.RegisterProfiles();

        LoggerInstance.Msg("FrikaMF runtime detected. UI replacement is active.");
        LoggerInstance.Msg("Hotkeys: Ctrl+U toggle replacement | Ctrl+Shift+U force refresh");
        LoggerInstance.Msg($"Configured max players: {_options.MaxPlayers}");
        LoggerInstance.Msg($"Auto refresh pass: {(_options.EnableAutoRefreshPass ? "enabled" : "disabled")} ({_options.AutoRefreshIntervalSeconds:0.#}s)");
        LoggerInstance.Msg($"UI asset root: {_reactRuntime.GetActiveAssetDirectory()}");

        WriteStateSnapshot(force: true);
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (!_frameworkReady || !_uiReplacementEnabled)
            return;

        if (_reactRuntime != null)
        {
            _reactRuntime.TryAttachBridge(logFailures: false);
            _reactRuntime.RegisterProfiles();
            _reactRuntime.ApplyToAllCanvases(forceReset: true);
        }

        _lastAppliedGraphics = ApplyUiReplacement();
        LoggerInstance.Msg($"Scene '{sceneName}' styled. Updated graphics: {_lastAppliedGraphics}");
        WriteStateSnapshot(force: true);
    }

    public override void OnUpdate()
    {
        if (!_frameworkReady)
        {
            TryResolveDependencyAtRuntime();
            return;
        }

        HandleHotkeys();

        _discordRuntime?.UpdatePresenceIfDue(_options?.MaxPlayers ?? 16);

        if (Time.unscaledTime >= _nextStateSnapshotAt)
        {
            _nextStateSnapshotAt = Time.unscaledTime + 2.5f;
            WriteStateSnapshot(force: false);
        }

        if (!_uiReplacementEnabled)
            return;

        if ((_options?.EnableLiveUiReload ?? true) && _reactRuntime != null && _reactRuntime.ConsumeLiveReloadRequest())
        {
            _reactRuntime.LoadAssets();
            _reactRuntime.RegisterProfiles();
            _reactRuntime.ApplyToAllCanvases(forceReset: true);
            _lastAppliedGraphics = ApplyUiReplacement();
            LoggerInstance.Msg("[FMF.UIReplacement] Live UI reload applied.");
            WriteStateSnapshot(force: true);
        }

        if (!(_options?.EnableAutoRefreshPass ?? false))
            return;

        if (Time.unscaledTime < _nextRefreshAt)
            return;

        _nextRefreshAt = Time.unscaledTime + Math.Max(2f, _options?.AutoRefreshIntervalSeconds ?? 10f);

        if (_reactRuntime != null)
        {
            _reactRuntime.TryAttachBridge(logFailures: false);
            _reactRuntime.ApplyToAllCanvases(forceReset: false);
        }

        _lastAppliedGraphics = ApplyUiReplacement();
    }

    private void TryResolveDependencyAtRuntime()
    {
        if (Time.unscaledTime < _nextDependencyCheckAt)
            return;

        _nextDependencyCheckAt = Time.unscaledTime + 1.5f;

        _frameworkReady = IsFrikaMfRuntimeLoaded();
        if (!_frameworkReady)
            return;

        _reactRuntime?.TryAttachBridge(logFailures: true);
        _reactRuntime?.RegisterProfiles();

        LoggerInstance.Msg("FrikaMF runtime detected at runtime. UI replacement is now active.");

        if (_dependencyWarningShown)
            LoggerInstance.Msg("Dependency wait resolved successfully.");

        _dependencyWarningShown = false;
        _lastAppliedGraphics = ApplyUiReplacement();
        WriteStateSnapshot(force: true);
    }

    public override void OnGUI()
    {
        if (!_frameworkReady)
            return;

        const float width = 520f;
        const float height = 30f;
        var boxRect = new Rect(10f, Screen.height - height - 10f, width, height);
        GUI.Box(boxRect, $"FMF UI Replacement: {(_uiReplacementEnabled ? "ON" : "OFF")} | Last pass: {_lastAppliedGraphics} graphics | MaxPlayers: {_options?.MaxPlayers}");
    }

    private void HandleHotkeys()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        bool ctrlPressed = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
        if (!ctrlPressed)
            return;

        if (keyboard.uKey.wasPressedThisFrame && !keyboard.leftShiftKey.isPressed && !keyboard.rightShiftKey.isPressed)
        {
            _uiReplacementEnabled = !_uiReplacementEnabled;
            LoggerInstance.Msg(_uiReplacementEnabled ? "UI replacement enabled." : "UI replacement disabled.");
            return;
        }

        bool shiftPressed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        if (shiftPressed && keyboard.uKey.wasPressedThisFrame)
        {
            _reactRuntime?.LoadAssets();
            _reactRuntime?.RegisterProfiles();
            _reactRuntime?.ApplyToAllCanvases(forceReset: true);
            _lastAppliedGraphics = ApplyUiReplacement();
            LoggerInstance.Msg($"Forced UI refresh completed. Updated graphics: {_lastAppliedGraphics}");
        }
    }

    public override void OnApplicationQuit()
    {
        _discordRuntime?.Shutdown();
    }

    private static bool IsFrikaMfRuntimeLoaded()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int index = 0; index < assemblies.Length; index++)
        {
            var coreType = assemblies[index].GetType("DataCenterModLoader.Core", throwOnError: false);
            if (coreType != null)
                return true;
        }

        return false;
    }

    private int ApplyUiReplacement()
    {
        int touched = 0;

        try
        {
            var graphics = UnityEngine.Object.FindObjectsOfType<Graphic>();
            if (graphics != null)
            {
                for (int index = 0; index < graphics.Count; index++)
                {
                    var graphic = graphics[index];
                    if (graphic == null)
                        continue;

                    if (ApplyGraphicTheme(graphic))
                        touched++;
                }
            }

            var buttons = UnityEngine.Object.FindObjectsOfType<Button>();
            if (buttons != null)
            {
                for (int index = 0; index < buttons.Count; index++)
                {
                    var button = buttons[index];
                    if (button == null)
                        continue;

                    ApplyButtonTheme(button);
                }
            }

            var inputFields = UnityEngine.Object.FindObjectsOfType<InputField>();
            if (inputFields != null)
            {
                for (int index = 0; index < inputFields.Count; index++)
                    ApplyInputTheme(inputFields[index]);
            }

            var tmpInputFields = UnityEngine.Object.FindObjectsOfType<TMP_InputField>();
            if (tmpInputFields != null)
            {
                for (int index = 0; index < tmpInputFields.Count; index++)
                    ApplyTmpInputTheme(tmpInputFields[index]);
            }
        }
        catch (Exception ex)
        {
            LoggerInstance.Error($"UI replacement failed: {ex.Message}");
        }

        return touched;
    }

    private static bool ApplyGraphicTheme(Graphic graphic)
    {
        if (graphic == null)
            return false;

        string objectName = graphic.name?.ToLowerInvariant() ?? string.Empty;

        if (graphic.TryCast<Text>() is Text legacyText && legacyText != null)
        {
            legacyText.color = NameLooksSecondaryText(objectName) ? TextSecondary : TextPrimary;
            return true;
        }

        if (graphic.TryCast<TextMeshProUGUI>() is TextMeshProUGUI tmpText && tmpText != null)
        {
            tmpText.color = NameLooksSecondaryText(objectName) ? TextSecondary : TextPrimary;
            return true;
        }

        if (graphic.TryCast<Image>() is Image image && image != null)
        {
            image.color = DetermineImageColor(objectName);
            return true;
        }

        graphic.color = TextPrimary;
        return true;
    }

    private static void ApplyButtonTheme(Button button)
    {
        if (button == null)
            return;

        var colors = button.colors;
        colors.normalColor = PrimaryAction;
        colors.highlightedColor = PrimaryHighlight;
        colors.pressedColor = PrimaryPressed;
        colors.selectedColor = PrimaryHighlight;
        colors.disabledColor = new Color(0.22f, 0.27f, 0.35f, 0.65f);
        colors.colorMultiplier = 1f;
        button.colors = colors;

        if (button.targetGraphic != null)
            button.targetGraphic.color = PrimaryAction;
    }

    private static void ApplyInputTheme(InputField input)
    {
        if (input == null)
            return;

        if (input.image != null)
            input.image.color = SurfaceBackground;

        if (input.textComponent != null)
            input.textComponent.color = TextPrimary;

        if (input.placeholder != null)
            input.placeholder.color = TextSecondary;
    }

    private static void ApplyTmpInputTheme(TMP_InputField input)
    {
        if (input == null)
            return;

        if (input.textViewport != null)
        {
            var viewportImage = input.textViewport.GetComponent<Image>();
            if (viewportImage != null)
                viewportImage.color = SurfaceBackground;
        }

        if (input.textComponent != null)
            input.textComponent.color = TextPrimary;

        if (input.placeholder != null)
            input.placeholder.color = TextSecondary;
    }

    private static bool NameLooksSecondaryText(string objectName)
    {
        return objectName.Contains("sub")
            || objectName.Contains("desc")
            || objectName.Contains("hint")
            || objectName.Contains("secondary")
            || objectName.Contains("footer");
    }

    private static Color DetermineImageColor(string objectName)
    {
        if (objectName.Contains("button") || objectName.Contains("btn"))
            return PrimaryAction;

        if (objectName.Contains("accent") || objectName.Contains("selected") || objectName.Contains("active"))
            return Accent;

        if (objectName.Contains("panel") || objectName.Contains("window") || objectName.Contains("popup") || objectName.Contains("background"))
            return WindowBackground;

        return SurfaceBackground;
    }

    private void WriteStateSnapshot(bool force)
    {
        try
        {
            if (_reactRuntime == null)
                return;

            string assetDir = _reactRuntime.GetActiveAssetDirectory();
            if (string.IsNullOrWhiteSpace(assetDir))
                return;

            Directory.CreateDirectory(assetDir);

            string sceneName = "unknown";
            try
            {
                sceneName = SceneManager.GetActiveScene().name;
            }
            catch
            {
            }

            var envelope = new UiStateEnvelope
            {
                Payload = new UiStatePayload
                {
                    SceneName = sceneName,
                    MaxPlayers = _options?.MaxPlayers ?? 16,
                    UiReplacementEnabled = _uiReplacementEnabled,
                    AutoRefreshEnabled = _options?.EnableAutoRefreshPass ?? false,
                    AutoRefreshIntervalSeconds = _options?.AutoRefreshIntervalSeconds ?? 10f,
                    LiveReloadEnabled = _options?.EnableLiveUiReload ?? true,
                    BridgeReady = _reactRuntime.IsReady,
                    Timestamp = DateTime.UtcNow.ToString("O"),
                },
            };

            string json = JsonSerializer.Serialize(envelope, new JsonSerializerOptions { WriteIndented = true });
            if (!force && string.Equals(json, _lastStateSnapshot, StringComparison.Ordinal))
                return;

            string statePath = Path.Combine(assetDir, StateSnapshotFileName);
            File.WriteAllText(statePath, json);
            _lastStateSnapshot = json;
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"[FMF.UIReplacement] Failed to write state snapshot: {ex.Message}");
        }
    }
}
