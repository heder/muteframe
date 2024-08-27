using MuteFrame.TeamsApi;
using System.Diagnostics;

namespace MuteFrame;
public class Teams
{
    public BorderForm borderForm { get; set; }
    public TeamsClient teams;

    public async Task Init(BorderForm form)
    {
        borderForm = form;

        var token = Settings.Default.Token != string.Empty ? Settings.Default.Token : null;
        teams = new TeamsClient("Thomas Heder", "MuteFrame", "MuteFrame", "1.0.0", token) ?? throw new Exception("Could not create client");

        CancellationTokenSource cts = new();

        teams.TokenReceived += Teams_TokenReceived;
        teams.PropertyChanged += Teams_PropertyChanged;
        teams.Connected += Teams_Connected;
        teams.Disconnected += Teams_Disconnected;
        teams.ErrorReceived += Teams_ErrorReceived;

        await teams.Connect(true, cts.Token);
    }

    private static void Teams_Connected(object? sender, System.EventArgs e)
    {
        Debug.WriteLine("Event: Connected");
    }

    private static void Teams_Disconnected(object? sender, System.EventArgs e)
    {
        Debug.WriteLine("Event: Disconnected");
    }

    private static void Teams_ErrorReceived(object? sender, TeamsApi.EventArgs e)
    {
        Debug.WriteLine($"Event: ErrorReceived: {e.ErrorMessage}");
    }

    private void Teams_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender == null || e.PropertyName == null) return;

        Debug.WriteLine($"Event: PropertyChanged: {e.PropertyName}={sender.GetType()?.GetProperty(e.PropertyName)?.GetValue(sender, null)}");

        if (e.PropertyName == "IsMuted")
        {
            var isMuted = ((TeamsClient)sender).IsMuted;

            Debug.WriteLine($"IsMuted: {isMuted}");

            if (borderForm.InvokeRequired == true)
            {
                borderForm.Invoke(() => 
                { 
                    borderForm.Visible = isMuted;
                    borderForm.MoveToPrimaryScreen();
                });
            }
            else
            {
                borderForm.Visible = isMuted;
                borderForm.MoveToPrimaryScreen();
            }
        }
    }

    private static void Teams_TokenReceived(object? sender, TokenReceivedEventArgs e)
    {
        Debug.WriteLine("Event: TokenReceived");
        Settings.Default.Token = e.Token;
        Settings.Default.Save();
    }
}
