using System.Text.Json.Serialization;

namespace WorkshopUploader.Models;

public sealed class WorkshopMetadata
{
	[JsonPropertyName("publishedFileId")]
	public ulong PublishedFileId { get; set; }

	[JsonPropertyName("title")]
	public string Title { get; set; } = "";

	[JsonPropertyName("description")]
	public string Description { get; set; } = "";

	/// <summary>Public, FriendsOnly, or Private.</summary>
	[JsonPropertyName("visibility")]
	public string Visibility { get; set; } = "Public";

	[JsonPropertyName("previewImageRelativePath")]
	public string PreviewImageRelativePath { get; set; } = "preview.png";

	[JsonPropertyName("tags")]
	public List<string> Tags { get; set; } = new();

	[JsonPropertyName("needsFmf")]
	public bool NeedsFmf { get; set; }

	[JsonPropertyName("additionalPreviews")]
	public List<string> AdditionalPreviews { get; set; } = new();
}
