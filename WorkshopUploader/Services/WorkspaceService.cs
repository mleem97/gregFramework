using System.Text.Json;
using Steamworks;
using WorkshopUploader.Models;
using WorkshopUploader.Steam;

namespace WorkshopUploader.Services;

public sealed class WorkspaceService
{
	public const string CustomWorkspacePathKey = "CustomWorkspacePath";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
	};

	private static readonly string LegacyFallbackPath =
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DataCenterWS");

	private readonly SteamWorkshopService? _steam;
	private string? _cachedWorkspaceRoot;

	/// <summary>GUI: resolves workspace via Custom > Game > Fallback. Headless: use parameterless ctor.</summary>
	public WorkspaceService(SteamWorkshopService steam)
	{
		_steam = steam;
	}

	/// <summary>Headless CLI — no Steam injection.</summary>
	public WorkspaceService()
	{
	}

	/// <summary>
	/// Resolution order: 1) User-configured custom path (Settings), 2) <c>&lt;GameRoot&gt;/workshop</c> via Steam, 3) <c>%USERPROFILE%/DataCenterWS</c>.
	/// </summary>
	public string WorkspaceRoot
	{
		get
		{
			if (_cachedWorkspaceRoot != null)
			{
				return _cachedWorkspaceRoot;
			}

			var custom = Preferences.Default.Get(CustomWorkspacePathKey, "");
			if (!string.IsNullOrWhiteSpace(custom) && Directory.Exists(custom))
			{
				_cachedWorkspaceRoot = custom;
				return _cachedWorkspaceRoot;
			}

			_steam?.EnsureInitialized(null);
			var fromGame = TryGetGameWorkshopDirectory();
			_cachedWorkspaceRoot = !string.IsNullOrEmpty(fromGame)
				? fromGame!
				: LegacyFallbackPath;
			return _cachedWorkspaceRoot;
		}
	}

	/// <summary>Clears cached root so the next access re-resolves (call after changing the custom path preference).</summary>
	public void InvalidateCache() => _cachedWorkspaceRoot = null;

	/// <summary>Moves all project folders from the legacy <c>DataCenterWS</c> location into the current <see cref="WorkspaceRoot"/>. Returns number of folders moved.</summary>
	public int MigrateLegacyProjects()
	{
		var target = WorkspaceRoot;
		if (string.Equals(Path.GetFullPath(LegacyFallbackPath), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
			return 0;

		if (!Directory.Exists(LegacyFallbackPath))
			return 0;

		var moved = 0;
		foreach (var dir in Directory.EnumerateDirectories(LegacyFallbackPath))
		{
			var name = Path.GetFileName(dir);
			if (name.StartsWith(".", StringComparison.Ordinal)) continue;

			var metaFile = Path.Combine(dir, "metadata.json");
			var contentDir = Path.Combine(dir, "content");
			if (!File.Exists(metaFile) && !Directory.Exists(contentDir)) continue;

			var dest = Path.Combine(target, name);
			if (Directory.Exists(dest)) continue;

			try
			{
				Directory.CreateDirectory(target);
				Directory.Move(dir, dest);
				moved++;
			}
			catch
			{
				// skip folders that can't be moved (locked, permissions, etc.)
			}
		}

		if (moved > 0)
		{
			try
			{
				var remaining = Directory.EnumerateFileSystemEntries(LegacyFallbackPath).Any();
				if (!remaining)
					Directory.Delete(LegacyFallbackPath, false);
			}
			catch
			{
				// legacy folder still in use or not empty — leave it
			}
		}

		return moved;
	}

	private static string? TryGetGameWorkshopDirectory()
	{
		if (!SteamClient.IsValid)
		{
			return null;
		}

		try
		{
			var install = SteamApps.AppInstallDir(SteamConstants.DataCenterAppId);
			if (string.IsNullOrWhiteSpace(install))
			{
				return null;
			}

			return Path.Combine(install.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), "workshop");
		}
		catch
		{
			return null;
		}
	}

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

	private static readonly string[] PreviewExtensions = [".png", ".jpg", ".jpeg", ".gif", ".webp"];

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

		AutoDetectPreviewImage(projectRoot, meta);

		return meta;
	}

	/// <summary>
	/// If the configured preview image doesn't exist, scan for common formats.
	/// </summary>
	private static void AutoDetectPreviewImage(string projectRoot, WorkshopMetadata meta)
	{
		var configured = Path.Combine(projectRoot, meta.PreviewImageRelativePath);
		if (File.Exists(configured)) return;

		foreach (var ext in PreviewExtensions)
		{
			var candidate = Path.Combine(projectRoot, $"preview{ext}");
			if (File.Exists(candidate))
			{
				meta.PreviewImageRelativePath = $"preview{ext}";
				return;
			}
		}
	}

	/// <summary>Creates <c>content/</c>, <c>metadata.json</c>, and template readmes. <paramref name="folderName"/> is sanitized for a single folder segment.</summary>
	public string CreateTemplateProject(string folderName, WorkshopTemplateKind kind)
	{
		EnsureWorkspaceStructure();
		var dirName = SanitizeFolderName(folderName);
		if (string.IsNullOrEmpty(dirName))
		{
			throw new ArgumentException("Project folder name is empty or invalid.");
		}

		var root = Path.Combine(WorkspaceRoot, dirName);
		if (Directory.Exists(root))
		{
			throw new InvalidOperationException($"A project folder named \"{dirName}\" already exists.");
		}

		var content = Path.Combine(root, "content");
		Directory.CreateDirectory(content);

		switch (kind)
		{
			case WorkshopTemplateKind.VanillaObjectDecoration:
				CreateVanillaObjectDecorationTemplate(content);
				break;
			case WorkshopTemplateKind.ModdedMelonLoader:
				CreateMelonLoaderTemplate(content);
				break;
			case WorkshopTemplateKind.ModdedFrikaModFramework:
				CreateFrikaModFrameworkTemplate(content);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
		}

		var meta = BuildMetadataForTemplate(dirName, kind);
		SaveMetadata(root, meta);
		return root;
	}

	private static WorkshopMetadata BuildMetadataForTemplate(string title, WorkshopTemplateKind kind)
	{
		var meta = new WorkshopMetadata
		{
			Title = title,
			PreviewImageRelativePath = "preview.png",
		};

		switch (kind)
		{
			case WorkshopTemplateKind.VanillaObjectDecoration:
				meta.Description =
					"[h1]Custom Items for Data Center[/h1]\n" +
					"Adds custom objects to Data Center using the native mod system.\n\n" +
					"[h2]Contents[/h2]\n" +
					"[list]\n[*] Custom shop items (purchasable in-game)\n[/list]\n\n" +
					"[h2]Installation[/h2]\n" +
					"Subscribe to this Workshop item — the game loads it automatically. No additional tools required.\n\n" +
					"[h2]Mod Format[/h2]\n" +
					"This mod uses the native Data Center mod system ([b]config.json[/b] + OBJ/PNG assets).\n\n" +
					"[hr][/hr]\n" +
					"[i]Replace this placeholder description with your own.[/i]";
				meta.Tags.AddRange(new[] { "vanilla", "object", "decoration" });
				break;
			case WorkshopTemplateKind.ModdedMelonLoader:
				meta.Description =
					"[h1]MelonLoader Mod[/h1]\n" +
					"A MelonLoader mod for Data Center.\n\n" +
					"[h2]Features[/h2]\n" +
					"[list]\n[*] Feature 1\n[*] Feature 2\n[/list]\n\n" +
					"[h2]Requirements[/h2]\n" +
					"[list]\n[*] [url=https://melonwiki.xyz]MelonLoader[/url]\n[/list]\n\n" +
					"[h2]Installation[/h2]\n" +
					"[list]\n[*] Install MelonLoader for Data Center\n[*] Subscribe to this Workshop item\n[*] Restart the game\n[/list]\n\n" +
					"[hr][/hr]\n" +
					"[i]Replace this placeholder description with your own.[/i]";
				meta.Tags.AddRange(new[] { "modded", "melonloader" });
				break;
			case WorkshopTemplateKind.ModdedFrikaModFramework:
				meta.Description =
					"[h1]FrikaModFramework Plugin[/h1]\n" +
					"A FrikaModFramework plugin for Data Center.\n\n" +
					"[h2]Features[/h2]\n" +
					"[list]\n[*] Feature 1\n[*] Feature 2\n[/list]\n\n" +
					"[h2]Requirements[/h2]\n" +
					"[list]\n[*] [url=https://melonwiki.xyz]MelonLoader[/url]\n[*] [url=https://gregframework.eu]FrikaModFramework[/url]\n[/list]\n\n" +
					"[h2]Installation[/h2]\n" +
					"[list]\n[*] Install MelonLoader for Data Center\n[*] Install FrikaModFramework\n[*] Subscribe to this Workshop item\n[*] Restart the game\n[/list]\n\n" +
					"[hr][/hr]\n" +
					"[i]Replace this placeholder description with your own.[/i]";
				meta.Tags.AddRange(new[] { "modded", "fmf", "frika-mod-framework" });
				break;
		}

		return meta;
	}

	private static void CreateVanillaObjectDecorationTemplate(string contentRoot)
	{
		Directory.CreateDirectory(contentRoot);

		const string sampleConfig = """
			{
			  "modName": "My Workshop Mod",
			  "shopItems": [
			    {
			      "itemName": "Custom Server",
			      "price": 500,
			      "xpToUnlock": 0,
			      "sizeInU": 2,
			      "mass": 5.0,
			      "modelScale": 1.0,
			      "colliderSize": [0.5, 0.5, 0.5],
			      "colliderCenter": [0.0, 0.0, 0.0],
			      "modelFile": "server.obj",
			      "textureFile": "server.png",
			      "iconFile": "server_icon.png",
			      "objectType": 0
			    }
			  ],
			  "staticItems": [],
			  "dlls": []
			}
			""";

		File.WriteAllText(Path.Combine(contentRoot, "config.json"), sampleConfig.Trim());

		File.WriteAllText(
			Path.Combine(contentRoot, "README.txt"),
			"""
			Native Data Center Mod (Vanilla)

			This folder uses the game's built-in mod system.

			Structure:
			  config.json        — mod definition (items, assets, optional DLLs)
			  *.obj              — 3D models (Wavefront OBJ)
			  *.png              — textures and shop icons

			config.json fields:
			  modName            — display name of your mod
			  shopItems[]        — purchasable objects for the in-game shop
			  staticItems[]      — statically placed decorations
			  dlls[]             — optional plugin DLLs (untested/experimental)

			shopItems fields:
			  itemName, price, xpToUnlock, sizeInU, mass, modelScale,
			  colliderSize [x,y,z], colliderCenter [x,y,z],
			  modelFile (.obj), textureFile (.png), iconFile (.png),
			  objectType (0 = passive object)

			staticItems fields:
			  itemName, modelFile, textureFile, modelScale,
			  colliderSize [x,y,z], colliderCenter [x,y,z],
			  position [x,y,z], rotation [x,y,z]

			Place your .obj models, .png textures, and icon files in this folder
			alongside config.json.

			Do not ship game binaries — only your own assets.
			""".Trim());
	}

	private static void CreateMelonLoaderTemplate(string contentRoot)
	{
		var mods = Path.Combine(contentRoot, "Mods");
		Directory.CreateDirectory(mods);

		File.WriteAllText(
			Path.Combine(mods, "README.txt"),
			"""
			MelonLoader mods

			Place your MelonLoader mod DLL(s) here. When players install your Workshop item, this maps to the game's Mods folder layout.

			Do not redistribute the game or MelonLoader; only your mod assemblies and your own assets.
			""".Trim());
	}

	private static void CreateFrikaModFrameworkTemplate(string contentRoot)
	{
		var plugins = Path.Combine(contentRoot, "FMF", "Plugins");
		Directory.CreateDirectory(plugins);

		File.WriteAllText(
			Path.Combine(plugins, "README.txt"),
			"""
			FrikaModFramework plugins

			Place FMF plugin DLLs (FFM.Plugin.*) in this folder. This mirrors FMF/Plugins next to the game executable.

			Do not ship game binaries; only your plugin DLLs and allowed content.
			""".Trim());
	}

	/// <summary>Copies all files from <paramref name="sourceDir"/> into <paramref name="destDir"/> (recursive).</summary>
	public static void CopyDirectoryRecursive(string sourceDir, string destDir)
	{
		if (!Directory.Exists(sourceDir))
		{
			throw new DirectoryNotFoundException(sourceDir);
		}

		Directory.CreateDirectory(destDir);
		foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
		{
			var rel = Path.GetRelativePath(sourceDir, file);
			var target = Path.Combine(destDir, rel);
			var targetDir = Path.GetDirectoryName(target);
			if (!string.IsNullOrEmpty(targetDir))
			{
				Directory.CreateDirectory(targetDir);
			}

			File.Copy(file, target, overwrite: true);
		}
	}

	/// <summary>Total size of <c>content/</c> and largest direct children.</summary>
	public ContentStats GetContentStats(string projectRoot)
	{
		var content = Path.Combine(projectRoot, "content");
		if (!Directory.Exists(content))
		{
			return new ContentStats { Exists = false };
		}

		var total = GetDirectorySizeRecursive(content);
		var entries = new List<ContentFolderSize>();

		foreach (var file in Directory.EnumerateFiles(content))
		{
			try
			{
				var len = new FileInfo(file).Length;
				entries.Add(new ContentFolderSize(Path.GetFileName(file), len));
			}
			catch
			{
				// ignored
			}
		}

		foreach (var dir in Directory.EnumerateDirectories(content))
		{
			var name = Path.GetFileName(dir) + "/";
			entries.Add(new ContentFolderSize(name, GetDirectorySizeRecursive(dir)));
		}

		var top = entries.OrderByDescending(e => e.Bytes).Take(16).ToList();
		return new ContentStats
		{
			Exists = true,
			TotalBytes = total,
			TopEntries = top,
		};
	}

	private static long GetDirectorySizeRecursive(string dir)
	{
		long n = 0;
		try
		{
			foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
			{
				try
				{
					n += new FileInfo(f).Length;
				}
				catch
				{
					// ignored
				}
			}
		}
		catch
		{
			// ignored
		}

		return n;
	}

	public static string FormatBytes(long bytes)
	{
		if (bytes < 0)
		{
			bytes = 0;
		}

		string[] suf = { "B", "KB", "MB", "GB", "TB" };
		double v = bytes;
		var i = 0;
		while (v >= 1024 && i < suf.Length - 1)
		{
			v /= 1024;
			i++;
		}

		return $"{v:0.##} {suf[i]}";
	}

	/// <summary>Single path segment under <see cref="WorkspaceRoot"/>; strips invalid characters.</summary>
	public string SanitizeFolderName(string raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			return string.Empty;
		}

		var trimmed = raw.Trim();
		var invalid = Path.GetInvalidFileNameChars();
		var chars = trimmed.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
		var s = new string(chars);
		while (s.Contains("..", StringComparison.Ordinal))
		{
			s = s.Replace("..", "_", StringComparison.Ordinal);
		}

		s = s.Trim('.', ' ');
		if (s.Length > 80)
		{
			s = s[..80].TrimEnd();
		}

		return s;
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
