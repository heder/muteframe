namespace MuteFrame;

internal class App
{
    private readonly NotifyIcon ni = new();
    private readonly ContextMenuStrip cms = new();
    private readonly MainForm mainForm = new();
    private readonly BorderForm borderForm = new();
    private readonly Thread teamsCommThread;
    private readonly Teams teams = new();

    public App()
    {
        mainForm.t = teams;

        cms.Items.Add("Settings", null, (s, e) => { mainForm.Show(); });
        cms.Items.Add("Exit", null, (s, e) => Environment.Exit(0));

        ni.Icon = SystemIcons.Asterisk;
        ni.Visible = true;
        ni.ContextMenuStrip = cms;

        borderForm.Visible = true;
        borderForm.Hide();

        if (Settings.Default.InitialPopup ==  false)
        {
            mainForm.Show();

            Settings.Default.InitialPopup = true;
            Settings.Default.Save();
        }

        teamsCommThread = new Thread(ThreadStart);
        teamsCommThread.Start();
    }

    async void ThreadStart()
    {
        await teams.Init(borderForm);
    }
}

