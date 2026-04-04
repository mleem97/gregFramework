using MelonLoader;
using System;
using DataCenterModLoader;

namespace FrikaMF;

public static class FrameworkLog
{
    private static MelonLogger.Instance _logger;

    public static void Initialize(MelonLogger.Instance logger)
    {
        _logger = logger;
    }

    public static void Info(string category, string message)
    {
        string line = Format("INFO", category, message);
        _logger?.Msg(line);
        CrashLog.Log(line);
    }

    public static void Warn(string category, string message)
    {
        string line = Format("WARN", category, message);
        _logger?.Warning(line);
        CrashLog.Log(line);
    }

    public static void Error(string category, string message)
    {
        string line = Format("ERROR", category, message);
        _logger?.Error(line);
        CrashLog.Log(line);
    }

    public static void Debug(string category, string message)
    {
        string line = Format("DEBUG", category, message);
        _logger?.Msg(line);
        CrashLog.Log(line);
    }

    public static void Exception(string category, string context, Exception exception)
    {
        if (exception == null)
        {
            Error(category, context);
            return;
        }

        Error(category, $"{context}: {exception.Message}");
        CrashLog.LogException($"[{category}] {context}", exception);
    }

    private static string Format(string level, string category, string message)
    {
        string safeCategory = string.IsNullOrWhiteSpace(category) ? "general" : category.Trim().ToLowerInvariant();
        return $"[FMF/{level}/{safeCategory}] {message}";
    }
}
