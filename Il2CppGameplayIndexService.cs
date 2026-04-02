using System;
using System.Collections.Generic;
using System.IO;

namespace AssetExporter
{
    public sealed class Il2CppGameplayIndexService
    {
        private static readonly Dictionary<string, string[]> Categories = new Dictionary<string, string[]>
        {
            { "UI", new[] { "ui", "canvas", "textmeshpro", "button", "image", "eventsystem", "panel" } },
            { "Audio", new[] { "audio", "audioclip", "audiosource", "mixer" } },
            { "Gameplay", new[] { "player", "npc", "mission", "quest", "inventory", "item", "spawn" } },
            { "Networking", new[] { "network", "packet", "socket", "rpc", "server", "client" } },
            { "Input", new[] { "input", "keyboard", "mouse", "action", "binding" } },
            { "Rendering", new[] { "mesh", "material", "texture", "shader", "renderer", "sprite" } },
            { "Triggers", new[] { "event", "trigger", "dispatch", "invoke", "callback", "notify", "on" } }
        };

        public string ExportGameplayIndex(string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            string decompiledRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "il2cpp-unpack");
            var lines = new List<string>
            {
                "# IL2CPP Gameplay Index",
                $"timestamp_utc={DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                $"source_path={decompiledRoot}",
                ""
            };

            if (!Directory.Exists(decompiledRoot))
            {
                lines.Add("status=missing_il2cpp_unpack_directory");
                string missingFile = Path.Combine(outputDirectory, "il2cpp-gameplay-index.txt");
                File.WriteAllLines(missingFile, lines);
                return missingFile;
            }

            int total = 0;
            foreach (var category in Categories)
            {
                lines.Add($"## {category.Key}");
                int categoryCount = 0;

                foreach (string file in Directory.GetFiles(decompiledRoot, "*.cs", SearchOption.AllDirectories))
                {
                    string[] content;
                    try
                    {
                        content = File.ReadAllLines(file);
                    }
                    catch
                    {
                        continue;
                    }

                    for (int i = 0; i < content.Length; i++)
                    {
                        string sourceLine = content[i];
                        if (string.IsNullOrWhiteSpace(sourceLine))
                            continue;

                        string lower = sourceLine.ToLowerInvariant();
                        if (!ContainsAny(lower, category.Value))
                            continue;

                        string rel = file.Replace(decompiledRoot + Path.DirectorySeparatorChar, string.Empty);
                        lines.Add($"{category.Key.ToLowerInvariant()} | file={rel} | line={i + 1} | text={sourceLine.Trim()}");
                        categoryCount++;
                        total++;
                    }
                }

                lines.Add($"category_entries={categoryCount}");
                lines.Add(string.Empty);
            }

            lines.Insert(3, $"entries_total={total}");
            string filePath = Path.Combine(outputDirectory, "il2cpp-gameplay-index.txt");
            File.WriteAllLines(filePath, lines);
            return filePath;
        }

        private static bool ContainsAny(string source, IEnumerable<string> keywords)
        {
            foreach (string keyword in keywords)
            {
                if (source.Contains(keyword))
                    return true;
            }

            return false;
        }
    }
}
