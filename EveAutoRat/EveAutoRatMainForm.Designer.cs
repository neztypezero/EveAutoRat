namespace EveAutoRat
{
  partial class EveAutoRatMainForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.learnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.characters20x32ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.characters32x32ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.characters64x64ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.startMiningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.startNewsRATToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.wordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // menuStrip1
      // 
      this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
      this.fileToolStripMenuItem,
      this.learnToolStripMenuItem,
      this.actionToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
      this.menuStrip1.Size = new System.Drawing.Size(1922, 24);
      this.menuStrip1.TabIndex = 0;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
      this.exitToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
      this.exitToolStripMenuItem.Text = "Exit";
      // 
      // learnToolStripMenuItem
      // 
      this.learnToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
      this.characters20x32ToolStripMenuItem,
      this.characters32x32ToolStripMenuItem,
      this.characters64x64ToolStripMenuItem,
      this.wordsToolStripMenuItem});
      this.learnToolStripMenuItem.Name = "learnToolStripMenuItem";
      this.learnToolStripMenuItem.Size = new System.Drawing.Size(48, 22);
      this.learnToolStripMenuItem.Text = "Learn";
      // 
      // characters20x32ToolStripMenuItem
      // 
      this.characters20x32ToolStripMenuItem.Name = "characters20x32ToolStripMenuItem";
      this.characters20x32ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
      this.characters20x32ToolStripMenuItem.Text = "Characters 20x32";
      this.characters20x32ToolStripMenuItem.Click += new System.EventHandler(this.characters20x32ToolStripMenuItem_Click);
      // 
      // characters32x32ToolStripMenuItem
      // 
      this.characters32x32ToolStripMenuItem.Name = "characters32x32ToolStripMenuItem";
      this.characters32x32ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
      this.characters32x32ToolStripMenuItem.Text = "Characters 32x32";
      this.characters32x32ToolStripMenuItem.Click += new System.EventHandler(this.characters32x32ToolStripMenuItem_Click);
      // 
      // characters64x64ToolStripMenuItem
      // 
      this.characters64x64ToolStripMenuItem.Name = "characters64x64ToolStripMenuItem";
      this.characters64x64ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
      this.characters64x64ToolStripMenuItem.Text = "Characters 64x64";
      this.characters64x64ToolStripMenuItem.Click += new System.EventHandler(this.characters64x64ToolStripMenuItem_Click);
      // 
      // actionToolStripMenuItem
      // 
      this.actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
      this.startMiningToolStripMenuItem,
      this.startNewsRATToolStripMenuItem});
      this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
      this.actionToolStripMenuItem.Size = new System.Drawing.Size(54, 22);
      this.actionToolStripMenuItem.Text = "Action";
      // 
      // startMiningToolStripMenuItem
      // 
      this.startMiningToolStripMenuItem.Name = "startMiningToolStripMenuItem";
      this.startMiningToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
      this.startMiningToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
      this.startMiningToolStripMenuItem.Text = "Start Mining";
      this.startMiningToolStripMenuItem.Click += new System.EventHandler(this.startMiningToolStripMenuItem_Click);
      // 
      // startNewsRATToolStripMenuItem
      // 
      this.startNewsRATToolStripMenuItem.Name = "startNewsRATToolStripMenuItem";
      this.startNewsRATToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
      this.startNewsRATToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
      this.startNewsRATToolStripMenuItem.Text = "Start News RAT";
      this.startNewsRATToolStripMenuItem.Click += new System.EventHandler(this.startNewsRATToolStripMenuItem_Click);
      // 
      // wordsToolStripMenuItem
      // 
      this.wordsToolStripMenuItem.Name = "wordsToolStripMenuItem";
      this.wordsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
      this.wordsToolStripMenuItem.Text = "Words";
      this.wordsToolStripMenuItem.Click += new System.EventHandler(this.wordsToolStripMenuItem_Click);
      // 
      // EveAutoRatMainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.ClientSize = new System.Drawing.Size(1922, 1107);
      this.Controls.Add(this.menuStrip1);
      this.DoubleBuffered = true;
      this.MainMenuStrip = this.menuStrip1;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "EveAutoRatMainForm";
      this.Text = "EveAutoRat";
      this.Paint += new System.Windows.Forms.PaintEventHandler(this.EveAutoRatMainForm_Paint);
      this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.EveAutoRatMainForm_MouseDown);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.EveAutoRatMainForm_MouseMove);
      this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.EveAutoRatMainForm_MouseUp);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.MenuStrip menuStrip1;
    private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem learnToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem actionToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem startMiningToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem characters32x32ToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem characters64x64ToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem characters20x32ToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem startNewsRATToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem wordsToolStripMenuItem;
  }
}

