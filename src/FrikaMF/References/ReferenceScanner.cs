using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MelonLoader;

namespace FrikaMF.References;

/// <summary>
/// Recursively scans a references folder and loads all valid managed assemblies.
/// </summary>
public static class ReferenceScanner
{
    private static readonly Dictionary<string, Assembly> _loaded = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _syncRoot = new();

    /// <summary>
    /// Initializes the scanner and loads all assemblies found under the given base path recursively.
    /// </summary>
    /// <param name="basePath">Root path to scan, typically the local references directory.</param>
    public static void Initialize(string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            MelonLogger.Warning("ReferenceScanner.Initialize skipped: base path is empty.");
            return;
        }

        if (!Directory.Exists(basePath))
        {
            MelonLogger.Warning($"ReferenceScanner.Initialize skipped: path not found '{basePath}'.");
            return;
        }

        string[] files;
        try
        {
            files = Directory.GetFiles(basePath, "*.dll", SearchOption.AllDirectories);
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"ReferenceScanner failed to enumerate assemblies: {exception.Message}");
            return;
        }

        for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
        {
            string filePath = files[fileIndex];
            TryLoadAssembly(filePath);
        }
    }

    /// <summary>
    /// Returns all loaded assemblies indexed by simple assembly name.
    /// </summary>
    public static IReadOnlyDictionary<string, Assembly> LoadedAssemblies => _loaded;

    /// <summary>
    /// Attempts to resolve a type by full name across all loaded and currently loaded app-domain assemblies.
    /// </summary>
    /// <param name="fullTypeName">Fully qualified type name.</param>
    /// <returns>The resolved <see cref="Type"/> if found; otherwise <c>null</c>.</returns>
    public static Type ResolveType(string fullTypeName)
    {
        if (string.IsNullOrWhiteSpace(fullTypeName))
            return null;

        lock (_syncRoot)
        {
            foreach (Assembly assembly in _loaded.Values)
            {
                Type resolved = assembly.GetType(fullTypeName, throwOnError: false, ignoreCase: false);
                if (resolved != null)
                    return resolved;
            }
        }

        Assembly[] appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int assemblyIndex = 0; assemblyIndex < appDomainAssemblies.Length; assemblyIndex++)
        {
            Type resolved = appDomainAssemblies[assemblyIndex].GetType(fullTypeName, throwOnError: false, ignoreCase: false);
            if (resolved != null)
                return resolved;
        }

        return null;
    }

    private static void TryLoadAssembly(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        try
        {
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(filePath);
            string simpleName = assemblyName.Name ?? string.Empty;
            if (string.IsNullOrWhiteSpace(simpleName))
                return;

            lock (_syncRoot)
            {
                if (_loaded.ContainsKey(simpleName))
                    return;
            }

            Assembly[] appDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int assemblyIndex = 0; assemblyIndex < appDomainAssemblies.Length; assemblyIndex++)
            {
                Assembly assembly = appDomainAssemblies[assemblyIndex];
                string currentName = assembly.GetName().Name ?? string.Empty;
                if (!string.Equals(currentName, simpleName, StringComparison.OrdinalIgnoreCase))
                    continue;

                lock (_syncRoot)
                {
                    if (!_loaded.ContainsKey(simpleName))
                        _loaded[simpleName] = assembly;
                }

                MelonLogger.Msg($"ReferenceScanner: already loaded '{simpleName}' (domain). ");
                return;
            }

            Assembly loadedAssembly = Assembly.LoadFrom(filePath);
            lock (_syncRoot)
            {
                if (!_loaded.ContainsKey(simpleName))
                    _loaded[simpleName] = loadedAssembly;
            }

            MelonLogger.Msg($"ReferenceScanner loaded: {simpleName} from '{filePath}'");
        }
        catch (BadImageFormatException)
        {
            MelonLogger.Warning($"ReferenceScanner skipped native/invalid DLL: '{filePath}'");
        }
        catch (FileLoadException fileLoadException)
        {
            MelonLogger.Warning($"ReferenceScanner failed to load '{filePath}': {fileLoadException.Message}");
        }
        catch (Exception exception)
        {
            MelonLogger.Warning($"ReferenceScanner unexpected error for '{filePath}': {exception.Message}");
        }
    }
}
