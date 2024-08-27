using System.Diagnostics;

namespace MuteFrame;

public partial class BorderForm : Form
{
    public BorderForm()
    {
        InitializeComponent();
        MoveToPrimaryScreen();
    }


    private const int WS_EX_NOACTIVATE = 0x08000000;
    protected override CreateParams CreateParams
    {
        get
        {
            var createParams = base.CreateParams;
            createParams.ExStyle |= WS_EX_NOACTIVATE;
            return createParams;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Pen redPen = new(Color.Red, 10);
        redPen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
        e.Graphics.DrawRectangle(redPen, new Rectangle(0, 0, Width - 1, Height - 1));
    }

    public void MoveToPrimaryScreen()
    {
        //// Find the ms-teams-process and which screen it is located on
        var ttt = Process.GetProcessesByName("ms-teams").FirstOrDefault();

        //// Get the process
        Process process = Process.GetProcessById(ttt.Id);

        //// Get the screen that the process window is located on
        Screen screen = Screen.FromHandle(process.MainWindowHandle);

        // Get the primary screen's working area
        Rectangle primaryScreenWorkingArea = screen.WorkingArea;

        // Set the form's location to the top-left corner of the primary screen
        this.Location = new Point(primaryScreenWorkingArea.Left, primaryScreenWorkingArea.Top);
        Bounds = screen.Bounds;
    }
}
