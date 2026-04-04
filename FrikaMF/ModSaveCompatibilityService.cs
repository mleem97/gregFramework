using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MelonLoader.Utils;

namespace DataCenterModLoader;

/// <summary>
/// Keeps framework/mod state isolated from vanilla save data and repairs broken sidecar data.
/// </summary>
public static class ModSaveCompatibilityService
{
    private static readonly string CompatibilityDirectory = Path.Combine(MelonEnvironment.UserDataDirectory, "FrikaFM");
    private static readonly string StampPath = Path.Combine(CompatibilityDirectory, "save-compatibility.json");

    public static void Initialize()
    {
        EnsureSafeExternalState();
        WriteCompatibilityStamp("init");
    }

    public static void OnSaveCompleted()
    {
        EnsureSafeExternalState();
        WriteCompatibilityStamp("save");
    }

    public static void OnLoadCompleted()
    {
        EnsureSafeExternalState();
        WriteCompatibilityStamp("load");
    }

    public static void EnsureSafeExternalState()
    {
        try
        {
            Directory.CreateDirectory(CompatibilityDirectory);
            EnsureTextFileSafe(Path.Combine(MelonEnvironment.UserDataDirectory, "custom_employees_hired.txt"), 1024 * 256);
            EnsureJsonFileSafe(Path.Combine(MelonEnvironment.UserDataDirectory, "multiplayer-sync.config.json"), 1024 * 512);
            EnsureJsonFileSafe(Path.Combine(MelonEnvironment.UserDataDirectory, "pluginsync.config.json"), 1024 * 512);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("ModSaveCompatibilityService.EnsureSafeExternalState", ex);
        }
    }

    private static void EnsureTextFileSafe(string path, int maxSizeBytes)
    {
        try
        {
            if (!File.Exists(path))
                return;

            var info = new FileInfo(path);
            if (info.Length > maxSizeBytes)
            {
                Quarantine(path, "oversize");
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException($"EnsureTextFileSafe({path})", ex);
        }
    }

    private static void EnsureJsonFileSafe(string path, int maxSizeBytes)
    {
        try
        {
            if (!File.Exists(path))
                return;

            var info = new FileInfo(path);
            if (info.Length > maxSizeBytes)
            {
                Quarantine(path, "oversize");
                return;
            }

            var json = File.ReadAllText(path);
            using var _ = JsonDocument.Parse(json);
        }
        catch
        {
            Quarantine(path, "invalid-json");
        }
    }

    private static void Quarantine(string sourcePath, string reason)
    {
        try
        {
            if (!File.Exists(sourcePath))
                return;

            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            string fileName = Path.GetFileName(sourcePath);
            string targetPath = Path.Combine(CompatibilityDirectory, $"{fileName}.{reason}.{timestamp}.broken");

            File.Copy(sourcePath, targetPath, true);
            File.Delete(sourcePath);

            CrashLog.Log($"[SaveCompat] Quarantined '{sourcePath}' -> '{targetPath}' ({reason}).");
        }
        catch (Exception ex)
        {
            CrashLog.LogException($"Quarantine({sourcePath})", ex);
        }
    }

    private static void WriteCompatibilityStamp(string phase)
    {
        try
        {
            var payload = new Dictionary<string, object>
            {
                ["updatedAtUtc"] = DateTime.UtcNow.ToString("O"),
                ["phase"] = phase,
                ["policy"] = "Framework stores mod state only in sidecar files under UserData; no custom fields are injected into vanilla save schema.",
                ["sidecarFiles"] = new[]
                {
                    "custom_employees_hired.txt",
                    "multiplayer-sync.config.json",
                    "pluginsync.config.json"
                }
            };

            string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StampPath, json);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("WriteCompatibilityStamp", ex);
        }
    }
}
