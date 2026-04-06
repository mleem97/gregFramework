using System.Net.Http;

namespace WorkshopUploader;

public partial class Form1 : Form
{
    private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

    public Form1()
    {
        InitializeComponent();
    }

    private void WorkshopStub_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "Steam Workshop integration requires Steamworks SDK setup (App ID, steam_appid.txt for development, and a Steamworks partner account for uploads). See docs/Steam-Workshop-and-Tooling.md.",
            "Workshop",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private async void FetchBetas_Click(object? sender, EventArgs e)
    {
        string baseUrl = (txtBaseUrl.Text ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            txtBetasLog.Text = "Enter a base URL.";
            return;
        }

        txtBetasLog.Text = "Requesting " + baseUrl + "/api/v1/betas ...";
        btnFetchBetas.Enabled = false;
        try
        {
            using var response = await Http.GetAsync(baseUrl + "/api/v1/betas").ConfigureAwait(true);
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            txtBetasLog.Text =
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\r\n\r\n" +
                body +
                "\r\n\r\n(If 404, the DevServer endpoint is not deployed yet. See docs/devserver-betas.md.)";
        }
        catch (Exception ex)
        {
            txtBetasLog.Text = "Request failed: " + ex.Message;
        }
        finally
        {
            btnFetchBetas.Enabled = true;
        }
    }
}
