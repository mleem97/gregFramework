using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        private readonly string harmonyId = "assetexporter.framework.runtimehooks";

        public HookInstallResult ScanAndInstall(int maxHooks)
        {
            var candidates = DiscoverCandidates(maxHooks).ToList();
            ModFramework.Events.Publish(new HookScanCompletedEvent(DateTime.UtcNow, candidates.Count));

            int installed = 0;
            int failed = 0;
            var errors = new List<string>();

            if (!TryGetHarmonyTypes(out var harmonyType, out var harmonyMethodType, out var reason))
            {
                errors.Add(reason);
                return new HookInstallResult(candidates.Count, installed, failed, errors);
            }

            object harmony = Activator.CreateInstance(harmonyType, harmonyId);
            MethodInfo patchMethod = harmonyType.GetMethod(
                "Patch",
                new[] { typeof(MethodBase), harmonyMethodType, harmonyMethodType, harmonyMethodType, harmonyMethodType });

            if (patchMethod == null)
            {
                errors.Add("Harmony patch method could not be resolved.");
                return new HookInstallResult(candidates.Count, installed, failed, errors);
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
            return new HookInstallResult(candidates.Count, installed, failed, errors);
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
            string lower = assemblyName.ToLowerInvariant();
            return lower.Contains("assembly-csharp") || lower.Contains("unity") || lower.Contains("data") || lower.Contains("game");
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
}
