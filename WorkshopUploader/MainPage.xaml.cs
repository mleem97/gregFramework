using System.Collections.ObjectModel;
using WorkshopUploader.Models;
using WorkshopUploader.Services;

namespace WorkshopUploader;

public partial class MainPage : ContentPage
{
	private readonly WorkspaceService _workspace;
	private readonly AppLogService _log;
	private readonly ObservableCollection<ProjectTileVm> _projects = new();

	public MainPage(WorkspaceService workspace, AppLogService log)
	{
		InitializeComponent();
		_workspace = workspace;
		_log = log;
		ProjectList.ItemsSource = _projects;
		_workspace.EnsureWorkspaceStructure();
		WorkspacePathLabel.Text = _workspace.WorkspaceRoot;
		_log.LineAppended += OnLogAppended;
		ReloadProjects();
		_log.Append("Ready.");
	}

	protected override void OnDisappearing()
	{
		_log.LineAppended -= OnLogAppended;
		base.OnDisappearing();
	}

	private void OnLogAppended(object? sender, EventArgs e)
	{
		LogLabel.Text = string.Join(Environment.NewLine, _log.Lines);
		LogScroll.ScrollToAsync(LogLabel, ScrollToPosition.End, false);
	}

	private void ReloadProjects()
	{
		_projects.Clear();
		foreach (var p in _workspace.ScanProjects())
		{
			_projects.Add(new ProjectTileVm(p));
		}
	}

	private void OnRefreshing(object? sender, EventArgs e)
	{
		try
		{
			ReloadProjects();
			_log.Append("Workspace refreshed.");
		}
		finally
		{
			RefreshView.IsRefreshing = false;
		}
	}

	private async void OnProjectSelected(object? sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is not ProjectTileVm vm)
		{
			return;
		}

		ProjectList.SelectedItem = null;
		await Shell.Current.GoToAsync($"{nameof(EditorPage)}?ProjectPath={Uri.EscapeDataString(vm.RootPath)}");
	}

	private sealed class ProjectTileVm
	{
		public ProjectTileVm(WorkshopProject project)
		{
			Name = project.Name;
			RootPath = project.RootPath;
			StatusText = project.IsValidLayout ? "content/ present" : "Missing content/ — add folder to upload";
		}

		public string Name { get; }

		public string RootPath { get; }

		public string StatusText { get; }
	}
}
