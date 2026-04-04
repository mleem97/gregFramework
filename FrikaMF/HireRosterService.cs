using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Il2Cpp;
using MelonLoader.Utils;

namespace DataCenterModLoader;

public sealed class AvailableHireEntry
{
    public string HireId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool IsAlreadyHired { get; set; }
}

public static class HireRosterService
{
    public static List<AvailableHireEntry> GetAvailableHiresSnapshot()
    {
        var result = new List<AvailableHireEntry>();

        try
        {
            var technicians = TechnicianManager.instance?.technicians;
            if (technicians != null)
            {
                for (int index = 0; index < technicians.Count; index++)
                {
                    Technician technician = technicians[index];
                    if (technician == null)
                        continue;

                    result.Add(new AvailableHireEntry
                    {
                        HireId = $"builtin.tech.{technician.technicianID}",
                        Name = technician.technicianName ?? $"Technician {technician.technicianID}",
                        Source = "builtin",
                        IsAlreadyHired = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException("HireRosterService.GetAvailableHiresSnapshot builtin", ex);
        }

        try
        {
            foreach (var entry in CustomEmployeeManager.Employees)
            {
                if (entry == null)
                    continue;

                result.Add(new AvailableHireEntry
                {
                    HireId = entry.EmployeeId ?? string.Empty,
                    Name = entry.Name ?? "Custom Employee",
                    Source = "custom",
                    IsAlreadyHired = entry.IsHired
                });
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException("HireRosterService.GetAvailableHiresSnapshot custom", ex);
        }

        return result.OrderBy(item => item.Source).ThenBy(item => item.Name).ToList();
    }

    public static void ExportAvailableHiresSnapshot()
    {
        try
        {
            string frameworkDir = Path.Combine(MelonEnvironment.GameRootDirectory, "FrikaFM");
            Directory.CreateDirectory(frameworkDir);
            string outputPath = Path.Combine(frameworkDir, "available-hires.json");

            var snapshot = new
            {
                generatedAtUtc = DateTime.UtcNow,
                entries = GetAvailableHiresSnapshot()
            };

            string json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outputPath, json);
        }
        catch (Exception ex)
        {
            CrashLog.LogException("HireRosterService.ExportAvailableHiresSnapshot", ex);
        }
    }
}
