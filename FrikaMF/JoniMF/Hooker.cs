using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;

namespace DataCenterModLoader;

public static class Hooker
{
    private static readonly string[] Keywords =
    {
        "event",
        "trigger",
        "dispatch",
        "invoke",
        "notify",
        "spawn",
        "update",
        "start",
        "complete",
        "open",
        "close",
    };

    private static readonly Regex CatalogLineRegex = new Regex(
        "^runtime_trigger \\| asm=Assembly-CSharp \\| type=(?<type>[^|]+) \\| method=(?<method>.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly ConcurrentDictionary<string, byte> PatchedMethods = new ConcurrentDictionary<string, byte>();
    private static readonly ConcurrentDictionary<string, int> TriggerCounts = new ConcurrentDictionary<string, int>();

    private static HarmonyLib.Harmony _harmony;
    private static readonly object SyncRoot = new object();

    public static HookerInstallResult InstallByScan(int maxHooks = 500, string harmonyId = "dc.modloader.hooker")
    {
        var candidates = DiscoverByScan(maxHooks).ToList();
        return InstallCandidates(candidates, harmonyId);
    }

    public static HookerInstallResult InstallFromCatalog(string catalogPath, int maxHooks = 2000, string harmonyId = "dc.modloader.hooker")
    {
        if (string.IsNullOrWhiteSpace(catalogPath))
            return new HookerInstallResult(0, 0, 0, new[] { "Catalog path is empty." });

        if (!File.Exists(catalogPath))
            return new HookerInstallResult(0, 0, 0, new[] { $"Catalog file not found: {catalogPath}" });

        var candidates = DiscoverFromCatalog(catalogPath, maxHooks).ToList();
        return InstallCandidates(candidates, harmonyId);
    }

    public static void GenericPostfix(MethodBase __originalMethod)
    {
        if (__originalMethod == null)
            return;

        string key = BuildMethodKey(__originalMethod);
        int count = TriggerCounts.AddOrUpdate(key, 1, UpdateTriggerCount);

        if (count <= 3 || count % 50 == 0)
            EventDispatcher.FireHookBridgeTriggered(key);
    }

    private static HookerInstallResult InstallCandidates(IReadOnlyList<MethodInfo> candidates, string harmonyId)
    {
        int scanned = candidates?.Count ?? 0;
        int installed = 0;
        int failed = 0;
        var errors = new List<string>();

        if (scanned == 0)
            return new HookerInstallResult(scanned, installed, failed, errors);

        try
        {
            EnsureHarmony(harmonyId);

            var postfix = new HarmonyMethod(typeof(Hooker).GetMethod(nameof(GenericPostfix), BindingFlags.Public | BindingFlags.Static));

            foreach (MethodInfo method in candidates)
            {
                string key = BuildMethodKey(method);
                if (!PatchedMethods.TryAdd(key, 1))
                    continue;

                try
                {
                    _harmony.Patch(method, postfix: postfix);
                    installed++;
                }
                catch (Exception ex)
                {
                    failed++;
                    errors.Add($"{key} => {ex.GetBaseException().Message}");
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Hooker setup failed: {ex.GetBaseException().Message}");
        }

        EventDispatcher.FireHookBridgeInstalled(installed, failed);
        return new HookerInstallResult(scanned, installed, failed, errors);
    }

    private static void EnsureHarmony(string harmonyId)
    {
        lock (SyncRoot)
        {
            if (_harmony != null)
                return;

            _harmony = new HarmonyLib.Harmony(harmonyId);
        }
    }

    private static IEnumerable<MethodInfo> DiscoverByScan(int maxHooks)
    {
        int limit = maxHooks <= 0 ? int.MaxValue : maxHooks;
        var results = new List<MethodInfo>();

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string asmName = assembly.GetName().Name ?? string.Empty;
            if (!string.Equals(asmName, "Assembly-CSharp", StringComparison.OrdinalIgnoreCase))
                continue;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                continue;
            }

            foreach (Type type in types)
            {
                MethodInfo[] methods;
                try
                {
                    methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                }
                catch
                {
                    continue;
                }

                foreach (MethodInfo method in methods)
                {
                    if (!LooksLikeHookCandidate(method))
                        continue;

                    results.Add(method);
                    if (results.Count >= limit)
                        return results;
                }
            }
        }

        return results;
    }

    private static IEnumerable<MethodInfo> DiscoverFromCatalog(string catalogPath, int maxHooks)
    {
        int limit = maxHooks <= 0 ? int.MaxValue : maxHooks;
        var results = new List<MethodInfo>();
        var unique = new HashSet<string>(StringComparer.Ordinal);
        var typeMap = BuildTypeMap();

        foreach (string line in File.ReadLines(catalogPath))
        {
            if (!TryParseCatalogLine(line, out string typeName, out string methodName))
                continue;

            if (!typeMap.TryGetValue(typeName, out Type type) || type == null)
                continue;

            MethodInfo[] methods;
            try
            {
                methods = type
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal))
                    .ToArray();
            }
            catch
            {
                continue;
            }

            foreach (MethodInfo method in methods)
            {
                string key = BuildMethodKey(method);
                if (!unique.Add(key))
                    continue;

                results.Add(method);
                if (results.Count >= limit)
                    return results;
            }
        }

        return results;
    }

    private static Dictionary<string, Type> BuildTypeMap()
    {
        var map = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string asmName = assembly.GetName().Name ?? string.Empty;
            if (!string.Equals(asmName, "Assembly-CSharp", StringComparison.OrdinalIgnoreCase))
                continue;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                continue;
            }

            foreach (Type type in types)
            {
                if (string.IsNullOrWhiteSpace(type.FullName))
                    continue;

                map[type.FullName] = type;
            }
        }

        return map;
    }

    private static bool TryParseCatalogLine(string line, out string typeName, out string methodName)
    {
        typeName = string.Empty;
        methodName = string.Empty;

        if (string.IsNullOrWhiteSpace(line))
            return false;

        Match match = CatalogLineRegex.Match(line);
        if (!match.Success)
            return false;

        typeName = match.Groups["type"].Value.Trim();
        methodName = match.Groups["method"].Value.Trim();

        return !string.IsNullOrWhiteSpace(typeName) && !string.IsNullOrWhiteSpace(methodName);
    }

    private static bool LooksLikeHookCandidate(MethodInfo method)
    {
        if (method == null)
            return false;

        if (method.IsAbstract || method.IsGenericMethodDefinition)
            return false;

        if (method.Name.StartsWith("get_", StringComparison.Ordinal)
            || method.Name.StartsWith("set_", StringComparison.Ordinal)
            || method.Name.StartsWith("add_", StringComparison.Ordinal)
            || method.Name.StartsWith("remove_", StringComparison.Ordinal))
            return false;

        string name = method.Name.ToLowerInvariant();
        return Keywords.Any(k => name.Contains(k));
    }

    private static string BuildMethodKey(MethodBase method)
    {
        return $"{method.DeclaringType?.FullName ?? "<unknown>"}.{method.Name}";
    }

    private static int UpdateTriggerCount(string methodKey, int currentCount)
    {
        return currentCount + 1;
    }
}

public sealed class HookerInstallResult
{
    public HookerInstallResult(int scanned, int installed, int failed, IReadOnlyList<string> errors)
    {
        Scanned = scanned;
        Installed = installed;
        Failed = failed;
        Errors = errors;
    }

    public int Scanned { get; }
    public int Installed { get; }
    public int Failed { get; }
    public IReadOnlyList<string> Errors { get; }
}
