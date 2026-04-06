using System;
using System.Reflection;
using Il2Cpp;
using UnityEngine;

namespace FMF.HexLabelMod;

/// <summary>
/// Resolves RJ / SFP / QSFP for the cable item the player is holding, using
/// reflection on <see cref="PlayerClass"/> and related types (names vary by build).
/// </summary>
internal static class HeldCableKindResolver
{
    public static string Resolve()
    {
        try
        {
            var pc = PlayerManager.instance?.playerClass;
            if (pc == null)
                return null;

            var t = pc.GetType();
            foreach (var member in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object val = null;
                try
                {
                    if (member is FieldInfo fi)
                        val = fi.GetValue(pc);
                    else if (member is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                        val = pi.GetValue(pc);
                    else
                        continue;
                }
                catch
                {
                    continue;
                }

                if (val == null)
                    continue;

                var kind = ClassifyObject(val);
                if (kind != null)
                    return kind;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    /// <summary>
    /// Tries to read a cable color string from held / player state (matches save <c>cableColor</c> style).
    /// </summary>
    public static bool TryGetHeldCableHex(out string hex)
    {
        hex = null;
        try
        {
            var pc = PlayerManager.instance?.playerClass;
            if (pc == null)
                return false;

            foreach (var m in pc.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object val = null;
                try
                {
                    if (m is FieldInfo fi)
                        val = fi.GetValue(pc);
                    else if (m is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                        val = pi.GetValue(pc);
                    else
                        continue;
                }
                catch
                {
                    continue;
                }

                if (val == null)
                    continue;

                var name = m.Name;
                if (name.IndexOf("cable", StringComparison.OrdinalIgnoreCase) < 0
                    && name.IndexOf("held", StringComparison.OrdinalIgnoreCase) < 0
                    && name.IndexOf("item", StringComparison.OrdinalIgnoreCase) < 0
                    && name.IndexOf("inventory", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                if (TryHexFromObject(val, out hex))
                    return true;
            }

            return TryHexFromObject(pc, out hex);
        }
        catch
        {
            return false;
        }
    }

    private static bool TryHexFromObject(object o, out string hex)
    {
        hex = null;
        if (o == null)
            return false;

        if (o is string str && HexColorUtil.TryNormalizeHex(str, out hex))
            return true;

        if (o is UnityEngine.Color c)
        {
            hex = HexColorUtil.ToHex(c);
            return true;
        }

        var t = o.GetType();
        foreach (var m in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (m.Name.IndexOf("rgb", StringComparison.OrdinalIgnoreCase) < 0
                && m.Name.IndexOf("color", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            object val = null;
            try
            {
                if (m is FieldInfo fi)
                    val = fi.GetValue(o);
                else if (m is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                    val = pi.GetValue(o);
                else
                    continue;
            }
            catch
            {
                continue;
            }

            if (val is string s2 && HexColorUtil.TryNormalizeHex(s2, out hex))
                return true;
        }

        return false;
    }

    private static string ClassifyObject(object o)
    {
        if (o is string s)
            return ClassifyString(s);

        var s2 = o.ToString();
        if (!string.IsNullOrEmpty(s2))
        {
            var k = ClassifyString(s2);
            if (k != null)
                return k;
        }

        var t = o.GetType();
        foreach (var member in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            object val = null;
            try
            {
                if (member is FieldInfo fi)
                    val = fi.GetValue(o);
                else if (member is PropertyInfo pi && pi.GetIndexParameters().Length == 0)
                    val = pi.GetValue(o);
                else
                    continue;
            }
            catch
            {
                continue;
            }

            if (val is string str)
            {
                var k = ClassifyString(str);
                if (k != null)
                    return k;
            }
        }

        return null;
    }

    private static string ClassifyString(string s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        var u = s.ToUpperInvariant();
        if (u.Contains("QSFP", StringComparison.Ordinal))
            return "QSFP";
        if (u.Contains("SFP", StringComparison.Ordinal))
            return "SFP";
        if (u.Contains("RJ45", StringComparison.Ordinal) || u.Contains("RJ-45", StringComparison.Ordinal))
            return "RJ45";
        if (u == "RJ" || u.Contains("RJ ", StringComparison.Ordinal) || u.EndsWith("RJ", StringComparison.Ordinal))
            return "RJ45";

        return null;
    }
}
