using System;
using System.Collections.Generic;

namespace DataCenterModLoader;

/// <summary>
/// Custom text resolver delegate for localisation interception.
/// Called during text resolution to allow mods to override or augment localised strings.
/// </summary>
/// <param name="textId">The text UID being resolved.</param>
/// <param name="currentText">The current/previous text value from localisation.</param>
/// <param name="resolvedText">Output: the text to use (set by resolver if handling the ID).</param>
/// <returns>True if this resolver handled the ID and set resolvedText; false to continue chain.</returns>
public delegate bool LocalisationResolverDelegate(int textId, string currentText, out string resolvedText);

/// <summary>
/// Localisation bridge managing custom text resolution and language switching.
/// Allows mods to register resolver chains that intercept and override game localisation
/// without modifying the core i18n system.
/// 
/// Previously part of ModigAPIs, now consolidated within FrikaMF core.
/// 
/// Use via ModigApi.RegisterLocalisationResolver() / UnregisterLocalisationResolver().
/// </summary>
public static class LocalisationBridge
{
    private static readonly Dictionary<string, LocalisationResolverDelegate> Resolvers = 
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly object SyncRoot = new();

    /// <summary>
    /// Registers a custom text resolver.
    /// Resolvers are called in sequence order during text resolution.
    /// First resolver returning true wins; chain continues otherwise.
    /// </summary>
    /// <param name="resolverId">Unique resolver identifier (case-insensitive).</param>
    /// <param name="resolver">Resolver delegate to register.</param>
    public static void RegisterResolver(string resolverId, LocalisationResolverDelegate resolver)
    {
        if (string.IsNullOrWhiteSpace(resolverId) || resolver == null)
            return;

        lock (SyncRoot)
            Resolvers[resolverId.Trim()] = resolver;
    }

    /// <summary>
    /// Unregisters a previously registered text resolver.
    /// </summary>
    /// <param name="resolverId">Resolver identifier to unregister.</param>
    public static void UnregisterResolver(string resolverId)
    {
        if (string.IsNullOrWhiteSpace(resolverId))
            return;

        lock (SyncRoot)
            Resolvers.Remove(resolverId.Trim());
    }

    /// <summary>
    /// Attempts to resolve text through the registered resolver chain.
    /// Resolvers are called in sequence; first to return true provides the resolved text.
    /// If no resolver handles the ID, original text is returned.
    /// </summary>
    /// <param name="textId">Text UID to resolve.</param>
    /// <param name="currentText">Current text from localisation (input and fallback).</param>
    /// <param name="resolvedText">Output: resolved text (set by resolver or equals currentText if unhandled).</param>
    /// <returns>True if text was modified by resolver; false if unchanged.</returns>
    public static bool TryResolve(int textId, string currentText, out string resolvedText)
    {
        resolvedText = currentText;

        KeyValuePair<string, LocalisationResolverDelegate>[] resolvers;
        lock (SyncRoot)
            resolvers = new List<KeyValuePair<string, LocalisationResolverDelegate>>(Resolvers).ToArray();

        foreach (var pair in resolvers)
        {
            try
            {
                if (!pair.Value(textId, resolvedText, out string overrideText))
                    continue;

                if (string.IsNullOrWhiteSpace(overrideText))
                    continue;

                resolvedText = overrideText;
            }
            catch (Exception ex)
            {
                CrashLog.LogException($"LocalisationBridge resolver '{pair.Key}'", ex);
            }
        }

        return !string.Equals(resolvedText, currentText, StringComparison.Ordinal);
    }
}
