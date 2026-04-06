using WorkshopUploader.Services;

namespace WorkshopUploader.Models;

/// <summary>View model for a tile in the workshop project grid.</summary>
public sealed class ProjectTileVm
{
	public ProjectTileVm(WorkshopProject project, WorkspaceService workspace)
	{
		Name = project.Name;
		RootPath = project.RootPath;

		var meta = workspace.LoadMetadata(project.RootPath);
		Title = string.IsNullOrWhiteSpace(meta.Title) ? project.Name : meta.Title;
		Tags = meta.Tags.Count > 0 ? string.Join(", ", meta.Tags) : "";
		IsPublished = meta.PublishedFileId != 0;

		HasContent = project.IsValidLayout;
		HasPreview = !string.IsNullOrWhiteSpace(meta.PreviewImageRelativePath)
			&& File.Exists(Path.Combine(project.RootPath, meta.PreviewImageRelativePath));

		var checks = UploadDependencyChecker.Check(project.RootPath, meta);
		var errors = checks.Count(c => c.Severity == UploadCheckSeverity.Error);
		var warnings = checks.Count(c => c.Severity == UploadCheckSeverity.Warning);

		if (errors > 0)
			ReadinessText = $"{errors} error(s)";
		else if (warnings > 0)
			ReadinessText = $"Ready ({warnings} warning(s))";
		else
			ReadinessText = "Ready";

		ReadinessColor = errors > 0 ? "#D7383B" : warnings > 0 ? "#D7A23B" : "#61F4D8";
	}

	public string Name { get; }
	public string RootPath { get; }
	public string Title { get; }
	public string Tags { get; }
	public bool IsPublished { get; }
	public bool HasContent { get; }
	public bool HasPreview { get; }
	public string ReadinessText { get; }
	public string ReadinessColor { get; }
}
