using System.Collections.ObjectModel;
using WorkshopUploader.Models;
using WorkshopUploader.Services;
using WorkshopUploader.Steam;

namespace WorkshopUploader;

[QueryProperty(nameof(ProjectPath), "ProjectPath")]
public partial class EditorPage : ContentPage
{
	private readonly WorkspaceService _workspace;
	private readonly SteamWorkshopService _steam;
	private readonly AppLogService _log;
	private readonly ObservableCollection<string> _visibilityItems = new() { "Public", "FriendsOnly", "Private" };

	private string _projectRoot = "";
	private WorkshopMetadata _metadata = new();

	public string ProjectPath
	{
		set
		{
			_projectRoot = Uri.UnescapeDataString(value);
			_ = LoadAsync();
		}
	}

	public EditorPage(WorkspaceService workspace, SteamWorkshopService steam, AppLogService log)
	{
		InitializeComponent();
		_workspace = workspace;
		_steam = steam;
		_log = log;
		VisibilityPicker.ItemsSource = _visibilityItems;
	}

	private async Task LoadAsync()
	{
		PathLabel.Text = _projectRoot;
		_metadata = _workspace.LoadMetadata(_projectRoot);
		TitleEntry.Text = _metadata.Title;
		DescriptionEditor.Text = _metadata.Description;
		VisibilityPicker.SelectedItem = _visibilityItems.Contains(_metadata.Visibility)
			? _metadata.Visibility
			: "Public";
		PreviewPathLabel.Text = Path.Combine(_projectRoot, _metadata.PreviewImageRelativePath);
		PublishedIdLabel.Text = _metadata.PublishedFileId == 0
			? "Not published yet (first publish creates a new workshop item)."
			: $"Published file id: {_metadata.PublishedFileId}";
		UpdateCounts();
		await Task.CompletedTask;
	}

	private void OnTitleChanged(object? sender, TextChangedEventArgs e) => UpdateCounts();

	private void OnDescriptionChanged(object? sender, TextChangedEventArgs e) => UpdateCounts();

	private void UpdateCounts()
	{
		var t = TitleEntry.Text?.Length ?? 0;
		var d = DescriptionEditor.Text?.Length ?? 0;
		TitleCountLabel.Text = $"{t} / {SteamConstants.MaxTitleLength}";
		DescriptionCountLabel.Text = $"{d} / {SteamConstants.MaxDescriptionLength}";
	}

	private async void OnPickPreview(object? sender, EventArgs e)
	{
		var result = await FilePicker.Default.PickAsync(new PickOptions
		{
			PickerTitle = "Preview image",
			FileTypes = FilePickerFileType.Images,
		});
		if (result is null)
		{
			return;
		}

		var dest = Path.Combine(_projectRoot, "preview.png");
		using (var src = await result.OpenReadAsync())
		using (var dst = File.Create(dest))
		{
			await src.CopyToAsync(dst);
		}

		_metadata.PreviewImageRelativePath = "preview.png";
		PreviewPathLabel.Text = dest;
		_log.Append($"Preview copied to {dest}");
	}

	private void OnSave(object? sender, EventArgs e)
	{
		try
		{
			_metadata.Title = TitleEntry.Text ?? "";
			_metadata.Description = DescriptionEditor.Text ?? "";
			_metadata.Visibility = VisibilityPicker.SelectedItem as string ?? "Public";
			_workspace.SaveMetadata(_projectRoot, _metadata);
			_log.Append($"Saved metadata for {_projectRoot}");
			DisplayAlert("Saved", "metadata.json updated.", "OK");
		}
		catch (Exception ex)
		{
			DisplayAlert("Error", ex.Message, "OK");
		}
	}

	private async void OnPublish(object? sender, EventArgs e)
	{
		try
		{
			_metadata.Title = TitleEntry.Text ?? "";
			_metadata.Description = DescriptionEditor.Text ?? "";
			_metadata.Visibility = VisibilityPicker.SelectedItem as string ?? "Public";
			_workspace.SaveMetadata(_projectRoot, _metadata);

			var content = Path.Combine(_projectRoot, "content");
			if (!Directory.Exists(content))
			{
				await DisplayAlert("Missing content", "Add a content/ folder before publishing.", "OK");
				return;
			}

			var upload = new Progress<float>(p => _log.Append($"Upload progress: {p:P0}"));
			var log = new Progress<string>(s => _log.Append(s));
			var outcome = await _steam.PublishAsync(
				_projectRoot,
				_metadata,
				content,
				upload,
				log,
				CancellationToken.None);

			if (!outcome.Success)
			{
				await DisplayAlert("Publish failed", outcome.Message, "OK");
				return;
			}

			_workspace.SaveMetadata(_projectRoot, _metadata);
			PublishedIdLabel.Text = $"Published file id: {_metadata.PublishedFileId}";
			await DisplayAlert("Published", $"Workshop file id {_metadata.PublishedFileId}", "OK");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "OK");
		}
	}
}
