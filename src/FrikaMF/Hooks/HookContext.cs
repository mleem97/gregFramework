using System;
using System.Collections.Generic;
using System.Reflection;

namespace FrikaMF.Hooks;

/// <summary>
/// Runtime context for an FFM hook invocation.
/// </summary>
public sealed class HookContext
{
    /// <summary>
    /// Creates a new hook context.
    /// </summary>
    /// <param name="hookName">Canonical FFM hook alias.</param>
    /// <param name="method">Target reflected method.</param>
    /// <param name="instance">Target instance for instance methods, or <c>null</c> for static methods.</param>
    /// <param name="arguments">Method arguments snapshot.</param>
    public HookContext(string hookName, MethodBase method, object instance, object[] arguments)
    {
        HookName = hookName ?? string.Empty;
        Method = method;
        Instance = instance;
        Arguments = arguments ?? Array.Empty<object>();
    }

    /// <summary>
    /// Canonical FFM hook alias.
    /// </summary>
    public string HookName { get; }

    /// <summary>
    /// Reflected target method.
    /// </summary>
    public MethodBase Method { get; }

    /// <summary>
    /// Target instance for instance methods.
    /// </summary>
    public object Instance { get; }

    /// <summary>
    /// Runtime argument snapshot for the invocation.
    /// </summary>
    public object[] Arguments { get; }

    /// <summary>
    /// Optional return value for post-invocation handlers.
    /// </summary>
    public object ReturnValue { get; set; }

    /// <summary>
    /// Optional exception that occurred during invocation.
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    /// Extensible bag for plugin-specific metadata.
    /// </summary>
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}
