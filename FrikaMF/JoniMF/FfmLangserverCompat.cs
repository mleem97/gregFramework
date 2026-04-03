using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;

namespace DataCenterModLoader;

public sealed class FfmLangserverCompatRuntime
{
    private const string CompatFolderName = "FFM.Langserver.Compat";
    private readonly MelonLogger.Instance _logger;
    private readonly string _modsDirectory;
    private readonly string _frameworkDirectory;
    private readonly string _compatDirectory;
    private readonly List<LangserverCompatAdapter> _adapters = new();

    public IReadOnlyList<LangserverCompatAdapter> Adapters => _adapters;

    private FfmLangserverCompatRuntime(MelonLogger.Instance logger)
    {
        _logger = logger;
        _modsDirectory = MelonEnvironment.ModsDirectory;
        _frameworkDirectory = Path.Combine(MelonEnvironment.GameRootDirectory, "FrikaFM");
        _compatDirectory = Path.Combine(_modsDirectory, CompatFolderName);
    }

    public static FfmLangserverCompatRuntime Initialize(MelonLogger.Instance logger)
    {
        var runtime = new FfmLangserverCompatRuntime(logger);
        runtime.EnsureFolders();
        runtime.EnsureExampleManifest();
        runtime.LoadAdapters();
        runtime.DetectPotentialFfiMods();
        runtime.DetectClaimCollisions();
        runtime.WriteStatusFile();
        return runtime;
    }

    public void DetectHarmonyConflicts(string localHarmonyId)
    {
        if (string.IsNullOrWhiteSpace(localHarmonyId))
            return;

        try
        {
            foreach (var method in HarmonyLib.Harmony.GetAllPatchedMethods())
            {
                var patchInfo = HarmonyLib.Harmony.GetPatchInfo(method);
                if (patchInfo == null)
                    continue;

                var owners = patchInfo.Owners?.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? Array.Empty<string>();
                if (owners.Length <= 1)
                    continue;

                bool oursIncluded = owners.Any(owner => string.Equals(owner, localHarmonyId, StringComparison.OrdinalIgnoreCase));
                if (!oursIncluded)
                    continue;

                var foreignOwners = owners
                    .Where(owner => !string.Equals(owner, localHarmonyId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (foreignOwners.Length == 0)
                    continue;

                string methodName = $"{method.DeclaringType?.FullName}.{method.Name}";
                string ownerList = string.Join(", ", foreignOwners);
                string message = $"Harmony hook overlap detected on '{methodName}' with owners [{ownerList}]. Running in compatibility mode.";
                _logger.Warning(message);
                CrashLog.LogError("compat-harmony", message);
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException("FfmLangserverCompatRuntime.DetectHarmonyConflicts", ex);
        }
    }

    private void EnsureFolders()
    {
        Directory.CreateDirectory(_frameworkDirectory);
        Directory.CreateDirectory(_compatDirectory);
    }

    private void EnsureExampleManifest()
    {
        string examplePath = Path.Combine(_compatDirectory, "example.langserver.compat.json");
        if (File.Exists(examplePath))
            return;

        var example = new LangserverCompatAdapter
        {
            AdapterId = "example.adapter",
            Language = "rust",
            Runtime = "native-dll",
            Version = "1.0.0",
            Description = "Example adapter for FFM.Langserver.Compat discovery.",
            HookClaims = new List<string>
            {
                "Assembly-CSharp::Server.PowerButton",
                "Assembly-CSharp::NetworkSwitch.RepairDevice"
            }
        };

        string json = JsonSerializer.Serialize(example, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(examplePath, json);
    }

    private void LoadAdapters()
    {
        _adapters.Clear();

        foreach (string filePath in Directory.GetFiles(_compatDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                string content = File.ReadAllText(filePath);
                var adapter = JsonSerializer.Deserialize<LangserverCompatAdapter>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (adapter == null)
                    continue;

                if (string.IsNullOrWhiteSpace(adapter.AdapterId))
                    adapter.AdapterId = Path.GetFileNameWithoutExtension(filePath);

                adapter.SourceFile = filePath;
                adapter.HookClaims ??= new List<string>();
                _adapters.Add(adapter);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to parse compat adapter manifest '{Path.GetFileName(filePath)}': {ex.Message}");
                CrashLog.LogException("FfmLangserverCompatRuntime.LoadAdapters", ex);
            }
        }

        _logger.Msg($"[Compat] Loaded {_adapters.Count} adapter manifest(s) from '{_compatDirectory}'.");
    }

    private void DetectPotentialFfiMods()
    {
        try
        {
            var knownSafeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "FrikaModdingFramework.dll",
                "FMF.UIReplacementMod.dll",
                "FMF.HexLabelMod.dll",
                "FMF.JoniMLCompatMod.dll"
            };

            var candidates = Directory.GetFiles(_modsDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(path => !knownSafeNames.Contains(Path.GetFileName(path)))
                .Where(path =>
                {
                    string name = Path.GetFileNameWithoutExtension(path);
                    return name.Contains("ffi", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("bridge", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("modloader", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("langserver", StringComparison.OrdinalIgnoreCase);
                })
                .ToArray();

            foreach (string candidate in candidates)
            {
                string message = $"Potential external FFI/bridge detected: '{Path.GetFileName(candidate)}'. Compatibility guard enabled.";
                _logger.Warning(message);
                CrashLog.LogError("compat-external-ffi", message);
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException("FfmLangserverCompatRuntime.DetectPotentialFfiMods", ex);
        }
    }

    private void DetectClaimCollisions()
    {
        if (_adapters.Count == 0)
            return;

        var claimOwners = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var adapter in _adapters)
        {
            foreach (string claim in adapter.HookClaims ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(claim))
                    continue;

                if (!claimOwners.TryGetValue(claim, out var owners))
                {
                    owners = new List<string>();
                    claimOwners[claim] = owners;
                }

                owners.Add(adapter.AdapterId ?? "unknown");
            }
        }

        foreach (var pair in claimOwners)
        {
            var owners = pair.Value.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (owners.Length <= 1)
                continue;

            string message = $"Compat hook claim collision on '{pair.Key}' by [{string.Join(", ", owners)}].";
            _logger.Warning(message);
            CrashLog.LogError("compat-claim-collision", message);
        }
    }

    private void WriteStatusFile()
    {
        try
        {
            string path = Path.Combine(_frameworkDirectory, "ffm-langserver-compat-status.json");
            var report = new
            {
                generatedAtUtc = DateTime.UtcNow,
                compatDirectory = _compatDirectory,
                adapterCount = _adapters.Count,
                adapters = _adapters.Select(adapter => new
                {
                    adapterId = adapter.AdapterId,
                    language = adapter.Language,
                    runtime = adapter.Runtime,
                    version = adapter.Version,
                    sourceFile = adapter.SourceFile,
                    claims = adapter.HookClaims ?? new List<string>()
                }).ToArray()
            };

            string json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("FfmLangserverCompatRuntime.WriteStatusFile", ex);
        }
    }
}

public sealed class LangserverCompatAdapter
{
    public string AdapterId { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Runtime { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> HookClaims { get; set; } = new();
    public string SourceFile { get; set; } = string.Empty;
}
