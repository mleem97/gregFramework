using Microsoft.Extensions.Logging;
using WorkshopUploader.Services;

namespace WorkshopUploader;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		if (HeadlessRunner.TryHandle(Environment.GetCommandLineArgs(), out var exitCode))
		{
			Environment.Exit(exitCode);
			throw new InvalidOperationException("Unreachable: process should have exited.");
		}

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>();

		builder.Services.AddSingleton<AppLogService>();
		builder.Services.AddSingleton<WorkspaceService>();
		builder.Services.AddSingleton<SteamWorkshopService>();
		builder.Services.AddSingleton<RalphSyncService>();
		builder.Services.AddSingleton<VdfGeneratorService>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddTransient<EditorPage>();
		builder.Services.AddSingleton<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
