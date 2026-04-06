using Steamworks;
using Steamworks.Data;
using WorkshopUploader.Models;
using WorkshopUploader.Steam;

namespace WorkshopUploader.Services;

public sealed class SteamWorkshopService
{
	private static readonly object InitLock = new();

	private bool _initialized;

	public bool EnsureInitialized(IProgress<string>? log)
	{
		lock (InitLock)
		{
			if (_initialized)
			{
				return true;
			}

			try
			{
				SteamClient.Init(SteamConstants.DataCenterAppId, true);
				_initialized = true;
				log?.Report("Steam API initialized.");
				return true;
			}
			catch (Exception ex)
			{
				log?.Report($"Steam init failed: {ex.Message}");
				return false;
			}
		}
	}

	public void Shutdown()
	{
		lock (InitLock)
		{
			if (!_initialized)
			{
				return;
			}

			try
			{
				SteamClient.Shutdown();
			}
			catch
			{
				// ignored
			}

			_initialized = false;
		}
	}

	public async Task<PublishOutcome> PublishAsync(
		string projectRoot,
		WorkshopMetadata metadata,
		string contentFolder,
		IProgress<float>? uploadProgress,
		IProgress<string>? log,
		CancellationToken cancellationToken)
	{
		if (!EnsureInitialized(log))
		{
			return PublishOutcome.Fail("Steam is not available (is Steam running?)");
		}

		cancellationToken.ThrowIfCancellationRequested();

		if (!Directory.Exists(contentFolder))
		{
			return PublishOutcome.Fail($"Content folder not found: {contentFolder}");
		}

		var title = (metadata.Title ?? string.Empty).Trim();
		var description = (metadata.Description ?? string.Empty).Trim();
		if (string.IsNullOrEmpty(title))
		{
			return PublishOutcome.Fail("Title is required.");
		}

		if (title.Length > SteamConstants.MaxTitleLength || description.Length > SteamConstants.MaxDescriptionLength)
		{
			return PublishOutcome.Fail("Title or description exceeds Steam limits.");
		}

		var previewPath = string.IsNullOrWhiteSpace(metadata.PreviewImageRelativePath)
			? null
			: Path.Combine(projectRoot, metadata.PreviewImageRelativePath);

		Steamworks.Ugc.Editor editor = metadata.PublishedFileId == 0
			? Steamworks.Ugc.Editor.NewCommunityFile
			: new Steamworks.Ugc.Editor((PublishedFileId)metadata.PublishedFileId);

		editor = editor
			.WithTitle(title)
			.WithDescription(description)
			.WithContent(contentFolder);

		if (!string.IsNullOrEmpty(previewPath) && File.Exists(previewPath))
		{
			editor = editor.WithPreviewFile(previewPath);
		}

		foreach (var tag in metadata.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
		{
			editor = editor.WithTag(tag.Trim());
		}

		editor = ApplyVisibility(editor, metadata.Visibility);

		log?.Report(metadata.PublishedFileId == 0 ? "Creating new workshop item…" : $"Updating workshop item {metadata.PublishedFileId}…");

		Steamworks.Ugc.PublishResult result;
		try
		{
			result = await editor.SubmitAsync(uploadProgress).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			return PublishOutcome.Fail(ex.Message);
		}

		if (!result.Success)
		{
			return PublishOutcome.Fail($"Steam publish failed: {result.Result}");
		}

		if (result.NeedsWorkshopAgreement)
		{
			return PublishOutcome.Fail("Workshop legal agreement must be accepted in the Steam client.");
		}

		var id = result.FileId.Value;
		if (id != 0)
		{
			metadata.PublishedFileId = id;
		}

		return PublishOutcome.Ok(metadata.PublishedFileId);
	}

	private static Steamworks.Ugc.Editor ApplyVisibility(Steamworks.Ugc.Editor editor, string visibility)
	{
		return visibility switch
		{
			"Private" => editor.WithPrivateVisibility(),
			"FriendsOnly" => editor.WithFriendsOnlyVisibility(),
			_ => editor.WithPublicVisibility(),
		};
	}
}

public readonly record struct PublishOutcome(bool Success, ulong PublishedFileId, string Message)
{
	public static PublishOutcome Ok(ulong id) => new(true, id, "Published.");

	public static PublishOutcome Fail(string message) => new(false, 0, message);
}
