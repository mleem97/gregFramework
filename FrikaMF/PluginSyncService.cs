using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using MelonLoader;
using MelonLoader.Utils;

namespace DataCenterModLoader;

public sealed class PluginSyncService
{
    private readonly MelonLogger.Instance _logger;
    private PluginSyncConfig _config;
    private float _nextPollAt;
    private bool _initialized;

    private static readonly HttpClient HttpClient = new();

    public PluginSyncService(MelonLogger.Instance logger)
    {
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            _config = PluginSyncConfig.Load();
            _initialized = true;
            _nextPollAt = 0f;

            if (_config.Enabled)
                _logger.Msg($"[PluginSync] Enabled. Server={_config.ServerUrl}, Mode={_config.InstallMode}");
            else
                _logger.Msg("[PluginSync] Disabled in config.");
        }
        catch (Exception ex)
        {
            CrashLog.LogException("PluginSyncService.Initialize", ex);
        }
    }

    public void Tick(float now)
    {
        if (!_initialized || _config == null || !_config.Enabled)
            return;

        if (now < _nextPollAt)
            return;

        _nextPollAt = now + Math.Max(5f, _config.PollSeconds);
        _ = PollAndSyncAsync();
    }

    private async Task PollAndSyncAsync()
    {
        try
        {
            var manifest = await LoadManifestAsync();

            if (manifest?.Plugins == null)
                return;

            string targetRoot = Path.Combine(MelonEnvironment.ModsDirectory, "PluginSync", _config.InstallMode.Equals("permanent", StringComparison.OrdinalIgnoreCase) ? "Permanent" : "Temporary");
            Directory.CreateDirectory(targetRoot);

            foreach (var plugin in manifest.Plugins)
            {
                if (plugin == null || string.IsNullOrWhiteSpace(plugin.Name) || string.IsNullOrWhiteSpace(plugin.Url))
                    continue;

                string targetPath = Path.Combine(targetRoot, plugin.Name);
                string tempPath = targetPath + ".download";
                string backupPath = targetPath + ".bak";

                _logger.Msg($"[PluginSync] Downloading {plugin.Name}...");
                using (var stream = await HttpClient.GetStreamAsync(plugin.Url))
                using (var fileStream = File.Create(tempPath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                if (!ValidateSha256(plugin.Sha256, tempPath))
                {
                    CrashLog.LogError("pluginsync", $"SHA256 mismatch for {plugin.Name}. Download rejected.");
                    TryDelete(tempPath);
                    continue;
                }

                bool hadExisting = File.Exists(targetPath);
                try
                {
                    if (hadExisting)
                        File.Copy(targetPath, backupPath, overwrite: true);

                    File.Copy(tempPath, targetPath, overwrite: true);
                    TryDelete(tempPath);

                    if (!ValidateSha256(plugin.Sha256, targetPath))
                        throw new IOException($"Post-install hash validation failed for {plugin.Name}.");

                    TryDelete(backupPath);
                    _logger.Msg($"[PluginSync] Installed {plugin.Name} ({_config.InstallMode}).");
                }
                catch (Exception ex)
                {
                    CrashLog.LogException($"PluginSync install rollback ({plugin.Name})", ex);
                    TryDelete(targetPath);

                    if (hadExisting && File.Exists(backupPath))
                    {
                        File.Copy(backupPath, targetPath, overwrite: true);
                        CrashLog.Log($"[PluginSync] Rollback restored previous file for {plugin.Name}.");
                    }

                    TryDelete(tempPath);
                    TryDelete(backupPath);
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException("PluginSyncService.PollAndSyncAsync", ex);
            CrashLog.LogError("pluginsync", "Plugin sync poll failed.");
        }
    }

    private async Task<PluginSyncManifest> LoadManifestAsync()
    {
        if (!string.IsNullOrWhiteSpace(_config.ServerUrl))
        {
            string url = _config.ServerUrl.TrimEnd('/') + "/plugins/manifest.json";
            string json = await HttpClient.GetStringAsync(url);
            return JsonSerializer.Deserialize<PluginSyncManifest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        if (_config.AllowP2PManifestFallback && !string.IsNullOrWhiteSpace(_config.P2PManifestPath) && File.Exists(_config.P2PManifestPath))
        {
            string json = await File.ReadAllTextAsync(_config.P2PManifestPath);
            return JsonSerializer.Deserialize<PluginSyncManifest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    private static bool ValidateSha256(string expectedSha256, string filePath)
    {
        if (string.IsNullOrWhiteSpace(expectedSha256))
            return true;

        try
        {
            using var stream = File.OpenRead(filePath);
            using var sha = SHA256.Create();
            byte[] hashBytes = sha.ComputeHash(stream);
            string actual = Convert.ToHexString(hashBytes);
            string expected = expectedSha256.Trim().Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
        }
    }
}

public sealed class PluginSyncConfig
{
    public bool Enabled { get; set; } = false;
    public string ServerUrl { get; set; } = "http://127.0.0.1:8080";
    public float PollSeconds { get; set; } = 10f;
    public string InstallMode { get; set; } = "temporary";
    public bool AllowP2PManifestFallback { get; set; } = true;
    public string P2PManifestPath { get; set; } = string.Empty;

    public static PluginSyncConfig Load()
    {
        string path = Path.Combine(MelonEnvironment.UserDataDirectory, "pluginsync.config.json");

        if (!File.Exists(path))
        {
            var created = new PluginSyncConfig();
            string createdJson = JsonSerializer.Serialize(created, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, createdJson);
            return created;
        }

        string json = File.ReadAllText(path);
        var parsed = JsonSerializer.Deserialize<PluginSyncConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return parsed ?? new PluginSyncConfig();
    }
}

public sealed class PluginSyncManifest
{
    public List<PluginSyncManifestEntry> Plugins { get; set; } = new();
}

public sealed class PluginSyncManifestEntry
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
}
