using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using MelonLoader;

namespace FrikaMF.Hooks;

/// <summary>
/// Binds raw game assembly methods to FFM hook aliases and dispatches before/after handlers.
/// </summary>
public static class HookBinder
{
    private static readonly Dictionary<string, List<Action<HookContext>>> BeforeHandlers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, List<Action<HookContext>>> AfterHandlers = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object SyncRoot = new();

    private static readonly Regex BracketFormatRegex = new(
        "^runtimetrigger\\s+asm\\[Assembly-CSharp\\]\\s+type\\[(?<type>[^\\]]+)\\]\\s+method\\[(?<method>[^\\]]+)\\]$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex PipeFormatRegex = new(
        "^runtime_trigger\\s*\\|\\s*asm=Assembly-CSharp\\s*\\|\\s*type=(?<type>[^|]+)\\|\\s*method=(?<method>.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Dictionary<string, string> AliasesByRawMethod = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a pre-patch style handler for a canonical FFM hook alias.
    /// </summary>
    /// <param name="ffmHookName">FFM alias such as <c>FFM.Server.OnServerStart</c>.</param>
    /// <param name="handler">Handler delegate to invoke.</param>
    public static void OnBefore(string ffmHookName, Action<HookContext> handler)
    {
        RegisterHandler(BeforeHandlers, ffmHookName, handler);
    }

    /// <summary>
    /// Registers a post-patch style handler for a canonical FFM hook alias.
    /// </summary>
    /// <param name="ffmHookName">FFM alias such as <c>FFM.Server.OnServerStart</c>.</param>
    /// <param name="handler">Handler delegate to invoke.</param>
    public static void OnAfter(string ffmHookName, Action<HookContext> handler)
    {
        RegisterHandler(AfterHandlers, ffmHookName, handler);
    }

    /// <summary>
    /// Unregisters all before and after handlers for the provided hook alias.
    /// </summary>
    /// <param name="ffmHookName">FFM hook alias.</param>
    public static void Unregister(string ffmHookName)
    {
        if (string.IsNullOrWhiteSpace(ffmHookName))
            return;

        lock (SyncRoot)
        {
            BeforeHandlers.Remove(ffmHookName);
            AfterHandlers.Remove(ffmHookName);
        }
    }

    /// <summary>
    /// Loads raw-to-FFM alias mappings from a hook dump file.
    /// </summary>
    /// <param name="hookDumpPath">Path to the assembly hook dump text file.</param>
    public static void LoadAliases(string hookDumpPath)
    {
        if (string.IsNullOrWhiteSpace(hookDumpPath))
        {
            MelonLogger.Warning($"HookBinder alias load skipped: file missing '{hookDumpPath}'.");
            return;
        }

        string effectivePath = hookDumpPath;
        if (!File.Exists(effectivePath) && File.Exists(hookDumpPath + ".gz"))
            effectivePath = hookDumpPath + ".gz";

        if (!File.Exists(effectivePath))
        {
            MelonLogger.Warning($"HookBinder alias load skipped: file missing '{hookDumpPath}' (or .gz).");
            return;
        }

        string[] lines = ReadAllLinesFlexible(effectivePath);
        int added = 0;

        lock (SyncRoot)
        {
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                if (!TryParseHookLine(lines[lineIndex], out string typeName, out string methodName))
                    continue;

                string rawKey = BuildRawKey(typeName, methodName);
                if (AliasesByRawMethod.ContainsKey(rawKey))
                    continue;

                string alias = BuildAlias(typeName, methodName);
                AliasesByRawMethod[rawKey] = alias;
                added++;
            }
        }

        MelonLogger.Msg($"HookBinder aliases loaded: {added}");
    }

    private static string[] ReadAllLinesFlexible(string path)
    {
        if (path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
        {
            using FileStream fileStream = File.OpenRead(path);
            using var gzip = new GZipStream(fileStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            string content = reader.ReadToEnd();
            return content.Replace("\r\n", "\n").Split('\n');
        }

        return File.ReadAllLines(path);
    }

    /// <summary>
    /// Resolves an FFM alias from a reflected method.
    /// </summary>
    /// <param name="method">Method to resolve.</param>
    /// <returns>Canonical FFM alias.</returns>
    public static string ResolveAlias(MethodBase method)
    {
        if (method == null)
            return "FFM.Misc.OnUnknown";

        string typeName = method.DeclaringType?.FullName ?? method.DeclaringType?.Name ?? "UnknownType";
        string methodName = method.Name ?? "UnknownMethod";
        string rawKey = BuildRawKey(typeName, methodName);

        lock (SyncRoot)
        {
            if (AliasesByRawMethod.TryGetValue(rawKey, out string alias))
                return alias;
        }

        return BuildAlias(typeName, methodName);
    }

    internal static void InvokeBefore(HookContext context)
    {
        InvokeHandlers(BeforeHandlers, context);
    }

    internal static void InvokeAfter(HookContext context)
    {
        InvokeHandlers(AfterHandlers, context);
    }

    /// <summary>
    /// Dispatches a before-hook context to registered before handlers.
    /// </summary>
    public static void DispatchBefore(HookContext context)
    {
        InvokeBefore(context);
    }

    /// <summary>
    /// Dispatches an after-hook context to registered after handlers.
    /// </summary>
    public static void DispatchAfter(HookContext context)
    {
        InvokeAfter(context);
    }

    private static void RegisterHandler(Dictionary<string, List<Action<HookContext>>> map, string ffmHookName, Action<HookContext> handler)
    {
        if (string.IsNullOrWhiteSpace(ffmHookName))
            throw new ArgumentException("FFM hook name must not be empty.", nameof(ffmHookName));

        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        lock (SyncRoot)
        {
            if (!map.TryGetValue(ffmHookName, out List<Action<HookContext>> handlers))
            {
                handlers = new List<Action<HookContext>>();
                map[ffmHookName] = handlers;
            }

            handlers.Add(handler);
        }
    }

    private static void InvokeHandlers(Dictionary<string, List<Action<HookContext>>> map, HookContext context)
    {
        if (context == null || string.IsNullOrWhiteSpace(context.HookName))
            return;

        List<Action<HookContext>> snapshot;
        lock (SyncRoot)
        {
            if (!map.TryGetValue(context.HookName, out List<Action<HookContext>> handlers) || handlers.Count == 0)
                return;

            snapshot = new List<Action<HookContext>>(handlers);
        }

        for (int index = 0; index < snapshot.Count; index++)
        {
            try
            {
                snapshot[index](context);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"HookBinder handler failed for '{context.HookName}': {ex.Message}");
            }
        }
    }

    private static bool TryParseHookLine(string line, out string typeName, out string methodName)
    {
        typeName = string.Empty;
        methodName = string.Empty;

        if (string.IsNullOrWhiteSpace(line))
            return false;

        Match bracketMatch = BracketFormatRegex.Match(line.Trim());
        if (bracketMatch.Success)
        {
            typeName = bracketMatch.Groups["type"].Value.Trim();
            methodName = bracketMatch.Groups["method"].Value.Trim();
            return !string.IsNullOrWhiteSpace(typeName) && !string.IsNullOrWhiteSpace(methodName);
        }

        Match pipeMatch = PipeFormatRegex.Match(line.Trim());
        if (!pipeMatch.Success)
            return false;

        typeName = pipeMatch.Groups["type"].Value.Trim();
        methodName = pipeMatch.Groups["method"].Value.Trim();
        return !string.IsNullOrWhiteSpace(typeName) && !string.IsNullOrWhiteSpace(methodName);
    }

    private static string BuildRawKey(string typeName, string methodName)
    {
        return $"{typeName}::{methodName}";
    }

    private static string BuildAlias(string typeName, string methodName)
    {
        string category = GetCategory(typeName);
        string action = methodName.StartsWith("On", StringComparison.Ordinal) ? methodName : $"On{methodName}";
        return $"FFM.{category}.{action}";
    }

    private static string GetCategory(string typeName)
    {
        string source = typeName ?? string.Empty;

        if (source.StartsWith("DataCenter.", StringComparison.OrdinalIgnoreCase))
            return "DataCenter";

        if (source.Contains("ServerManager", StringComparison.OrdinalIgnoreCase) || source.StartsWith("Server", StringComparison.OrdinalIgnoreCase))
            return "Server";

        if (source.Contains("NetworkManager", StringComparison.OrdinalIgnoreCase) || source.StartsWith("Network", StringComparison.OrdinalIgnoreCase))
            return "Network";

        if (source.Contains("PlayerController", StringComparison.OrdinalIgnoreCase) || source.StartsWith("Player", StringComparison.OrdinalIgnoreCase))
            return "Player";

        if (source.Contains("UIManager", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("UI", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("HUD", StringComparison.OrdinalIgnoreCase))
            return "UI";

        if (source.StartsWith("Item", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("Equipment", StringComparison.OrdinalIgnoreCase))
            return "Items";

        if (source.Contains("GameManager", StringComparison.OrdinalIgnoreCase)
            || source.Contains("GameController", StringComparison.OrdinalIgnoreCase))
            return "Game";

        return "Misc";
    }
}
