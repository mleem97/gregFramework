using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FrikaMF;

internal static class AssemblyHookDumpService
{
    public static AssemblyHookDumpResult ExportAssemblyCSharpDump(string outputFilePath)
    {
        if (string.IsNullOrWhiteSpace(outputFilePath))
            throw new ArgumentException("Output file path must not be empty.", nameof(outputFilePath));

        string directory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        Assembly assemblyCSharp = ResolveAssemblyCSharp();
        if (assemblyCSharp == null)
        {
            File.WriteAllLines(outputFilePath, Array.Empty<string>());
            return new AssemblyHookDumpResult(outputFilePath, 0, 0, false);
        }

        var lines = new List<string>(capacity: 32768);
        int typeCount = 0;
        int methodCount = 0;

        Type[] types;
        try
        {
            types = assemblyCSharp.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types;
        }

        for (int typeIndex = 0; typeIndex < types.Length; typeIndex++)
        {
            Type type = types[typeIndex];
            if (type == null || string.IsNullOrWhiteSpace(type.FullName))
                continue;

            MethodInfo[] methods;
            try
            {
                methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
            }
            catch
            {
                continue;
            }

            typeCount++;
            for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++)
            {
                MethodInfo method = methods[methodIndex];
                string methodName = method.Name ?? string.Empty;
                lines.Add($"runtimetrigger asm[Assembly-CSharp] type[{type.FullName}] method[{methodName}]");
                methodCount++;
            }
        }

        lines.Sort(StringComparer.Ordinal);
        File.WriteAllLines(outputFilePath, lines);

        return new AssemblyHookDumpResult(outputFilePath, typeCount, methodCount, true);
    }

    private static Assembly ResolveAssemblyCSharp()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int index = 0; index < assemblies.Length; index++)
        {
            Assembly assembly = assemblies[index];
            string name = assembly.GetName().Name ?? string.Empty;
            if (string.Equals(name, "Assembly-CSharp", StringComparison.OrdinalIgnoreCase))
                return assembly;
        }

        return null;
    }
}

internal readonly struct AssemblyHookDumpResult
{
    public AssemblyHookDumpResult(string outputPath, int typeCount, int methodCount, bool success)
    {
        OutputPath = outputPath;
        TypeCount = typeCount;
        MethodCount = methodCount;
        Success = success;
    }

    public string OutputPath { get; }
    public int TypeCount { get; }
    public int MethodCount { get; }
    public bool Success { get; }
}
