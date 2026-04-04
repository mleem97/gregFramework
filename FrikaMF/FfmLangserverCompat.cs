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
    private readonly HashSet<string> _frameworkOwnedMethods = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ClaimLockEntry> _claimLocks = new(StringComparer.OrdinalIgnoreCase);
    private const string FrameworkAdapterId = "framework.core";
    private const int FrameworkPriority = 10_000;

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
        runtime.RebuildClaimLocks();
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
            _frameworkOwnedMethods.Clear();
            _claimLocks.Clear();

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

                string methodKey = GetMethodKey(method);
                _frameworkOwnedMethods.Add(methodKey);

                UpsertClaimLock(methodKey, FrameworkAdapterId, FrameworkPriority, "exclusive");

                var foreignOwners = owners
                    .Where(owner => !string.Equals(owner, localHarmonyId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (foreignOwners.Length == 0)
                    continue;

                string methodName = $"{method.DeclaringType?.FullName}.{method.Name}";
                string ownerList = string.Join(", ", foreignOwners);
                if (CanOwnerPatch(methodName, FrameworkAdapterId, FrameworkPriority, "exclusive"))
                {
                    string message = $"Harmony hook overlap detected on '{methodName}' with owners [{ownerList}]. Framework lock keeps ownership.";
                    _logger.Warning(message);
                    CrashLog.LogError("compat-harmony", message);
                }
            }

            WriteClaimLocksFile();
        }
        catch (Exception ex)
        {
            CrashLog.LogException("FfmLangserverCompatRuntime.DetectHarmonyConflicts", ex);
        }
    }

    public bool EnsureFrameworkPatchPresence(string localHarmonyId)
    {
        if (string.IsNullOrWhiteSpace(localHarmonyId) || _frameworkOwnedMethods.Count == 0)
            return true;

        try
        {
            var patchedMethods = HarmonyLib.Harmony.GetAllPatchedMethods().ToList();
            var patchedByKey = patchedMethods.ToDictionary(GetMethodKey, method => method, StringComparer.OrdinalIgnoreCase);

            foreach (string ownedKey in _frameworkOwnedMethods)
            {
                if (!patchedByKey.TryGetValue(ownedKey, out var method))
                {
                    string missingMessage = $"Framework Harmony method '{ownedKey}' is no longer patched. Triggering patch reinforcement.";
                    _logger.Warning(missingMessage);
                    CrashLog.LogError("compat-harmony-guard", missingMessage);
                    return false;
                }

                var patchInfo = HarmonyLib.Harmony.GetPatchInfo(method);
                if (patchInfo == null)
                    return false;

                bool oursPresent = patchInfo.Owners != null && patchInfo.Owners.Any(owner =>
                    string.Equals(owner, localHarmonyId, StringComparison.OrdinalIgnoreCase));

                if (!oursPresent)
                {
                    string message = $"Framework Harmony owner missing for '{ownedKey}'. Triggering patch reinforcement.";
                    _logger.Warning(message);
                    CrashLog.LogError("compat-harmony-guard", message);
                    return false;
                }

                if (!CanOwnerPatch(ownedKey, FrameworkAdapterId, FrameworkPriority, "exclusive"))
                {
                    string message = $"Arbitration protocol denied foreign ownership on '{ownedKey}'. Triggering patch reinforcement.";
                    _logger.Warning(message);
                    CrashLog.LogError("compat-claim-arbitration", message);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException("FfmLangserverCompatRuntime.EnsureFrameworkPatchPresence", ex);
        }

        return true;
    }

    private static string GetMethodKey(System.Reflection.MethodBase method)
    {
        return $"{method.DeclaringType?.FullName}.{method.Name}";
    }

    private void RebuildClaimLocks()
    {
        _claimLocks.Clear();

        foreach (var adapter in _adapters)
        {
            string adapterId = string.IsNullOrWhiteSpace(adapter.AdapterId) ? "unknown" : adapter.AdapterId.Trim();
            int priority = adapter.Priority;
            string mode = string.IsNullOrWhiteSpace(adapter.ClaimMode) ? "exclusive" : adapter.ClaimMode.Trim().ToLowerInvariant();

            foreach (string claim in adapter.HookClaims ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(claim))
                    continue;

                UpsertClaimLock(claim.Trim(), adapterId, priority, mode);
            }
        }
    }

    private void UpsertClaimLock(string claim, string adapterId, int priority, string mode)
    {
        if (_claimLocks.TryGetValue(claim, out ClaimLockEntry existing))
        {
            bool shouldReplace = priority > existing.Priority
                || (priority == existing.Priority && string.Compare(adapterId, existing.WinnerAdapterId, StringComparison.OrdinalIgnoreCase) < 0);

            if (shouldReplace)
            {
                existing.WinnerAdapterId = adapterId;
                existing.Priority = priority;
                existing.Mode = mode;
            }

            if (!existing.Contenders.Contains(adapterId, StringComparer.OrdinalIgnoreCase))
                existing.Contenders.Add(adapterId);

            _claimLocks[claim] = existing;
            return;
        }

        _claimLocks[claim] = new ClaimLockEntry
        {
            Claim = claim,
            WinnerAdapterId = adapterId,
            Priority = priority,
            Mode = mode,
            Contenders = new List<string> { adapterId }
        };
    }

    private bool CanOwnerPatch(string claim, string ownerAdapterId, int ownerPriority, string ownerMode)
    {
        if (!_claimLocks.TryGetValue(claim, out ClaimLockEntry lockEntry))
        {
            UpsertClaimLock(claim, ownerAdapterId, ownerPriority, ownerMode);
            return true;
        }

        if (string.Equals(lockEntry.Mode, "shared", StringComparison.OrdinalIgnoreCase))
            return true;

        return string.Equals(lockEntry.WinnerAdapterId, ownerAdapterId, StringComparison.OrdinalIgnoreCase);
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

        WriteClaimLocksFile();
    }

    private void WriteClaimLocksFile()
    {
        try
        {
            string path = Path.Combine(_frameworkDirectory, "ffm-langserver-claim-locks.json");
            var locks = _claimLocks.Values
                .OrderBy(lockEntry => lockEntry.Claim, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            string json = JsonSerializer.Serialize(new
            {
                generatedAtUtc = DateTime.UtcNow,
                lockCount = locks.Length,
                locks
            }, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("FfmLangserverCompatRuntime.WriteClaimLocksFile", ex);
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
    public int Priority { get; set; } = 100;
    public string ClaimMode { get; set; } = "exclusive";
    public List<string> HookClaims { get; set; } = new();
    public string SourceFile { get; set; } = string.Empty;
}

public sealed class ClaimLockEntry
{
    public string Claim { get; set; } = string.Empty;
    public string WinnerAdapterId { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Mode { get; set; } = "exclusive";
    public List<string> Contenders { get; set; } = new();
}
