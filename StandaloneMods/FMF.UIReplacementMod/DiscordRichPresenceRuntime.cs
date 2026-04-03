using System;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FMF.UIReplacementMod;

internal sealed class DiscordRichPresenceRuntime
{
    private readonly MelonLogger.Instance _logger;
    private readonly RuntimeOptions _options;

    private object _client;
    private Type _clientType;
    private Type _presenceType;
    private Type _timestampsType;

    private bool _initialized;
    private float _nextUpdateAt;

    public DiscordRichPresenceRuntime(MelonLogger.Instance logger, RuntimeOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public bool TryInitialize()
    {
        if (_initialized)
            return true;

        if (!_options.EnableDiscordRichPresence)
            return false;

        try
        {
            Assembly rpcAssembly = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int index = 0; index < assemblies.Length; index++)
            {
                string name = assemblies[index].GetName().Name ?? string.Empty;
                if (name.Equals("DiscordRPC", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("discord-rpc-csharp", StringComparison.OrdinalIgnoreCase))
                {
                    rpcAssembly = assemblies[index];
                    break;
                }
            }

            if (rpcAssembly == null)
            {
                _logger.Warning("[FMF.UIReplacement] DiscordRPC assembly not loaded. Rich Presence is optional and will be skipped.");
                return false;
            }

            _clientType = rpcAssembly.GetType("DiscordRPC.DiscordRpcClient", throwOnError: false);
            _presenceType = rpcAssembly.GetType("DiscordRPC.RichPresence", throwOnError: false);
            _timestampsType = rpcAssembly.GetType("DiscordRPC.Timestamps", throwOnError: false);

            if (_clientType == null || _presenceType == null)
            {
                _logger.Warning("[FMF.UIReplacement] DiscordRPC types unavailable. Skipping Rich Presence.");
                return false;
            }

            _client = Activator.CreateInstance(_clientType, _options.DiscordClientId);
            _clientType.GetMethod("Initialize")?.Invoke(_client, null);
            _initialized = true;
            _logger.Msg("[FMF.UIReplacement] Discord Rich Presence initialized.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Warning($"[FMF.UIReplacement] Discord init failed: {ex.GetBaseException().Message}");
            return false;
        }
    }

    public void UpdatePresenceIfDue(int players)
    {
        if (!_initialized)
            return;

        if (Time.unscaledTime < _nextUpdateAt)
            return;

        _nextUpdateAt = Time.unscaledTime + 15f;

        try
        {
            object presence = Activator.CreateInstance(_presenceType);
            SetProp(presence, "Details", "Frika Modern UI active");
            SetProp(presence, "State", $"Players configured: {players}");

            if (_timestampsType != null)
            {
                var nowMethod = _timestampsType.GetMethod("Now", BindingFlags.Public | BindingFlags.Static);
                if (nowMethod != null)
                {
                    object timestamps = nowMethod.Invoke(null, null);
                    SetProp(presence, "Timestamps", timestamps);
                }
            }

            string scene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrWhiteSpace(scene))
                SetProp(presence, "LargeImageText", $"Scene: {scene}");

            _clientType.GetMethod("SetPresence")?.Invoke(_client, new[] { presence });
        }
        catch (Exception ex)
        {
            _logger.Warning($"[FMF.UIReplacement] Discord update failed: {ex.GetBaseException().Message}");
        }
    }

    public void Shutdown()
    {
        if (!_initialized || _client == null)
            return;

        try
        {
            _clientType.GetMethod("ClearPresence")?.Invoke(_client, null);
            _clientType.GetMethod("Dispose")?.Invoke(_client, null);
        }
        catch
        {
        }

        _initialized = false;
        _client = null;
    }

    private static void SetProp(object target, string propertyName, object value)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop != null && prop.CanWrite)
            prop.SetValue(target, value);
    }
}
