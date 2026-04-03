using System;
using System.IO;
using System.Text.Json;
using MelonLoader;
using MelonLoader.Utils;

namespace FMF.UIReplacementMod;

internal sealed class RuntimeOptions
{
    public bool EnableDiscordRichPresence { get; set; } = true;
    public string DiscordClientId { get; set; } = "123456789012345678";
    public int MaxPlayers { get; set; } = 16;
    public bool EnableLiveUiReload { get; set; } = true;
    public string PreferredUiRoot { get; set; } = "GameRoot/UI";

    public static RuntimeOptions LoadOrCreate(MelonLogger.Instance logger)
    {
        string configDir = Path.Combine(MelonEnvironment.GameRootDirectory, "FrikaFramework");
        Directory.CreateDirectory(configDir);
        string configPath = Path.Combine(configDir, "fmf-ui-replacement.config.json");

        if (!File.Exists(configPath))
        {
            var defaults = new RuntimeOptions();
            File.WriteAllText(configPath, JsonSerializer.Serialize(defaults, new JsonSerializerOptions { WriteIndented = true }));
            logger.Msg($"[FMF.UIReplacement] Created config: {configPath}");
            return defaults;
        }

        try
        {
            string raw = File.ReadAllText(configPath);
            var loaded = JsonSerializer.Deserialize<RuntimeOptions>(raw);
            if (loaded == null)
                return new RuntimeOptions();

            loaded.MaxPlayers = Math.Clamp(loaded.MaxPlayers, 2, 32);
            return loaded;
        }
        catch (Exception ex)
        {
            logger.Warning($"[FMF.UIReplacement] Failed to parse config, using defaults: {ex.Message}");
            return new RuntimeOptions();
        }
    }
}
