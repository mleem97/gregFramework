using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(FMF.GregifyEmployees.Main), "FMF Gregify Employees", "00.01.0009", "mleem97")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace FMF.GregifyEmployees;

public sealed class Main : MelonMod
{
    private const string RgbGregEmployeeId = "greg_rgb_star";
    private const double RgbGregPrice = 1_000_000_000_000_000d;

    private HarmonyLib.Harmony _harmony;
    private bool _frameworkReady;
    private float _nextReskinAt;
    private bool _rgbGregPurchased;
    private Sprite _gregSprite;

    private static Main _instance;

    public override void OnInitializeMelon()
    {
        _instance = this;
        _frameworkReady = IsFrameworkLoaded();
        if (!_frameworkReady)
        {
            LoggerInstance.Error("FMF Gregify Employees requires FrikaModdingFramework.dll.");
            return;
        }

        _harmony = new HarmonyLib.Harmony("fmf.gregifyemployees");
        _harmony.PatchAll(typeof(Main).Assembly);
        RegisterRuntimePatches();

        TryLoadGregSprite();
        RegisterRgbGregHire();
        LogAvailableHires();

        LoggerInstance.Msg("FMF Gregify Employees initialized.");
    }

    public override void OnUpdate()
    {
        if (!_frameworkReady)
            return;

        if (Time.time >= _nextReskinAt)
        {
            _nextReskinAt = Time.time + 1.5f;
            ApplyGregToAllEmployees();
        }

        if (_rgbGregPurchased)
            ApplyRgbOverlay();
    }

    private static bool IsFrameworkLoaded()
    {
        return AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
        {
            string name = assembly.GetName().Name ?? string.Empty;
            return string.Equals(name, "FrikaModdingFramework", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "FrikaMF", StringComparison.OrdinalIgnoreCase);
        });
    }

    private void RegisterRgbGregHire()
    {
        try
        {
            Type managerType = Type.GetType("DataCenterModLoader.CustomEmployeeManager, FrikaModdingFramework", throwOnError: false)
                ?? Type.GetType("DataCenterModLoader.CustomEmployeeManager, FrikaMF", throwOnError: false);

            MethodInfo registerMethod = managerType?.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
            registerMethod?.Invoke(null, new object[]
            {
                RgbGregEmployeeId,
                "RGB Greg",
                "A glowing premium Greg. Costs 1 Billiarde.",
                1f,
                0f,
                true
            });
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"RGB Greg registration failed: {ex.Message}");
        }
    }

    private void LogAvailableHires()
    {
        try
        {
            Type serviceType = Type.GetType("DataCenterModLoader.HireRosterService, FrikaModdingFramework", throwOnError: false)
                ?? Type.GetType("DataCenterModLoader.HireRosterService, FrikaMF", throwOnError: false);

            MethodInfo snapshotMethod = serviceType?.GetMethod("GetAvailableHiresSnapshot", BindingFlags.Public | BindingFlags.Static);
            object value = snapshotMethod?.Invoke(null, null);
            if (value is not System.Collections.IEnumerable hires)
                return;

            int shown = 0;
            foreach (object hire in hires)
            {
                if (hire == null)
                    continue;

                string id = hire.GetType().GetProperty("HireId")?.GetValue(hire)?.ToString() ?? "unknown";
                string name = hire.GetType().GetProperty("Name")?.GetValue(hire)?.ToString() ?? "unknown";
                string source = hire.GetType().GetProperty("Source")?.GetValue(hire)?.ToString() ?? "unknown";

                LoggerInstance.Msg($" - {id} | {name} | {source}");
                shown++;
                if (shown >= 20)
                    break;
            }

            LoggerInstance.Msg($"Gregify hires snapshot loaded (showing up to 20 entries). Count displayed={shown}.");
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"AvailableHires snapshot failed: {ex.Message}");
        }
    }

    private void TryLoadGregSprite()
    {
        try
        {
            string localImagePath = Path.Combine(Path.GetDirectoryName(typeof(Main).Assembly.Location) ?? string.Empty, "image.png");
            if (!File.Exists(localImagePath))
            {
                LoggerInstance.Warning($"Greg image not found at '{localImagePath}'. UI image replacement disabled.");
                return;
            }

            var bytes = File.ReadAllBytes(localImagePath);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, bytes))
            {
                UnityEngine.Object.Destroy(texture);
                LoggerInstance.Warning("Could not decode image.png for Greg sprite.");
                return;
            }

            _gregSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            _gregSprite.name = "GregEmployeeSprite";
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"Greg sprite load failed: {ex.Message}");
        }
    }

    private void ApplyGregToAllEmployees()
    {
        try
        {
            var tm = TechnicianManager.instance;
            if (tm == null || tm.technicians == null || tm.technicians.Count == 0)
                return;

            Technician gregSource = tm.technicians[0];
            if (gregSource == null)
                return;

            var sourceRenderers = gregSource.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (sourceRenderers == null || sourceRenderers.Length == 0)
                return;

            for (int index = 0; index < tm.technicians.Count; index++)
            {
                Technician target = tm.technicians[index];
                if (target == null || target == gregSource)
                    continue;

                ApplyGregModel(target, sourceRenderers);
            }

            if (_gregSprite != null)
                ApplyGregImages();
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"Gregify apply failed: {ex.Message}");
        }
    }

    private static void ApplyGregModel(Technician target, SkinnedMeshRenderer[] sourceRenderers)
    {
        var targetRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (targetRenderers == null || targetRenderers.Length == 0)
            return;

        int max = Math.Min(targetRenderers.Length, sourceRenderers.Length);
        for (int i = 0; i < max; i++)
        {
            var source = sourceRenderers[i];
            var destination = targetRenderers[i];
            if (source == null || destination == null)
                continue;

            destination.sharedMesh = source.sharedMesh;
            destination.sharedMaterials = source.sharedMaterials;
            destination.updateWhenOffscreen = true;
        }
    }

    private void ApplyGregImages()
    {
        var images = UnityEngine.Object.FindObjectsOfType<Image>(true);
        if (images == null)
            return;

        foreach (var image in images)
        {
            if (image == null)
                continue;

            string name = image.gameObject.name ?? string.Empty;
            if (!IsEmployeeUiName(name))
                continue;

            image.sprite = _gregSprite;
            image.preserveAspect = true;
        }
    }

    private static bool IsEmployeeUiName(string name)
    {
        return name.Contains("employee", StringComparison.OrdinalIgnoreCase)
            || name.Contains("technician", StringComparison.OrdinalIgnoreCase)
            || name.Contains("portrait", StringComparison.OrdinalIgnoreCase)
            || name.Contains("card", StringComparison.OrdinalIgnoreCase);
    }

    private static double GetPlayerMoney()
    {
        try
        {
            return PlayerManager.instance?.playerClass?.money ?? 0d;
        }
        catch
        {
            return 0d;
        }
    }

    private static bool TryDeductMoney(double amount)
    {
        try
        {
            var player = PlayerManager.instance?.playerClass;
            if (player == null)
                return false;

            double current = player.money;
            if (current < amount)
                return false;

            player.money = (float)(current - amount);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ApplyRgbOverlay()
    {
        try
        {
            float hue = Mathf.Repeat(Time.time * 0.2f, 1f);
            Color rgb = Color.HSVToRGB(hue, 1f, 1f);
            Color emission = rgb * (2.2f + Mathf.PingPong(Time.time * 2f, 1.5f));

            var renderers = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>(true);
            if (renderers == null)
                return;

            foreach (var renderer in renderers)
            {
                if (renderer == null)
                    continue;

                string name = renderer.gameObject.name ?? string.Empty;
                if (!name.Contains("technician", StringComparison.OrdinalIgnoreCase)
                    && !name.Contains("employee", StringComparison.OrdinalIgnoreCase)
                    && !name.Contains("greg", StringComparison.OrdinalIgnoreCase))
                    continue;

                var materials = renderer.materials;
                if (materials == null)
                    continue;

                foreach (var material in materials)
                {
                    if (material == null)
                        continue;

                    if (material.HasProperty("_Color"))
                        material.SetColor("_Color", Color.Lerp(Color.white, rgb, 0.45f));

                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", emission);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"RGB overlay failed: {ex.Message}");
        }
    }

    private void RegisterRuntimePatches()
    {
        try
        {
            Type managerType = Type.GetType("DataCenterModLoader.CustomEmployeeManager, FrikaModdingFramework", throwOnError: false)
                ?? Type.GetType("DataCenterModLoader.CustomEmployeeManager, FrikaMF", throwOnError: false);

            MethodInfo hireMethod = managerType?.GetMethod("Hire", BindingFlags.Public | BindingFlags.Static);
            if (hireMethod == null)
                return;

            MethodInfo prefix = typeof(Main).GetMethod(nameof(CustomEmployeeHirePrefix), BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo postfix = typeof(Main).GetMethod(nameof(CustomEmployeeHirePostfix), BindingFlags.NonPublic | BindingFlags.Static);
            _harmony.Patch(hireMethod, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
        }
        catch (Exception ex)
        {
            LoggerInstance.Warning($"Runtime patch registration failed: {ex.Message}");
        }
    }

    private static bool CustomEmployeeHirePrefix(string id, ref int __result)
    {
        if (!string.Equals(id, RgbGregEmployeeId, StringComparison.OrdinalIgnoreCase))
            return true;

        if (GetPlayerMoney() < RgbGregPrice)
        {
            _instance?.LoggerInstance.Warning("RGB Greg requires 1 Billiarde money.");
            __result = -1;
            return false;
        }

        if (!TryDeductMoney(RgbGregPrice))
        {
            __result = -1;
            return false;
        }

        return true;
    }

    private static void CustomEmployeeHirePostfix(string id, int __result)
    {
        if (!string.Equals(id, RgbGregEmployeeId, StringComparison.OrdinalIgnoreCase))
            return;

        if (__result == 1 && _instance != null)
        {
            _instance._rgbGregPurchased = true;
            _instance.LoggerInstance.Msg("RGB Greg purchased successfully.");
        }
    }

    [HarmonyPatch(typeof(HRSystem), "OnEnable")]
    private static class Patch_HR_OnEnable
    {
        private static void Postfix()
        {
            InstanceOrNull()?.ApplyGregToAllEmployees();
        }
    }

    private static Main InstanceOrNull()
    {
        return _instance;
    }
}
