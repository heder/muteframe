namespace MuteFrame
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            CloseButton = new Button();
            MuteButton = new Button();
            UnmuteButton = new Button();
            textBox1 = new TextBox();
            SuspendLayout();
            // 
            // CloseButton
            // 
            CloseButton.Location = new Point(764, 628);
            CloseButton.Name = "CloseButton";
            CloseButton.Size = new Size(112, 34);
            CloseButton.TabIndex = 1;
            CloseButton.Text = "Close";
            CloseButton.UseVisualStyleBackColor = true;
            CloseButton.Click += CloseButton_Click;
            // 
            // MuteButton
            // 
            MuteButton.Location = new Point(12, 628);
            MuteButton.Name = "MuteButton";
            MuteButton.Size = new Size(112, 34);
            MuteButton.TabIndex = 2;
            MuteButton.Text = "Mute";
            MuteButton.UseVisualStyleBackColor = true;
            MuteButton.Click += MuteButton_Click;
            // 
            // UnmuteButton
            // 
            UnmuteButton.Location = new Point(130, 628);
            UnmuteButton.Name = "UnmuteButton";
            UnmuteButton.Size = new Size(112, 34);
            UnmuteButton.TabIndex = 3;
            UnmuteButton.Text = "Unmute";
            UnmuteButton.UseVisualStyleBackColor = true;
            UnmuteButton.Click += UnmuteButton_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 12);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(864, 276);
            textBox1.TabIndex = 4;
            textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(888, 672);
            Controls.Add(textBox1);
            Controls.Add(UnmuteButton);
            Controls.Add(MuteButton);
            Controls.Add(CloseButton);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "MuteFrame";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button CloseButton;
        private Button MuteButton;
        private Button UnmuteButton;
        private TextBox textBox1;
    }
}
