namespace WorkshopUploader;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage)
	{
		InitializeComponent();
		Items.Add(new ShellContent
		{
			Title = "Dashboard",
			Content = mainPage,
			Route = "MainPage",
		});
		Routing.RegisterRoute(nameof(EditorPage), typeof(EditorPage));
	}
}
