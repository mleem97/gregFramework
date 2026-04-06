using WorkshopUploader.Models;
using WorkshopUploader.Steam;

namespace WorkshopUploader.Services;

public static class UploadDependencyChecker
{
	public static List<UploadCheckResult> Check(string projectRoot, WorkshopMetadata metadata, string? changeLog = null)
	{
		var results = new List<UploadCheckResult>();

		CheckContentFolder(projectRoot, results);
		CheckMetadataFields(metadata, results);
		CheckPreviewImage(projectRoot, metadata, results);
		CheckTags(metadata, results);
		CheckContentSize(projectRoot, results);
		CheckChangelog(metadata, changeLog, results);

		return results;
	}

	public static bool IsReadyToUpload(List<UploadCheckResult> results)
	{
		return results.All(r => r.Severity != UploadCheckSeverity.Error);
	}

	private static void CheckContentFolder(string projectRoot, List<UploadCheckResult> results)
	{
		var contentDir = Path.Combine(projectRoot, "content");
		if (!Directory.Exists(contentDir))
		{
			results.Add(new UploadCheckResult
			{
				Label = "Content folder",
				Severity = UploadCheckSeverity.Error,
				Detail = "content/ folder is missing. Create it and add your files before uploading.",
			});
			return;
		}

		var fileCount = 0;
		foreach (var _ in Directory.EnumerateFiles(contentDir, "*", SearchOption.AllDirectories))
		{
			fileCount++;
			if (fileCount > 0) break;
		}

		if (fileCount == 0)
		{
			results.Add(new UploadCheckResult
			{
				Label = "Content folder",
				Severity = UploadCheckSeverity.Error,
				Detail = "content/ folder is empty. Add files to upload.",
			});
		}
		else
		{
			var total = Directory.EnumerateFiles(contentDir, "*", SearchOption.AllDirectories).Count();
			results.Add(new UploadCheckResult
			{
				Label = "Content folder",
				Severity = UploadCheckSeverity.Ok,
				Detail = $"content/ contains {total} file(s).",
			});
		}
	}

	private static void CheckMetadataFields(WorkshopMetadata metadata, List<UploadCheckResult> results)
	{
		if (string.IsNullOrWhiteSpace(metadata.Title))
		{
			results.Add(new UploadCheckResult
			{
				Label = "Title",
				Severity = UploadCheckSeverity.Error,
				Detail = "Title is empty. Steam requires a title.",
			});
		}
		else if (metadata.Title.Length > SteamConstants.MaxTitleLength)
		{
			results.Add(new UploadCheckResult
			{
				Label = "Title",
				Severity = UploadCheckSeverity.Error,
				Detail = $"Title exceeds {SteamConstants.MaxTitleLength} characters ({metadata.Title.Length}).",
			});
		}
		else
		{
			results.Add(new UploadCheckResult
			{
				Label = "Title",
				Severity = UploadCheckSeverity.Ok,
				Detail = $"\"{metadata.Title}\" ({metadata.Title.Length}/{SteamConstants.MaxTitleLength})",
			});
		}

		if (string.IsNullOrWhiteSpace(metadata.Description))
		{
			results.Add(new UploadCheckResult
			{
				Label = "Description",
				Severity = UploadCheckSeverity.Warning,
				Detail = "Description is empty. Recommended for discoverability.",
			});
		}
		else if (metadata.Description.Length > SteamConstants.MaxDescriptionLength)
		{
			results.Add(new UploadCheckResult
			{
				Label = "Description",
				Severity = UploadCheckSeverity.Error,
				Detail = $"Description exceeds {SteamConstants.MaxDescriptionLength} characters.",
			});
		}
		else
		{
			results.Add(new UploadCheckResult
			{
				Label = "Description",
				Severity = UploadCheckSeverity.Ok,
				Detail = $"{metadata.Description.Length}/{SteamConstants.MaxDescriptionLength} characters.",
			});
		}

		var validVisibilities = new[] { "Public", "FriendsOnly", "Private" };
		if (!validVisibilities.Contains(metadata.Visibility))
		{
			results.Add(new UploadCheckResult
			{
				Label = "Visibility",
				Severity = UploadCheckSeverity.Warning,
				Detail = $"Unknown visibility \"{metadata.Visibility}\". Expected: Public, FriendsOnly, or Private.",
			});
		}
		else
		{
			results.Add(new UploadCheckResult
			{
				Label = "Visibility",
				Severity = UploadCheckSeverity.Ok,
				Detail = metadata.Visibility,
			});
		}
	}

	private static void CheckPreviewImage(string projectRoot, WorkshopMetadata metadata, List<UploadCheckResult> results)
	{
		if (string.IsNullOrWhiteSpace(metadata.PreviewImageRelativePath))
		{
			results.Add(new UploadCheckResult
			{
				Label = "Preview image",
				Severity = UploadCheckSeverity.Warning,
				Detail = "No preview image path set. Steam shows a placeholder without one.",
			});
			return;
		}

		var previewPath = Path.Combine(projectRoot, metadata.PreviewImageRelativePath);
		if (!File.Exists(previewPath))
		{
			results.Add(new UploadCheckResult
			{
				Label = "Preview image",
				Severity = UploadCheckSeverity.Warning,
				Detail = $"File not found: {metadata.PreviewImageRelativePath}. Add a preview image (PNG/JPG recommended).",
			});
		}
		else
		{
			var size = new FileInfo(previewPath).Length;
			if (size > 1_048_576)
			{
				results.Add(new UploadCheckResult
				{
					Label = "Preview image",
					Severity = UploadCheckSeverity.Warning,
					Detail = $"Preview image is {WorkspaceService.FormatBytes(size)} — Steam recommends under 1 MB.",
				});
			}
			else
			{
				results.Add(new UploadCheckResult
				{
					Label = "Preview image",
					Severity = UploadCheckSeverity.Ok,
					Detail = $"{metadata.PreviewImageRelativePath} ({WorkspaceService.FormatBytes(size)})",
				});
			}
		}
	}

	private static void CheckTags(WorkshopMetadata metadata, List<UploadCheckResult> results)
	{
		if (metadata.Tags.Count == 0)
		{
			results.Add(new UploadCheckResult
			{
				Label = "Tags",
				Severity = UploadCheckSeverity.Warning,
				Detail = "No tags set. Tags help users find your content.",
			});
		}
		else
		{
			results.Add(new UploadCheckResult
			{
				Label = "Tags",
				Severity = UploadCheckSeverity.Ok,
				Detail = string.Join(", ", metadata.Tags),
			});
		}
	}

	private static void CheckContentSize(string projectRoot, List<UploadCheckResult> results)
	{
		var contentDir = Path.Combine(projectRoot, "content");
		if (!Directory.Exists(contentDir)) return;

		long totalBytes = 0;
		try
		{
			foreach (var f in Directory.EnumerateFiles(contentDir, "*", SearchOption.AllDirectories))
			{
				totalBytes += new FileInfo(f).Length;
			}
		}
		catch
		{
			return;
		}

		const long warnThreshold = 100L * 1024 * 1024;
		if (totalBytes > warnThreshold)
		{
			results.Add(new UploadCheckResult
			{
				Label = "Content size",
				Severity = UploadCheckSeverity.Warning,
				Detail = $"Total content size is {WorkspaceService.FormatBytes(totalBytes)}. Large uploads take longer.",
			});
		}
		else
		{
			results.Add(new UploadCheckResult
			{
				Label = "Content size",
				Severity = UploadCheckSeverity.Ok,
				Detail = WorkspaceService.FormatBytes(totalBytes),
			});
		}
	}

	private static void CheckChangelog(WorkshopMetadata metadata, string? changeLog, List<UploadCheckResult> results)
	{
		var isFirstPublish = metadata.PublishedFileId == 0;
		var hasChangelog = !string.IsNullOrWhiteSpace(changeLog);

		if (isFirstPublish && !hasChangelog)
		{
			results.Add(new UploadCheckResult
			{
				Label = "Changelog",
				Severity = UploadCheckSeverity.Error,
				Detail = "A version changelog is required for the first publish. Describe your initial release.",
			});
		}
		else if (!isFirstPublish && !hasChangelog)
		{
			results.Add(new UploadCheckResult
			{
				Label = "Changelog",
				Severity = UploadCheckSeverity.Warning,
				Detail = "No changelog provided. Recommended so subscribers know what changed.",
			});
		}
		else
		{
			results.Add(new UploadCheckResult
			{
				Label = "Changelog",
				Severity = UploadCheckSeverity.Ok,
				Detail = $"{changeLog!.Length} characters.",
			});
		}
	}
}
