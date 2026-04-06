namespace WorkshopUploader;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        tabControl1 = new TabControl();
        tabWorkshop = new TabPage();
        lblWorkshopInfo = new Label();
        btnWorkshopStub = new Button();
        tabBetas = new TabPage();
        lblBaseUrl = new Label();
        txtBaseUrl = new TextBox();
        btnFetchBetas = new Button();
        txtBetasLog = new TextBox();
        tabControl1.SuspendLayout();
        tabWorkshop.SuspendLayout();
        tabBetas.SuspendLayout();
        SuspendLayout();
        //
        // tabControl1
        //
        tabControl1.Controls.Add(tabWorkshop);
        tabControl1.Controls.Add(tabBetas);
        tabControl1.Dock = DockStyle.Fill;
        tabControl1.Location = new Point(0, 0);
        tabControl1.Name = "tabControl1";
        tabControl1.SelectedIndex = 0;
        tabControl1.Size = new Size(784, 411);
        tabControl1.TabIndex = 0;
        //
        // tabWorkshop
        //
        tabWorkshop.Controls.Add(btnWorkshopStub);
        tabWorkshop.Controls.Add(lblWorkshopInfo);
        tabWorkshop.Location = new Point(4, 29);
        tabWorkshop.Name = "tabWorkshop";
        tabWorkshop.Padding = new Padding(3);
        tabWorkshop.Size = new Size(776, 378);
        tabWorkshop.TabIndex = 0;
        tabWorkshop.Text = "Steam Workshop";
        tabWorkshop.UseVisualStyleBackColor = true;
        //
        // lblWorkshopInfo
        //
        lblWorkshopInfo.Location = new Point(12, 12);
        lblWorkshopInfo.Name = "lblWorkshopInfo";
        lblWorkshopInfo.Size = new Size(740, 120);
        lblWorkshopInfo.TabIndex = 0;
        lblWorkshopInfo.Text = "Workshop upload requires Steamworks (Steam client, App ID, and publisher permissions). This build provides a placeholder until Steamworks.NET or Facepunch.Steamworks is wired in.";
        //
        // btnWorkshopStub
        //
        btnWorkshopStub.Location = new Point(12, 140);
        btnWorkshopStub.Name = "btnWorkshopStub";
        btnWorkshopStub.Size = new Size(200, 29);
        btnWorkshopStub.TabIndex = 1;
        btnWorkshopStub.Text = "Workshop actions (stub)";
        btnWorkshopStub.UseVisualStyleBackColor = true;
        btnWorkshopStub.Click += WorkshopStub_Click;
        //
        // tabBetas
        //
        tabBetas.Controls.Add(txtBetasLog);
        tabBetas.Controls.Add(btnFetchBetas);
        tabBetas.Controls.Add(txtBaseUrl);
        tabBetas.Controls.Add(lblBaseUrl);
        tabBetas.Location = new Point(4, 29);
        tabBetas.Name = "tabBetas";
        tabBetas.Padding = new Padding(3);
        tabBetas.Size = new Size(776, 378);
        tabBetas.TabIndex = 1;
        tabBetas.Text = "DevServer betas";
        tabBetas.UseVisualStyleBackColor = true;
        //
        // lblBaseUrl
        //
        lblBaseUrl.AutoSize = true;
        lblBaseUrl.Location = new Point(12, 16);
        lblBaseUrl.Name = "lblBaseUrl";
        lblBaseUrl.Size = new Size(65, 20);
        lblBaseUrl.TabIndex = 0;
        lblBaseUrl.Text = "Base URL";
        //
        // txtBaseUrl
        //
        txtBaseUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtBaseUrl.Location = new Point(100, 13);
        txtBaseUrl.Name = "txtBaseUrl";
        txtBaseUrl.Size = new Size(560, 27);
        txtBaseUrl.TabIndex = 1;
        txtBaseUrl.Text = "https://gregframework.eu";
        //
        // btnFetchBetas
        //
        btnFetchBetas.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnFetchBetas.Location = new Point(666, 11);
        btnFetchBetas.Name = "btnFetchBetas";
        btnFetchBetas.Size = new Size(94, 29);
        btnFetchBetas.TabIndex = 2;
        btnFetchBetas.Text = "Fetch";
        btnFetchBetas.UseVisualStyleBackColor = true;
        btnFetchBetas.Click += FetchBetas_Click;
        //
        // txtBetasLog
        //
        txtBetasLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtBetasLog.Location = new Point(12, 50);
        txtBetasLog.Multiline = true;
        txtBetasLog.Name = "txtBetasLog";
        txtBetasLog.ReadOnly = true;
        txtBetasLog.ScrollBars = ScrollBars.Vertical;
        txtBetasLog.Size = new Size(748, 310);
        txtBetasLog.TabIndex = 3;
        //
        // Form1
        //
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(784, 411);
        Controls.Add(tabControl1);
        MinimumSize = new Size(600, 400);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "FrikaMF Workshop Uploader";
        tabControl1.ResumeLayout(false);
        tabWorkshop.ResumeLayout(false);
        tabBetas.ResumeLayout(false);
        tabBetas.PerformLayout();
        ResumeLayout(false);
    }

    private TabControl tabControl1;
    private TabPage tabWorkshop;
    private TabPage tabBetas;
    private Label lblWorkshopInfo;
    private Button btnWorkshopStub;
    private Label lblBaseUrl;
    private TextBox txtBaseUrl;
    private Button btnFetchBetas;
    private TextBox txtBetasLog;

    #endregion
}
