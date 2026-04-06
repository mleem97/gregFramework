using System.Text.Json;
using WorkshopUploader.Models;
using WorkshopUploader.Steam;

namespace WorkshopUploader.Services;

public sealed class WorkspaceService
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
	};

	public string WorkspaceRoot { get; } = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		"DataCenterWS");

	public string TemplatesDirectory => Path.Combine(WorkspaceRoot, ".templates");

	public void EnsureWorkspaceStructure()
	{
		Directory.CreateDirectory(WorkspaceRoot);
		Directory.CreateDirectory(TemplatesDirectory);

		var sample = Path.Combine(TemplatesDirectory, "metadata.sample.json");
		if (!File.Exists(sample))
		{
			var meta = new WorkshopMetadata
			{
				Title = "My Mod",
				Description = "Description",
				Visibility = "Public",
				PreviewImageRelativePath = "preview.png",
			};
			File.WriteAllText(sample, JsonSerializer.Serialize(meta, JsonOptions));
		}
	}

	public IReadOnlyList<WorkshopProject> ScanProjects()
	{
		EnsureWorkspaceStructure();
		if (!Directory.Exists(WorkspaceRoot))
		{
			return Array.Empty<WorkshopProject>();
		}

		var list = new List<WorkshopProject>();
		foreach (var dir in Directory.EnumerateDirectories(WorkspaceRoot))
		{
			var name = Path.GetFileName(dir);
			if (name.StartsWith(".", StringComparison.Ordinal))
			{
				continue;
			}

			var content = Path.Combine(dir, "content");
			var metadataPath = Path.Combine(dir, "metadata.json");
			var hasContent = Directory.Exists(content);
			list.Add(new WorkshopProject
			{
				Name = name,
				RootPath = dir,
				ContentPath = content,
				MetadataPath = metadataPath,
				IsValidLayout = hasContent,
			});
		}

		return list.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
	}

	public WorkshopMetadata LoadMetadata(string projectRoot)
	{
		var path = Path.Combine(projectRoot, "metadata.json");
		if (!File.Exists(path))
		{
			return new WorkshopMetadata();
		}

		var json = File.ReadAllText(path);
		var meta = JsonSerializer.Deserialize<WorkshopMetadata>(json) ?? new WorkshopMetadata();
		meta.Title ??= string.Empty;
		meta.Description ??= string.Empty;
		return meta;
	}

	public void SaveMetadata(string projectRoot, WorkshopMetadata metadata)
	{
		var title = metadata.Title ?? string.Empty;
		var description = metadata.Description ?? string.Empty;
		if (title.Length > SteamConstants.MaxTitleLength)
		{
			throw new InvalidOperationException($"Title exceeds {SteamConstants.MaxTitleLength} characters.");
		}

		if (description.Length > SteamConstants.MaxDescriptionLength)
		{
			throw new InvalidOperationException($"Description exceeds {SteamConstants.MaxDescriptionLength} characters.");
		}

		var path = Path.Combine(projectRoot, "metadata.json");
		File.WriteAllText(path, JsonSerializer.Serialize(metadata, JsonOptions));
	}
}
