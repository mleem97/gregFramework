using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AssetExporter
{
    public sealed class RuntimeHookService
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
            "close"
        };

        private static readonly ConcurrentDictionary<string, int> TriggerCounts = new ConcurrentDictionary<string, int>();
        private static readonly ConcurrentDictionary<string, byte> PatchedMethods = new ConcurrentDictionary<string, byte>();

        private readonly string harmonyId = "frikadelle.framework.runtimehooks";
        private static readonly Regex CatalogLineRegex = new Regex(
            "^runtime_trigger \\| asm=Assembly-CSharp \\| type=(?<type>[^|]+) \\| method=(?<method>.+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public HookScanResult ScanCandidates(int maxHooks)
        {
            var candidates = DiscoverCandidates(maxHooks).ToList();
            var names = new List<string>(candidates.Count);

            foreach (MethodInfo candidate in candidates)
                names.Add(BuildMethodKey(candidate));

            ModFramework.Events.Publish(new HookScanCompletedEvent(DateTime.UtcNow, names.Count));
            return new HookScanResult(names);
        }

        public HookInstallResult ScanAndInstall(int maxHooks)
        {
            var candidates = DiscoverCandidates(maxHooks).ToList();
            ModFramework.Events.Publish(new HookScanCompletedEvent(DateTime.UtcNow, candidates.Count));

            return InstallCandidates(candidates);
        }

        public HookInstallResult InstallFromCatalog(string catalogPath, int maxHooks)
        {
            if (string.IsNullOrWhiteSpace(catalogPath))
                return new HookInstallResult(0, 0, 0, new[] { "Catalog path is empty." });

            if (!File.Exists(catalogPath))
                return new HookInstallResult(0, 0, 0, new[] { $"Catalog file not found: {catalogPath}" });

            var candidates = DiscoverFromCatalog(catalogPath, maxHooks).ToList();
            ModFramework.Events.Publish(new HookScanCompletedEvent(DateTime.UtcNow, candidates.Count));

            return InstallCandidates(candidates);
        }

        private HookInstallResult InstallCandidates(IReadOnlyList<MethodInfo> candidates)
        {
            int scanned = candidates?.Count ?? 0;
            if (scanned == 0)
                return new HookInstallResult(0, 0, 0, Array.Empty<string>());

            int installed = 0;
            int failed = 0;
            var errors = new List<string>();

            if (!TryGetHarmonyTypes(out var harmonyType, out var harmonyMethodType, out var reason))
            {
                errors.Add(reason);
                return new HookInstallResult(scanned, installed, failed, errors);
            }

            object harmony = Activator.CreateInstance(harmonyType, harmonyId);
            MethodInfo patchMethod = harmonyType.GetMethod(
                "Patch",
                new[] { typeof(MethodBase), harmonyMethodType, harmonyMethodType, harmonyMethodType, harmonyMethodType });

            if (patchMethod == null)
            {
                errors.Add("Harmony patch method could not be resolved.");
                return new HookInstallResult(scanned, installed, failed, errors);
            }

            MethodInfo postfix = typeof(RuntimeHookService).GetMethod(nameof(GenericPostfix), BindingFlags.Public | BindingFlags.Static);
            object postfixHarmonyMethod = Activator.CreateInstance(harmonyMethodType, postfix);

            foreach (MethodInfo candidate in candidates)
            {
                string key = BuildMethodKey(candidate);
                if (!PatchedMethods.TryAdd(key, 1))
                    continue;

                try
                {
                    patchMethod.Invoke(harmony, new[] { candidate, null, postfixHarmonyMethod, null, null });
                    installed++;
                }
                catch (Exception ex)
                {
                    failed++;
                    errors.Add($"{key} => {ex.GetBaseException().Message}");
                }
            }

            ModFramework.Events.Publish(new HookInstallCompletedEvent(DateTime.UtcNow, installed, failed));
            return new HookInstallResult(scanned, installed, failed, errors);
        }

        private static IEnumerable<MethodInfo> DiscoverFromCatalog(string catalogPath, int maxHooks)
        {
            int limit = maxHooks <= 0 ? int.MaxValue : maxHooks;
            var results = new List<MethodInfo>();
            var unique = new HashSet<string>(StringComparer.Ordinal);

            Dictionary<string, Type> typeMap = BuildAssemblyTypeMap();

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

        private static Dictionary<string, Type> BuildAssemblyTypeMap()
        {
            var map = new Dictionary<string, Type>(StringComparer.Ordinal);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string asmName = assembly.GetName().Name ?? string.Empty;
                if (!IsRelevantAssembly(asmName))
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

        public static void GenericPostfix(MethodBase __originalMethod)
        {
            if (__originalMethod == null)
                return;

            string key = BuildMethodKey(__originalMethod);
            int count = TriggerCounts.AddOrUpdate(key, 1, UpdateCount);

            if (count <= 3 || count % 50 == 0)
                ModFramework.Events.Publish(new HookTriggeredEvent(DateTime.UtcNow, key, count));
        }

        private static IEnumerable<MethodInfo> DiscoverCandidates(int maxHooks)
        {
            var found = new List<MethodInfo>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string asmName = assembly.GetName().Name ?? string.Empty;
                if (!IsRelevantAssembly(asmName))
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
                        if (!IsHookCandidate(method))
                            continue;

                        found.Add(method);
                        if (found.Count >= maxHooks)
                            return found;
                    }
                }
            }

            return found;
        }

        private static bool IsRelevantAssembly(string assemblyName)
        {
            return string.Equals(assemblyName, "Assembly-CSharp", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsHookCandidate(MethodInfo method)
        {
            if (method == null)
                return false;

            if (method.IsAbstract || method.IsGenericMethodDefinition)
                return false;

            if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_") || method.Name.StartsWith("add_") || method.Name.StartsWith("remove_"))
                return false;

            string name = method.Name.ToLowerInvariant();
            return Keywords.Any(k => name.Contains(k));
        }

        private static bool TryGetHarmonyTypes(out Type harmonyType, out Type harmonyMethodType, out string reason)
        {
            harmonyType = Type.GetType("HarmonyLib.Harmony, 0Harmony");
            harmonyMethodType = Type.GetType("HarmonyLib.HarmonyMethod, 0Harmony");
            if (harmonyType == null || harmonyMethodType == null)
            {
                reason = "Harmony types are not available at runtime (0Harmony missing).";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private static int UpdateCount(string key, int current)
        {
            return current + 1;
        }

        private static string BuildMethodKey(MethodBase method)
        {
            return $"{method.DeclaringType?.FullName ?? "<unknown>"}.{method.Name}";
        }
    }

    public sealed class HookInstallResult
    {
        public HookInstallResult(int scanned, int installed, int failed, IReadOnlyList<string> errors)
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

    public sealed class HookScanResult
    {
        public HookScanResult(IReadOnlyList<string> candidates)
        {
            Candidates = candidates;
        }

        public IReadOnlyList<string> Candidates { get; }
    }
}
