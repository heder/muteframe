namespace MuteFrame
{
    public partial class MainForm : Form
    {
        public Teams t { get; set; }

        public MainForm()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MuteButton_Click(object sender, EventArgs e)
        {
            t.teams.IsMuted = true;
        }

        private void UnmuteButton_Click(object sender, EventArgs e)
        {
            t.teams.IsMuted = false;
        }
    }
}
